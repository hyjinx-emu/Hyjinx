namespace Hyjinx.Horizon.Sdk.Ncm;

public readonly struct ApplicationId
{
    public readonly ulong Id;

    public static readonly ApplicationId First = new(LibHac.Ncm.ApplicationId.Start.Value);

    public static readonly ApplicationId Last = new(LibHac.Ncm.ApplicationId.End.Value);

    public static readonly ApplicationId Invalid = new(LibHac.Ncm.ApplicationId.InvalidId.Value);

    public bool IsValid => Id >= First.Id && Id <= Last.Id;

    public ApplicationId(ulong id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        return obj is ApplicationId applicationId && applicationId.Equals(this);
    }

    public bool Equals(ApplicationId other)
    {
        return other.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(ApplicationId lhs, ApplicationId rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(ApplicationId lhs, ApplicationId rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override string ToString()
    {
        return $"0x{Id:x}";
    }
}