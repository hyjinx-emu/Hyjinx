using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Hyjinx.Ava.Common.Locale;
using Hyjinx.Ava.UI.Helpers;
using Hyjinx.Ava.UI.Models;
using Hyjinx.Common.Configuration;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Common.Utilities;
using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.Loaders.Processes.Extensions;
using Hyjinx.HLE.Utilities;
using Hyjinx.UI.App.Common;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application = Avalonia.Application;
using ContentType = LibHac.Ncm.ContentType;
using Path = System.IO.Path;
using SpanHelpers = LibHac.Common.SpanHelpers;

namespace Hyjinx.Ava.UI.ViewModels
{
    public class TitleUpdateViewModel : BaseModel
    {
        public TitleUpdateMetadata TitleUpdateWindowData;
        public readonly string TitleUpdateJsonPath;
        private VirtualFileSystem VirtualFileSystem { get; }
        private ApplicationData ApplicationData { get; }

        private AvaloniaList<TitleUpdateModel> _titleUpdates = new();
        private AvaloniaList<object> _views = new();
        private object _selectedUpdate;

        private static readonly TitleUpdateMetadataJsonSerializerContext _serializerContext = new(JsonHelper.GetDefaultSerializerOptions());
        private static readonly ILogger<TitleUpdateViewModel> _logger = 
            Logger.DefaultLoggerFactory.CreateLogger<TitleUpdateViewModel>();
        
        public AvaloniaList<TitleUpdateModel> TitleUpdates
        {
            get => _titleUpdates;
            set
            {
                _titleUpdates = value;
                OnPropertyChanged();
            }
        }

        public AvaloniaList<object> Views
        {
            get => _views;
            set
            {
                _views = value;
                OnPropertyChanged();
            }
        }

        public object SelectedUpdate
        {
            get => _selectedUpdate;
            set
            {
                _selectedUpdate = value;
                OnPropertyChanged();
            }
        }

        public IStorageProvider StorageProvider;

        public TitleUpdateViewModel(VirtualFileSystem virtualFileSystem, ApplicationData applicationData)
        {
            VirtualFileSystem = virtualFileSystem;

            ApplicationData = applicationData;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                StorageProvider = desktop.MainWindow.StorageProvider;
            }

            TitleUpdateJsonPath = Path.Combine(AppDataManager.GamesDirPath, ApplicationData.IdBaseString, "updates.json");

            try
            {
                TitleUpdateWindowData = JsonHelper.DeserializeFromFile(TitleUpdateJsonPath, _serializerContext.TitleUpdateMetadata);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId((int)LogClass.Application, nameof(LogClass.Application)), ex,
                "Failed to deserialize title update data for {idBaseString} at {path}", applicationData.IdBaseString, TitleUpdateJsonPath);

                TitleUpdateWindowData = new TitleUpdateMetadata
                {
                    Selected = "",
                    Paths = new List<string>(),
                };

                Save();
            }

            LoadUpdates();
        }

        private void LoadUpdates()
        {
            // Try to load updates from PFS first
            AddUpdate(ApplicationData.Path, true);

            foreach (string path in TitleUpdateWindowData.Paths)
            {
                AddUpdate(path);
            }

            TitleUpdateModel selected = TitleUpdates.FirstOrDefault(x => x.Path == TitleUpdateWindowData.Selected, null);

            SelectedUpdate = selected;

            // NOTE: Save the list again to remove leftovers.
            Save();
            SortUpdates();
        }

        public void SortUpdates()
        {
            var sortedUpdates = TitleUpdates.OrderByDescending(update => update.Version);

            Views.Clear();
            Views.Add(new BaseModel());
            Views.AddRange(sortedUpdates);

            if (SelectedUpdate == null)
            {
                SelectedUpdate = Views[0];
            }
            else if (!TitleUpdates.Contains(SelectedUpdate))
            {
                if (Views.Count > 1)
                {
                    SelectedUpdate = Views[1];
                }
                else
                {
                    SelectedUpdate = Views[0];
                }
            }
        }

        private void AddUpdate(string path, bool ignoreNotFound = false, bool selected = false)
        {
            if (!File.Exists(path) || TitleUpdates.Any(x => x.Path == path))
            {
                return;
            }

            IntegrityCheckLevel checkLevel = ConfigurationState.Instance.System.EnableFsIntegrityChecks
                ? IntegrityCheckLevel.ErrorOnInvalid
                : IntegrityCheckLevel.None;

            try
            {
                using IFileSystem pfs = PartitionFileSystemUtils.OpenApplicationFileSystem(path, VirtualFileSystem);

                Dictionary<ulong, ContentMetaData> updates = pfs.GetContentData(ContentMetaType.Patch, VirtualFileSystem, checkLevel);

                Nca patchNca = null;
                Nca controlNca = null;

                if (updates.TryGetValue(ApplicationData.Id, out ContentMetaData content))
                {
                    patchNca = content.GetNcaByType(VirtualFileSystem.KeySet, ContentType.Program);
                    controlNca = content.GetNcaByType(VirtualFileSystem.KeySet, ContentType.Control);
                }

                if (controlNca != null && patchNca != null)
                {
                    ApplicationControlProperty controlData = new();

                    using UniqueRef<IFile> nacpFile = new();

                    controlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None).OpenFile(ref nacpFile.Ref, "/control.nacp".ToU8Span(), OpenMode.Read).ThrowIfFailure();
                    nacpFile.Get.Read(out _, 0, SpanHelpers.AsByteSpan(ref controlData), ReadOption.None).ThrowIfFailure();

                    var displayVersion = controlData.DisplayVersionString.ToString();
                    var update = new TitleUpdateModel(content.Version.Version, displayVersion, path);

                    TitleUpdates.Add(update);

                    if (selected)
                    {
                        Dispatcher.UIThread.InvokeAsync(() => SelectedUpdate = update);
                    }
                }
                else
                {
                    if (!ignoreNotFound)
                    {
                        Dispatcher.UIThread.InvokeAsync(() => ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogUpdateAddUpdateErrorMessage]));
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.InvokeAsync(() => ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogLoadFileErrorMessage, ex.Message, path)));
            }
        }

        public void RemoveUpdate(TitleUpdateModel update)
        {
            TitleUpdates.Remove(update);

            SortUpdates();
        }

        public async Task Add()
        {
            var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new(LocaleManager.Instance[LocaleKeys.AllSupportedFormats])
                    {
                        Patterns = new[] { "*.nsp" },
                        AppleUniformTypeIdentifiers = new[] { "com.ryujinx.nsp" },
                        MimeTypes = new[] { "application/x-nx-nsp" },
                    },
                },
            });

            foreach (var file in result)
            {
                AddUpdate(file.Path.LocalPath, selected: true);
            }

            SortUpdates();
        }

        public void Save()
        {
            TitleUpdateWindowData.Paths.Clear();
            TitleUpdateWindowData.Selected = "";

            foreach (TitleUpdateModel update in TitleUpdates)
            {
                TitleUpdateWindowData.Paths.Add(update.Path);

                if (update == SelectedUpdate)
                {
                    TitleUpdateWindowData.Selected = update.Path;
                }
            }

            JsonHelper.SerializeToFile(TitleUpdateJsonPath, TitleUpdateWindowData, _serializerContext.TitleUpdateMetadata);
        }
    }
}
