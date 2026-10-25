using DiscordRPC;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.Extensions.Abstractions.Events;
using Hyjinx.Ava.Emulation.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Hyjinx.Ava.Localization;

namespace Hyjinx.Ava.Modules;

internal class DiscordIntegrationModule : IModule
{
    /// <summary>
    /// The application id which was registered with Discord.
    /// </summary>
    private const string ApplicationId = "1563339670648004688";

    private const int ApplicationByteLimit = 128;
    private const string Ellipsis = "…";

    private class DiscordClientService : ISupportInitialization, IEventHandler<GameStartedEvent>, IEventHandler<GameStoppedEvent>, IEventHandler<ApplicationClosingEvent>
    {
        private RichPresence? discordPresenceMain;
        private DiscordRpcClient? discordClient;

        public void Handle(ApplicationClosingEvent evnt)
        {
            discordClient?.Dispose();
        }

        public void Handle(GameStoppedEvent evnt)
        {
            discordClient?.SetPresence(discordPresenceMain);
        }

        public void Handle(GameStartedEvent evnt)
        {
            discordClient?.SetPresence(new RichPresence
            {
                Assets = new Assets
                {
                    LargeImageKey = "game",
                    LargeImageText = TruncateToByteLength(evnt.ApplicationName, ApplicationByteLimit),
                    SmallImageKey = "logo_small",
                    SmallImageText = LocaleManager.Instance[LocaleKeys.DiscordApplicationDescription],
                },
                Details = TruncateToByteLength($"{LocaleManager.Instance[LocaleKeys.DiscordPlayingLabel]} {evnt.ApplicationName}", ApplicationByteLimit),
                Timestamps = Timestamps.Now,
                Buttons =
                [
                    new Button
                    {
                        Label = "Website",
                        Url = "https://github.org/hyjinx-emu/Hyjinx",
                    },
                ],
            });
        }

        private static string TruncateToByteLength(string input, int byteLimit)
        {
            if (Encoding.UTF8.GetByteCount(input) <= byteLimit)
            {
                return input;
            }

            // Find the length to trim the string to guarantee we have space for the trailing ellipsis.
            int trimLimit = byteLimit - Encoding.UTF8.GetByteCount(Ellipsis);

            // Make sure the string is long enough to perform the basic trim.
            // Amount of bytes != Length of the string
            if (input.Length > trimLimit)
            {
                // Basic trim to best case scenario of 1 byte characters.
                input = input[..trimLimit];
            }

            while (Encoding.UTF8.GetByteCount(input) > trimLimit)
            {
                // Remove one character from the end of the string at a time.
                input = input[..^1];
            }

            return input.TrimEnd() + Ellipsis;
        }

        public void Initialize()
        {
            // Attach to the event so it knows when to change.
            ConfigurationState.Instance.EnableDiscordIntegration.Event += Update;

            discordPresenceMain = new RichPresence
            {
                Assets = new Assets
                {
                    LargeImageKey = "logo",
                    LargeImageText = LocaleManager.Instance[LocaleKeys.DiscordApplicationDescription],
                },
                Details = LocaleManager.Instance[LocaleKeys.DiscordMainMenuLabel],
                State = LocaleManager.Instance[LocaleKeys.DiscordIdlingLabel],
                Timestamps = Timestamps.Now,
                Buttons =
                [
                    new Button
                    {
                        Label = "Website",
                        Url = "https://github.org/hyjinx-emu/Hyjinx",
                    },
                ],
            };

            if (ConfigurationState.Instance.EnableDiscordIntegration.Value)
            {
                discordClient = CreateClient();
                discordClient.SetPresence(discordPresenceMain);
            }
        }

        private static DiscordRpcClient CreateClient()
        {
            var result = new DiscordRpcClient(ApplicationId);
            result.Initialize();

            return result;
        }

        private void Update(object sender, ReactiveEventArgs<bool> evnt)
        {
            if (evnt.OldValue == evnt.NewValue)
            {
                return;
            }

            // If the integration was active, disable it and unload everything.
            if (evnt.OldValue && discordClient != null)
            {
                discordClient.ClearPresence();
                discordClient.Dispose();
                discordClient = null;
            }

            // If we need to activate it and the client isn't active, initialize it.
            if (evnt.NewValue && discordClient == null)
            {
                discordClient = CreateClient();
                discordClient.SetPresence(discordPresenceMain);
            }
        }
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<DiscordClientService>();
        services.AddTransient<ISupportInitialization>(sp => sp.GetRequiredService<DiscordClientService>());

        // Attach the events.
        services.AddTransient<IEventHandler<GameStartedEvent>>(sp => sp.GetRequiredService<DiscordClientService>());
        services.AddTransient<IEventHandler<GameStoppedEvent>>(sp => sp.GetRequiredService<DiscordClientService>());
        services.AddTransient<IEventHandler<ApplicationClosingEvent>>(sp => sp.GetRequiredService<DiscordClientService>());
    }
}