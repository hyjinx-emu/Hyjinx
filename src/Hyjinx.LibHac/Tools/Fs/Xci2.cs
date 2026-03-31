using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using System;
using System.IO;
using static LibHac.Tools.Fs.NativeTypes;

namespace LibHac.Tools.Fs;

/// <summary>
/// Provides a mechanism to interact with executable cartridge image (XCI) files.
/// </summary>
public class Xci2 : Xci
{
    /// <summary>
    /// The root file system.
    /// </summary>
    private IFileSystem RootFileSystem { get; }

    /// <summary>
    /// Gets the header.
    /// </summary>
    public new XciHeader2 Header => (XciHeader2)base.Header;

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The storage to use.</param>
    /// <param name="header">The header.</param>
    /// <param name="rootFileSystem">The root file system.</param>
    protected Xci2(IStorage baseStorage, XciHeader header, IFileSystem rootFileSystem)
        : base(baseStorage, header)
    {
        RootFileSystem = rootFileSystem;
    }

    /// <summary>
    /// Creates an <see cref="Xci2"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to use.</param>
    /// <returns>The new <see cref="Xci2"/> file.</returns>
    public static Xci2 Create(Stream stream)
    {
        var storage = StreamStorage2.Create(stream);

        try
        {
            return Create(storage);
        }
        catch (Exception)
        {
            storage.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates an <see cref="Xci2"/>.
    /// </summary>
    /// <param name="storage">The <see cref="IStorage"/> to use.</param>
    /// <returns>The new <see cref="Xci2"/> file.</returns>
    public static Xci2 Create(IStorage storage)
    {
        var header = XciHeader2.Create(storage);
        if (!header.Magic.Span.SequenceEqual(HeaderSignature))
        {
            throw new ArgumentException("The storage does not contain the expected header.", nameof(storage));
        }

        storage.GetSize(out var storageSize).ThrowIfFailure();

        try
        {
            var rootFs = Sha256PartitionFileSystem2.Create(
                storage.Slice2(header.RootPartitionOffset, storageSize - header.RootPartitionOffset));

            return new Xci2(storage, header, rootFs);
        }
        catch (Exception)
        {
            storage.Dispose();
            throw;
        }
    }

    public override bool HasPartition(XciPartitionType type)
    {
        return RootFileSystem.FileExists($"/{type.GetFileName()}");
    }

    public override IFileSystem OpenPartition(XciPartitionType type)
    {
        if (type == XciPartitionType.Root)
        {
            return RootFileSystem;
        }

        using var fileRef = new UniqueRef<IFile>();
        RootFileSystem.OpenFile(ref fileRef.Ref, $"/{type.GetFileName()}".ToU8Span(), OpenMode.Read).ThrowIfFailure();

        var stream = fileRef.Get.AsStream();

        try
        {
            var storage = StreamStorage2.Create(stream);

            return Sha256PartitionFileSystem2.Create(storage);
        }
        catch (Exception)
        {
            stream.Dispose();
            throw;
        }
    }
}