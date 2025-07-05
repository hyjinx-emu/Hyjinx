namespace LibHac.Fs.Fsa;

public interface IAttributeFileSystem : IFileSystem
{
    Result CreateDirectory(in Path path, NxFileAttributes archiveAttribute);
    
    Result GetFileAttributes(out NxFileAttributes attributes, in Path path);
    
    Result SetFileAttributes(in Path path, NxFileAttributes attributes);
    
    Result GetFileSize(out long fileSize, in Path path);
}