using System.Collections.Generic;

namespace Hyjinx.HLE.Loaders.Processes
{
    public struct TitleUpdateMetadata
    {
        public string Selected { get; set; }
        public List<string> Paths { get; set; }
    }
}