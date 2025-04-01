using Hyjinx.Common.Configuration;
using Hyjinx.Logging.Abstractions;
using Hyjinx.HLE.HOS.Services.Account.Acc.Types;
using Hyjinx.HLE.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Hyjinx.HLE.HOS.Services.Account.Acc
{
    class AccountSaveDataManager
    {
        private readonly ILogger<AccountSaveDataManager> _logger = Logger.DefaultLoggerFactory.CreateLogger<AccountSaveDataManager>();
        private readonly string _profilesJsonPath = Path.Join(AppDataManager.KeysDirPath, "Profiles.json");
        
        private static readonly ProfilesJsonSerializerContext _serializerContext = new(JsonHelper.GetDefaultSerializerOptions());

        public UserId LastOpened { get; set; }

        public AccountSaveDataManager(ConcurrentDictionary<string, UserProfile> profiles)
        {
            // TODO: Use 0x8000000000000010 system savedata instead of a JSON file if needed.

            if (File.Exists(_profilesJsonPath))
            {
                try
                {
                    ProfilesJson profilesJson = JsonHelper.DeserializeFromFile(_profilesJsonPath, _serializerContext.ProfilesJson);

                    foreach (var profile in profilesJson.Profiles)
                    {
                        UserProfile addedProfile = new(new UserId(profile.UserId), profile.Name, profile.Image, profile.LastModifiedTimestamp);

                        profiles.AddOrUpdate(profile.UserId, addedProfile, (key, old) => addedProfile);
                    }

                    LastOpened = new UserId(profilesJson.LastOpened);
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                        ex, "Failed to parse {profilesJsonPath}, loading default profile!",
                        _profilesJsonPath);

                    LastOpened = AccountManager.DefaultUserId;
                }
            }
            else
            {
                LastOpened = AccountManager.DefaultUserId;
            }
        }

        public void Save(ConcurrentDictionary<string, UserProfile> profiles)
        {
            ProfilesJson profilesJson = new()
            {
                Profiles = new List<UserProfileJson>(),
                LastOpened = LastOpened.ToString(),
            };

            foreach (var profile in profiles)
            {
                profilesJson.Profiles.Add(new UserProfileJson()
                {
                    UserId = profile.Value.UserId.ToString(),
                    Name = profile.Value.Name,
                    AccountState = profile.Value.AccountState,
                    OnlinePlayState = profile.Value.OnlinePlayState,
                    LastModifiedTimestamp = profile.Value.LastModifiedTimestamp,
                    Image = profile.Value.Image,
                });
            }

            JsonHelper.SerializeToFile(_profilesJsonPath, profilesJson, _serializerContext.ProfilesJson);
        }
    }
}
