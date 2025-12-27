using LibHac.Common;
using System;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Provides an interface for accessing a file system. <c>/</c> is used as the path delimiter.
/// </summary>
/// <remarks>Based on nnSdk 13.4.0 (FS 13.1.0)</remarks>
public interface IFileSystem : IDisposable
{
    /// <summary>
    /// Creates or overwrites a file at the specified path.
    /// </summary>
    /// <param name="path">The full path of the file to create.</param>
    /// <param name="size">The initial size of the created file.</param>
    /// <param name="option">Flags to control how the file is created.
    /// Should usually be <see cref="CreateFileOptions.None"/></param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The parent directory of the specified path does not exist.<br/>
    /// <see cref="ResultFs.PathAlreadyExists"/>: Specified path already exists as either a file or directory.<br/>
    /// <see cref="ResultFs.UsableSpaceNotEnough"/>: Insufficient free space to create the file.</returns>
    Result CreateFile(in Path path, long size, CreateFileOptions option);

    /// <summary>
    /// Creates or overwrites a file at the specified path.
    /// </summary>
    /// <param name="path">The full path of the file to create.</param>
    /// <param name="size">The initial size of the created file.
    /// Should usually be <see cref="CreateFileOptions.None"/></param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The parent directory of the specified path does not exist.<br/>
    /// <see cref="ResultFs.PathAlreadyExists"/>: Specified path already exists as either a file or directory.<br/>
    /// <see cref="ResultFs.UsableSpaceNotEnough"/>: Insufficient free space to create the file.</returns>
    Result CreateFile(in Path path, long size);

    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="path">The full path of the file to delete.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist or is a directory.</returns>
    Result DeleteFile(in Path path);

    /// <summary>
    /// Creates all directories and subdirectories in the specified path unless they already exist.
    /// </summary>
    /// <param name="path">The full path of the directory to create.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The parent directory of the specified path does not exist.<br/>
    /// <see cref="ResultFs.PathAlreadyExists"/>: Specified path already exists as either a file or directory.<br/>
    /// <see cref="ResultFs.UsableSpaceNotEnough"/>: Insufficient free space to create the directory.</returns>
    Result CreateDirectory(in Path path);

    /// <summary>
    /// Deletes the specified directory.
    /// </summary>
    /// <param name="path">The full path of the directory to delete.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist or is a file.<br/>
    /// <see cref="ResultFs.DirectoryNotEmpty"/>: The specified directory is not empty.</returns>
    Result DeleteDirectory(in Path path);

    /// <summary>
    /// Deletes the specified directory and any subdirectories and files in the directory.
    /// </summary>
    /// <param name="path">The full path of the directory to delete.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist or is a file.</returns>
    Result DeleteDirectoryRecursively(in Path path);

    /// <summary>
    /// Deletes any subdirectories and files in the specified directory.
    /// </summary>
    /// <param name="path">The full path of the directory to clean.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist or is a file.</returns>
    Result CleanDirectoryRecursively(in Path path);

    /// <summary>
    /// Renames or moves a file to a new location.
    /// </summary>
    /// <param name="currentPath">The current full path of the file to rename.</param>
    /// <param name="newPath">The new full path of the file.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: <paramref name="currentPath"/> does not exist or is a directory.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: <paramref name="newPath"/>'s parent directory does not exist.<br/>
    /// <see cref="ResultFs.PathAlreadyExists"/>: <paramref name="newPath"/> already exists as either a file or directory.</returns>
    /// <remarks>
    /// If <paramref name="currentPath"/> and <paramref name="newPath"/> are the same, this function does nothing and returns successfully.
    /// </remarks>
    Result RenameFile(in Path currentPath, in Path newPath);

    /// <summary>
    /// Renames or moves a directory to a new location.
    /// </summary>
    /// <param name="currentPath">The full path of the directory to rename.</param>
    /// <param name="newPath">The new full path of the directory.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: <paramref name="currentPath"/> does not exist or is a file.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: <paramref name="newPath"/>'s parent directory does not exist.<br/>
    /// <see cref="ResultFs.PathAlreadyExists"/>: <paramref name="newPath"/> already exists as either a file or directory.<br/>
    /// <see cref="ResultFs.DirectoryUnrenamable"/>: Either <paramref name="currentPath"/> or <paramref name="newPath"/> is a subpath of the other.</returns>
    /// <remarks>
    /// If <paramref name="currentPath"/> and <paramref name="newPath"/> are the same, this function does nothing and returns <see cref="Result.Success"/>.
    /// </remarks>
    Result RenameDirectory(in Path currentPath, in Path newPath);

    /// <summary>
    /// Determines whether the specified path is a file or directory, or does not exist.
    /// </summary>
    /// <param name="entryType">If the operation returns successfully, contains the <see cref="DirectoryEntryType"/> of the file.</param>
    /// <param name="path">The full path to check.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist.</returns>
    Result GetEntryType(out DirectoryEntryType entryType, in Path path);

    /// <summary>
    /// Determines whether the specified path is a file or directory, or does not exist.
    /// </summary>
    /// <param name="entryType">If the operation returns successfully, contains the <see cref="DirectoryEntryType"/> of the file.</param>
    /// <param name="path">The full path to check.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist.</returns>
    Result GetEntryType(out DirectoryEntryType entryType, U8Span path);

    /// <summary>
    /// Opens an <see cref="IFile"/> instance for the specified path.
    /// </summary>
    /// <param name="file">If the operation returns successfully,
    /// An <see cref="IFile"/> instance for the specified path.</param>
    /// <param name="path">The full path of the file to open.</param>
    /// <param name="mode">Specifies the access permissions of the created <see cref="IFile"/>.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist or is a directory.<br/>
    /// <see cref="ResultFs.TargetLocked"/>: When opening as <see cref="OpenMode.Write"/>,
    /// the file is already opened as <see cref="OpenMode.Write"/>.</returns>
    Result OpenFile(ref UniqueRef<IFile> file, in Path path, OpenMode mode);

    /// <summary>
    /// Opens an <see cref="IFile"/> instance for the specified path.
    /// </summary>
    /// <param name="file">If the operation returns successfully,
    /// An <see cref="IFile"/> instance for the specified path.</param>
    /// <param name="path">The full path of the file to open.</param>
    /// <param name="mode">Specifies the access permissions of the created <see cref="IFile"/>.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist or is a directory.<br/>
    /// <see cref="ResultFs.TargetLocked"/>: When opening as <see cref="OpenMode.Write"/>,
    /// the file is already opened as <see cref="OpenMode.Write"/>.</returns>
    Result OpenFile(ref UniqueRef<IFile> file, U8Span path, OpenMode mode);

    /// <summary>
    /// Creates an <see cref="IDirectory"/> instance for enumerating the specified directory.
    /// </summary>
    /// <param name="outDirectory"></param>
    /// <param name="path">The directory's full path.</param>
    /// <param name="mode">Specifies which sub-entries should be enumerated.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist or is a file.</returns>
    Result OpenDirectory(ref UniqueRef<IDirectory> outDirectory, in Path path, OpenDirectoryMode mode);

    /// <summary>
    /// Commits any changes to a transactional file system.
    /// Does nothing if called on a non-transactional file system.
    /// </summary>
    /// <returns>The <see cref="Result"/> of the requested operation.</returns>
    Result Commit();

    Result CommitProvisionally(long counter);
    Result Rollback();
    Result Flush();

    /// <summary>
    /// Gets the amount of available free space on a drive, in bytes.
    /// </summary>
    /// <param name="freeSpace">If the operation returns successfully, the amount of free space available on the drive, in bytes.</param>
    /// <param name="path">The path of the drive to query. Unused in almost all cases.</param>
    /// <returns>The <see cref="Result"/> of the requested operation.</returns>
    Result GetFreeSpaceSize(out long freeSpace, in Path path);

    /// <summary>
    /// Gets the total size of storage space on a drive, in bytes.
    /// </summary>
    /// <param name="totalSpace">If the operation returns successfully, the total size of the drive, in bytes.</param>
    /// <param name="path">The path of the drive to query. Unused in almost all cases.</param>
    /// <returns>The <see cref="Result"/> of the requested operation.</returns>
    Result GetTotalSpaceSize(out long totalSpace, in Path path);

    /// <summary>
    /// Gets the creation, last accessed, and last modified timestamps of a file or directory.
    /// </summary>
    /// <param name="timeStamp">If the operation returns successfully, the timestamps for the specified file or directory.
    /// These value are expressed as Unix timestamps.</param>
    /// <param name="path">The path of the file or directory.</param>
    /// <returns><see cref="Result.Success"/>: The operation was successful.<br/>
    /// <see cref="ResultFs.PathNotFound"/>: The specified path does not exist.</returns>
    Result GetFileTimeStampRaw(out FileTimeStampRaw timeStamp, in Path path);

    /// <summary>
    /// Performs a query on the specified file.
    /// </summary>
    /// <remarks>This method allows implementers of <see cref="IFileSystem"/> to accept queries and operations
    /// not included in the IFileSystem interface itself.</remarks>
    /// <param name="outBuffer">The buffer for receiving data from the query operation.
    /// May be unused depending on the query type.</param>
    /// <param name="inBuffer">The buffer for sending data to the query operation.
    /// May be unused depending on the query type.</param>
    /// <param name="queryId">The type of query to perform.</param>
    /// <param name="path">The full path of the file to query.</param>
    /// <returns>The <see cref="Result"/> of the requested operation.</returns>
    Result QueryEntry(Span<byte> outBuffer, ReadOnlySpan<byte> inBuffer, QueryId queryId, in Path path);

    /// <summary>
    /// Gets attributes of the <see cref="IFileSystem"/> including info about the maximum path length sizes it supports.
    /// </summary>
    /// <param name="outAttribute">If the operation returns successfully, the file system attributes.</param>
    /// <returns>The <see cref="Result"/> of the requested operation.</returns>
    Result GetFileSystemAttribute(out FileSystemAttribute outAttribute);
}