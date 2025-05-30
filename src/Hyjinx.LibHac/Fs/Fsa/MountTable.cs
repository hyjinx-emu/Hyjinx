using LibHac.Common;
using LibHac.Diag;
using LibHac.Os;
using LibHac.Util;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace LibHac.Fs.Impl;

/// <summary>
/// Holds a list of <see cref="FileSystemAccessor"/>s that are indexed by their name.
/// These may be retrieved or removed using their name as a key.
/// </summary>
/// <remarks>Based on nnSdk 13.4.0 (FS 13.1.0)</remarks>
internal class MountTable : IDisposable
{
    private LinkedList<FileSystemAccessor> _fileSystemList;
    private SdkMutexType _mutex;

    // LibHac addition
    private FileSystemClient _fsClient;

    public MountTable(FileSystemClient fsClient)
    {
        _fileSystemList = new LinkedList<FileSystemAccessor>();
        _mutex = new SdkMutexType();

        _fsClient = fsClient;
    }

    // Note: The original class does not have a destructor
    public void Dispose()
    {
        using ScopedLock<SdkMutexType> scopedLock = ScopedLock.Lock(ref _mutex);

        LinkedListNode<FileSystemAccessor> currentEntry = _fileSystemList.First;

        while (currentEntry is not null)
        {
            FileSystemAccessor accessor = currentEntry.Value;
            _fileSystemList.Remove(currentEntry);
            accessor?.Dispose();

            currentEntry = _fileSystemList.First;
        }

        _fileSystemList = null;
        _fsClient = null;
    }

    private static bool Matches(FileSystemAccessor accessor, U8Span name)
    {
        return StringUtils.Compare(accessor.GetName(), name, Unsafe.SizeOf<MountName>()) == 0;
    }

    public Result Mount(ref UniqueRef<FileSystemAccessor> fileSystem)
    {
        using ScopedLock<SdkMutexType> scopedLock = ScopedLock.Lock(ref _mutex);

        if (!CanAcceptMountName(fileSystem.Get.GetName()))
            return ResultFs.MountNameAlreadyExists.Log();

        _fileSystemList.AddLast(fileSystem.Release());
        return Result.Success;
    }

    public Result Find(out FileSystemAccessor accessor, U8Span name)
    {
        UnsafeHelpers.SkipParamInit(out accessor);
        using ScopedLock<SdkMutexType> scopedLock = ScopedLock.Lock(ref _mutex);

        for (LinkedListNode<FileSystemAccessor> currentNode = _fileSystemList.First;
            currentNode is not null;
            currentNode = currentNode.Next)
        {
            if (Matches(currentNode.Value, name))
            {
                accessor = currentNode.Value;
                return Result.Success;
            }
        }

        return ResultFs.NotMounted.Log();
    }

    public void Unmount(U8Span name)
    {
        using ScopedLock<SdkMutexType> scopedLock = ScopedLock.Lock(ref _mutex);

        for (LinkedListNode<FileSystemAccessor> currentNode = _fileSystemList.First;
            currentNode is not null;
            currentNode = currentNode.Next)
        {
            if (Matches(currentNode.Value, name))
            {
                _fileSystemList.Remove(currentNode);
                currentNode.Value.Dispose();
                return;
            }
        }

        _fsClient.Impl.LogErrorMessage(ResultFs.NotMounted.Value,
            "Error: Unmount failed because the mount name was not mounted. The mount name is \"{0}\".\n",
            name.ToString());

        Abort.DoAbortUnlessSuccess(ResultFs.NotMounted.Value);
    }

    private bool CanAcceptMountName(U8Span name)
    {
        Assert.SdkAssert(_mutex.IsLockedByCurrentThread());

        for (LinkedListNode<FileSystemAccessor> currentNode = _fileSystemList.First;
            currentNode is not null;
            currentNode = currentNode.Next)
        {
            if (Matches(currentNode.Value, name))
                return false;
        }

        return true;
    }
}