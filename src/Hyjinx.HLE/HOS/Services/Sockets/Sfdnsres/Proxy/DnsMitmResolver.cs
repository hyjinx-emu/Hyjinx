using Hyjinx.HLE.HOS.Services.Sockets.Nsd;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Net;

namespace Hyjinx.HLE.HOS.Services.Sockets.Sfdnsres.Proxy;

partial class DnsMitmResolver
{
    private const string HostsFilePath = "/atmosphere/hosts/default.txt";

    private static readonly ILogger<DnsMitmResolver> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<DnsMitmResolver>();

    private static DnsMitmResolver _instance;
    public static DnsMitmResolver Instance => _instance ??= new DnsMitmResolver();

    private readonly Dictionary<string, IPAddress> _mitmHostEntries = new();

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceBsd, EventName = nameof(LogClass.ServiceBsd),
        Message = "Invalid entry in hosts file: {line}")]
    private partial void LogInvalidEntryInHostsFile(string line);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceBsd, EventName = nameof(LogClass.ServiceBsd),
        Message = "Failed to parse IP address in hosts file: {line}")]
    private partial void LogFailedToParseIpAddress(string line);

    public void ReloadEntries(ServiceCtx context)
    {
        string sdPath = FileSystem.VirtualFileSystem.GetSdCardPath();
        string filePath = FileSystem.VirtualFileSystem.GetFullPath(sdPath, HostsFilePath);

        _mitmHostEntries.Clear();

        if (File.Exists(filePath))
        {
            using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new(fileStream);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line == null)
                {
                    break;
                }

                // Ignore comments and empty lines
                if (line.StartsWith('#') || line.Trim().Length == 0)
                {
                    continue;
                }

                string[] entry = line.Split(new[] { ' ', '\t' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                // Hosts file example entry:
                // 127.0.0.1  localhost loopback

                // 0. Check the size of the array
                if (entry.Length < 2)
                {
                    LogInvalidEntryInHostsFile(line);
                    continue;
                }

                // 1. Parse the address
                if (!IPAddress.TryParse(entry[0], out IPAddress address))
                {
                    LogFailedToParseIpAddress(entry[0]);
                    continue;
                }

                // 2. Check for AMS hosts file extension: "%"
                for (int i = 1; i < entry.Length; i++)
                {
                    entry[i] = entry[i].Replace("%", IManager.NsdSettings.Environment);
                }

                // 3. Add hostname to entry dictionary (updating duplicate entries)
                foreach (string hostname in entry[1..])
                {
                    _mitmHostEntries[hostname] = address;
                }
            }
        }
    }

    public IPHostEntry ResolveAddress(string host)
    {
        foreach (var hostEntry in _mitmHostEntries)
        {
            // Check for AMS hosts file extension: "*"
            // NOTE: MatchesSimpleExpression also allows "?" as a wildcard
            if (FileSystemName.MatchesSimpleExpression(hostEntry.Key, host))
            {
                _logger.LogInformation(new EventId((int)LogClass.ServiceBsd, nameof(LogClass.ServiceBsd)),
                    "Redirecting '{host}' to: {address}", host, hostEntry.Value);

                return new IPHostEntry
                {
                    AddressList = new[] { hostEntry.Value },
                    HostName = hostEntry.Key,
                    Aliases = Array.Empty<string>(),
                };
            }
        }

        // No match has been found, resolve the host using regular dns
        return Dns.GetHostEntry(host);
    }
}