#if IS_LEGACY_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyjinx.HLE.Loaders.Processes.Extensions;

partial class NcaExtensions
{
    public static bool IsMain(this Nca nca)
    {
        return nca.IsProgram() && !nca.IsPatch();
    }
}

#endif