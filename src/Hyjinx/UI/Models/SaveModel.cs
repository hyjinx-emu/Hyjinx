using Hyjinx.Ava.UI.ViewModels;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.HLE.FileSystem;
using Hyjinx.UI.App.Common;
using Hyjinx.UI.Common.Helper;
using LibHac.Fs;
using LibHac.Ncm;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Hyjinx.Ava.UI.Models;

public class SaveModel : BaseModel
{
    private long _size;

    public ulong SaveId { get; }
    public ProgramId TitleId { get; }
    public string TitleIdString => $"{TitleId.Value:X16}";
    public UserId UserId { get; }
    public bool InGameList { get; }
    public string Title { get; }
    public byte[] Icon { get; }

    public long Size
    {
        get => _size; set
        {
            _size = value;
            SizeAvailable = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SizeString));
            OnPropertyChanged(nameof(SizeAvailable));
        }
    }

    public bool SizeAvailable { get; set; }

    public string SizeString => ValueFormatUtils.FormatFileSize(Size);

    public SaveModel(SaveDataInfo info)
    {
        SaveId = info.SaveDataId;
        TitleId = info.ProgramId;
        UserId = info.UserId;

        var appData = MainWindow.MainWindowViewModel.Applications.FirstOrDefault(x => x.IdString.ToUpper() == TitleIdString);

        InGameList = appData != null;

        if (InGameList)
        {
            Icon = appData.Icon;
            Title = appData.Name;
        }
        else
        {
            var appMetadata = ApplicationLibrary.LoadAndSaveMetaData(TitleIdString);
            Title = appMetadata.Title ?? TitleIdString;
        }

        Task.Run(() =>
        {
            var saveRoot = Path.Combine(VirtualFileSystem.GetNandPath(), $"user/save/{info.SaveDataId:x16}");

            long totalSize = GetDirectorySize(saveRoot);

            static long GetDirectorySize(string path)
            {
                long size = 0;
                if (Directory.Exists(path))
                {
                    var directories = Directory.GetDirectories(path);
                    foreach (var directory in directories)
                    {
                        size += GetDirectorySize(directory);
                    }

                    var files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        size += new FileInfo(file).Length;
                    }
                }

                return size;
            }

            Size = totalSize;
        });

    }
}