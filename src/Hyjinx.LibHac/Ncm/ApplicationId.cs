using System;

namespace LibHac.Ncm;

public readonly struct ApplicationId : IEquatable<ApplicationId>
{
    public readonly ulong Value;

    public ApplicationId(ulong value)
    {
        Value = value;
    }

    public static readonly ApplicationId InvalidId = default;

    public static readonly ApplicationId Start = new(0x0100000000010000);
    public static readonly ApplicationId End = new(0x01FFFFFFFFFFFFFF);

    public static implicit operator ProgramId(ApplicationId id) => new ProgramId(id.Value);

    public static bool IsApplicationId(ProgramId programId)
    {
        return Start <= programId && programId <= End;
    }

    public bool Equals(ApplicationId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ApplicationId id && Equals(id);
    public override int GetHashCode() => HashCode.Combine(Value);

    public static bool operator ==(ApplicationId left, ApplicationId right) => left.Equals(right);
    public static bool operator !=(ApplicationId left, ApplicationId right) => !left.Equals(right);
}
