#if IS_LEGACY_ENABLED

namespace LibHac.Common;

partial struct U8SpanMutable
{
    public U8StringMutable ToU8String()
    {
        return new U8StringMutable(_buffer.ToArray());
    }
}

#endif