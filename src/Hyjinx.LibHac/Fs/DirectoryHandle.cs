using LibHac.Fs.Fsa;
using System;

namespace LibHac.Fs;

public readonly struct DirectoryHandle : IDisposable
{
    internal readonly Impl.DirectoryAccessor Directory;

    public bool IsValid => Directory is not null;

    internal DirectoryHandle(Impl.DirectoryAccessor directory)
    {
        Directory = directory;
    }

    public void Dispose()
    {
        if (IsValid)
        {
            Directory.GetParent().Hos.Fs.CloseDirectory(this);
        }
    }
}