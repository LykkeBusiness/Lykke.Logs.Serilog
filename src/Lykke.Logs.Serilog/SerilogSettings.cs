using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using Serilog.Events;

namespace Lykke.Logs.Serilog
{
    [UsedImplicitly]
    public class SerilogSettings
    {
        public LogEventLevel MinimumLevel { get; set; }
        public string WriteToName { get; set; }
        public string LogPath { get; set; }
        public string Application { get; set; }
        [Optional, CanBeNull]
        public string LogName { get; set; }
        
        public IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            return new Collection<KeyValuePair<string, string>>()
                .Append(new KeyValuePair<string, string>("serilog:minimumLevel.default", MinimumLevel.ToString()))
                .Append(new KeyValuePair<string, string>("serilog:writeTo:Name", "Async"))
                .Append(new KeyValuePair<string, string>("serilog:writeTo:Args:configure:Name", WriteToName))
                .Append(new KeyValuePair<string, string>("serilog:writeTo:Args:configure:Args:pathFormat", LogPath))
                .Append(new KeyValuePair<string, string>("serilog:Properties:Application", Application))
                .Append(new KeyValuePair<string, string>("serilog:Properties:LogName", LogName));
        }
    }
}