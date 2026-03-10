using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Represents an NCA file.
/// </summary>
public abstract class Nca
{
    public NcaHeader Header { get; }

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="header">The header.</param>
    protected Nca(NcaHeader header)
    {
        Header = header;
    }

    /// <summary>
    /// Identifies whether the section exists and can be opened.
    /// </summary>
    /// <param name="type">The section to check.</param>
    /// <returns><c>true</c> if the section exists and can be opened, otherwise <c>false</c>.</returns>
    public abstract bool CanOpenSection(NcaSectionType type);

    /// <summary>
    /// Identifies whether the section exists and can be opened.
    /// </summary>
    /// <param name="index">The zero-based section index to check.</param>
    /// <returns><c>true</c> if the section exists and can be opened, otherwise <c>false</c>.</returns>
    public abstract bool CanOpenSection(int index);

    /// <summary>
    /// Opens the raw storage.
    /// </summary>
    /// <param name="index">The zero-based section index to open.</param>
    /// <returns>The <see cref="IStorage"/> containing the raw section data.</returns>
    public abstract IStorage OpenRawStorage(int index);

    /// <summary>
    /// Opens the storage.
    /// </summary>
    /// <param name="index">The zero-based section index to open.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <param name="leaveCompressed">Optional. <c>true</c> to leave compressed, otherwise <c>false</c>.</param>
    /// <returns>The <see cref="IStorage"/> containing the section data.</returns>
    public abstract IStorage OpenStorage(int index, IntegrityCheckLevel integrityCheckLevel, bool leaveCompressed = false);

    /// <summary>
    /// Opens the storage.
    /// </summary>
    /// <param name="type">The section type.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <returns>The <see cref="IStorage"/> containing the section data.</returns>
    public abstract IStorage OpenStorage(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);

    /// <summary>
    /// Opens the storage with the patch applied.
    /// </summary>
    /// <param name="patchNca">The patch NCA archive.</param>
    /// <param name="type">The section type.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <returns>The <see cref="IStorage"/> containing the section data.</returns>
    public abstract IStorage OpenStorageWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);

    /// <summary>
    /// Opens the file system.
    /// </summary>
    /// <param name="index">The zero-based section index to open.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <returns>The <see cref="IFileSystem"/> instance.</returns>
    public abstract IFileSystem OpenFileSystem(int index, IntegrityCheckLevel integrityCheckLevel);

    /// <summary>
    /// Opens the file system.
    /// </summary>
    /// <param name="type">The section type.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <returns>The <see cref="IFileSystem"/> instance.</returns>
    public abstract IFileSystem OpenFileSystem(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);

    /// <summary>
    /// Opens the file system with the patch applied.
    /// </summary>
    /// <param name="patchNca">The patch NCA archive.</param>
    /// <param name="type">The section type.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <returns>The <see cref="IFileSystem"/> instance.</returns>
    public abstract IFileSystem OpenFileSystemWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);

    /// <summary>
    /// Gets the section index from the section type.
    /// </summary>
    /// <param name="type">The section type.</param>
    /// <param name="contentType">The type of content.</param>
    /// <returns>The zero-based section index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The archive does not contain the type of section.</exception>
    public static int GetSectionIndexFromType(NcaSectionType type, NcaContentType contentType)
    {
        if (!TryGetSectionIndexFromType(type, contentType, out int index))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "NCA does not contain this section type.");
        }

        return index;
    }

    /// <summary>
    /// Tries to convert the section index from the section type.
    /// </summary>
    /// <param name="type">The section type.</param>
    /// <param name="contentType">The type of content.</param>
    /// <param name="index">Upon return, contains the zero-based section index.</param>
    /// <returns><c>true</c> if the conversion was successful, otherwise <c>false</c>.</returns>
    public static bool TryGetSectionIndexFromType(NcaSectionType type, NcaContentType contentType, out int index)
    {
        switch (type)
        {
            case NcaSectionType.Code when contentType == NcaContentType.Program:
                index = 0;
                return true;
            case NcaSectionType.Data when contentType == NcaContentType.Program:
                index = 1;
                return true;
            case NcaSectionType.Logo when contentType == NcaContentType.Program:
                index = 2;
                return true;
            case NcaSectionType.Data:
                index = 0;
                return true;
            default:
                index = 0;
                return false;
        }
    }

    /// <summary>
    /// Gets the section type from the section index.
    /// </summary>
    /// <param name="index">The zero-based section index.</param>
    /// <param name="contentType">The type of content.</param>
    /// <returns>The <see cref="NcaSectionType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The archive does not contain the index.</exception>
    public static NcaSectionType GetSectionTypeFromIndex(int index, NcaContentType contentType)
    {
        if (!TryGetSectionTypeFromIndex(index, contentType, out NcaSectionType type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "NCA type does not contain this index.");
        }

        return type;
    }

    /// <summary>
    /// Tries to convert the section type from the section index.
    /// </summary>
    /// <param name="index">The zero-based section index.</param>
    /// <param name="contentType">The type of content.</param>
    /// <param name="type">Upon returned, the <see cref="NcaSectionType"/>.</param>
    /// <returns><c>true</c> if the conversion was successful, otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The archive does not contain the index.</exception>
    public static bool TryGetSectionTypeFromIndex(int index, NcaContentType contentType, out NcaSectionType type)
    {
        switch (index)
        {
            case 0 when contentType == NcaContentType.Program:
                type = NcaSectionType.Code;
                return true;
            case 1 when contentType == NcaContentType.Program:
                type = NcaSectionType.Data;
                return true;
            case 2 when contentType == NcaContentType.Program:
                type = NcaSectionType.Logo;
                return true;
            case 0:
                type = NcaSectionType.Data;
                return true;
            default:
                type = default;
                return false;
        }
    }
}