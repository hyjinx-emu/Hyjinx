using Ryujinx.Ava.Modules.Compatibility;
using Ryujinx.UI.App.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class CompatibilityViewModel : BaseModel
    {
        private bool _onlyShowOwnedGames = true;

        public bool OnlyShowOwnedGames
        {
            get => _onlyShowOwnedGames;
            set
            {
                _onlyShowOwnedGames = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentEntries));
            }
        }

        private IEnumerable<CompatibilityEntry> _currentEntries = CompatibilityCsv.Entries;
        private readonly string[] _ownedGameTitleIds = [];

        public IEnumerable<CompatibilityEntry> CurrentEntries => OnlyShowOwnedGames
            ? _currentEntries.Where(x => x.MatchesAnyId(_ownedGameTitleIds))
            : _currentEntries;

        public CompatibilityViewModel() {}

        public CompatibilityViewModel(ObservableCollection<ApplicationData> appLibraryApps)
        {
            _ownedGameTitleIds = appLibraryApps.Select(x => x.Id.ToString("X16")).ToArray();
        }

        public void Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                SetEntries(CompatibilityCsv.Entries);
                return;
            }

            SetEntries(CompatibilityCsv.Entries.Where(x =>
                x.GameName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || x.NearMatchesId(searchTerm)));
        }

        private void SetEntries(IEnumerable<CompatibilityEntry> entries)
        {
            _currentEntries = entries.ToList();
            OnPropertyChanged(nameof(CurrentEntries));
        }
    }
}
