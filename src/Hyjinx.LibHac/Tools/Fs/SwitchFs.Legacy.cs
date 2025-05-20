#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;

namespace LibHac.Tools.Fs;

partial class SwitchFs
{
    public static SwitchFs OpenSdCard(KeySet keySet, ref UniqueRef<IAttributeFileSystem> fileSystem)
    {
        var concatFs = new ConcatenationFileSystem(ref fileSystem);

        using var contentDirPath = new LibHac.Fs.Path();
        PathFunctions.SetUpFixedPath(ref contentDirPath.Ref(), "/Nintendo/Contents"u8).ThrowIfFailure();

        using var saveDirPath = new LibHac.Fs.Path();
        PathFunctions.SetUpFixedPath(ref saveDirPath.Ref(), "/Nintendo/save"u8).ThrowIfFailure();

        var contentDirFs = new SubdirectoryFileSystem(concatFs);
        contentDirFs.Initialize(in contentDirPath).ThrowIfFailure();

        AesXtsFileSystem encSaveFs = null;
        if (concatFs.DirectoryExists("/Nintendo/save"))
        {
            var saveDirFs = new SubdirectoryFileSystem(concatFs);
            saveDirFs.Initialize(in saveDirPath).ThrowIfFailure();

            encSaveFs = new AesXtsFileSystem(saveDirFs, keySet.SdCardEncryptionKeys[0].DataRo.ToArray(), 0x4000);
        }

        var encContentFs = new AesXtsFileSystem(contentDirFs, keySet.SdCardEncryptionKeys[1].DataRo.ToArray(), 0x4000);

        return new SwitchFs(keySet, encContentFs, encSaveFs);
    }

    public static SwitchFs OpenNandPartition(KeySet keySet, ref UniqueRef<IAttributeFileSystem> fileSystem)
    {
        var concatFs = new ConcatenationFileSystem(ref fileSystem);
        SubdirectoryFileSystem saveDirFs = null;
        SubdirectoryFileSystem contentDirFs;

        if (concatFs.DirectoryExists("/save"))
        {
            using var savePath = new LibHac.Fs.Path();
            PathFunctions.SetUpFixedPath(ref savePath.Ref(), "/save"u8);

            saveDirFs = new SubdirectoryFileSystem(concatFs);
            saveDirFs.Initialize(in savePath).ThrowIfFailure();
        }

        using var contentsPath = new LibHac.Fs.Path();
        PathFunctions.SetUpFixedPath(ref contentsPath.Ref(), "/Contents"u8);

        contentDirFs = new SubdirectoryFileSystem(concatFs);
        contentDirFs.Initialize(in contentsPath).ThrowIfFailure();

        return new SwitchFs(keySet, contentDirFs, saveDirFs);
    }

    public static SwitchFs OpenNcaDirectory(KeySet keySet, IFileSystem fileSystem)
    {
        return new SwitchFs(keySet, fileSystem, null);
    }
}

#endif