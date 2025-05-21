using System;

namespace Hyjinx.Horizon.Sdk.Ngc.Detail;

struct MatchRangeListState
{
    public MatchRangeList MatchRanges;

    public MatchRangeListState()
    {
        MatchRanges = new();
    }

    public static bool AddMatch(ReadOnlySpan<byte> text, int startOffset, int endOffset, int nodeId, ref MatchRangeListState state)
    {
        state.MatchRanges.Add(startOffset, endOffset);

        return true;
    }
}