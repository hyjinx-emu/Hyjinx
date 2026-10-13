#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Diag;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.Sf;
using LibHac.Os;
using LibHac.Sf;
using LibHac.Util;
using System;
using IDirectory = LibHac.Fs.Fsa.IDirectory;
using IFile = LibHac.Fs.Fsa.IFile;
using IFileSystem = LibHac.Fs.Fsa.IFileSystem;
using Path = LibHac.Fs.Path;
namespace LibHac.FsSystem;

partial class Utility
{
    public static Result CleanupDirectoryRecursively(IFileSystem fs, in Path rootPath, ref DirectoryEntry dirEntry,
        FsIterationTask onEnterDir, FsIterationTask onExitDir, FsIterationTask onFile,
        ref FsIterationTaskClosure closure)
    {
        using var pathBuffer = new Path();
        Result res = pathBuffer.Initialize(in rootPath);
        if (res.IsFailure())
            return res.Miss();

        return CleanupDirectoryRecursivelyInternal(fs, ref pathBuffer.Ref(), ref dirEntry, onEnterDir, onExitDir, onFile,
            ref closure);
    }

    public static Result CopyDirectoryRecursively(IFileSystem destinationFileSystem, IFileSystem sourceFileSystem,
        in Path destinationPath, in Path sourcePath, ref DirectoryEntry dirEntry, Span<byte> workBuffer)
    {
        static Result OnEnterDir(in Path path, in DirectoryEntry entry, ref FsIterationTaskClosure closure)
        {
            Result res = closure.DestinationPathBuffer.AppendChild(entry.Name);
            if (res.IsFailure())
                return res.Miss();

            return closure.SourceFileSystem.CreateDirectory(in closure.DestinationPathBuffer);
        }

        static Result OnExitDir(in Path path, in DirectoryEntry entry, ref FsIterationTaskClosure closure)
        {
            return closure.DestinationPathBuffer.RemoveChild();
        }

        static Result OnFile(in Path path, in DirectoryEntry entry, ref FsIterationTaskClosure closure)
        {
            Result res = closure.DestinationPathBuffer.AppendChild(entry.Name);
            if (res.IsFailure())
                return res.Miss();

            res = CopyFile(closure.DestFileSystem, closure.SourceFileSystem, in closure.DestinationPathBuffer,
                in path, closure.Buffer);
            if (res.IsFailure())
                return res.Miss();

            return closure.DestinationPathBuffer.RemoveChild();
        }

        var closure = new FsIterationTaskClosure();
        closure.Buffer = workBuffer;
        closure.SourceFileSystem = sourceFileSystem;
        closure.DestFileSystem = destinationFileSystem;

        Result res = closure.DestinationPathBuffer.Initialize(destinationPath);
        if (res.IsFailure())
            return res.Miss();

        res = IterateDirectoryRecursively(sourceFileSystem, in sourcePath, ref dirEntry, OnEnterDir, OnExitDir,
            OnFile, ref closure);

        closure.DestinationPathBuffer.Dispose();
        return res;
    }

    public static void AddCounter(Span<byte> counter, ulong value)
    {
        const int bitsPerByte = 8;

        ulong remaining = value;
        byte carry = 0;

        for (int i = 0; i < counter.Length; i++)
        {
            int sum = counter[counter.Length - 1 - i] + (byte)remaining + carry;
            carry = (byte)(sum >> bitsPerByte);

            counter[counter.Length - 1 - i] = (byte)sum;

            remaining >>= bitsPerByte;

            if (carry == 0 && remaining == 0)
                break;
        }
    }
}

#endif