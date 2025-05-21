using System;

namespace Hyjinx.Horizon.Sdk.Ngc;

[Flags]
enum ProfanityFilterFlags
{
    None = 0,
    MatchNormalizedFormKC = 1 << 0,
    MatchSimilarForm = 1 << 1,
}