namespace LibHac.Ncm;

public readonly struct PatchId
{
    public readonly ulong Value;

    public PatchId(ulong value)
    {
        Value = value;
    }

    public static implicit operator ProgramId(PatchId id) => new ProgramId(id.Value);
}

public readonly struct DeltaId
{
    public readonly ulong Value;

    public DeltaId(ulong value)
    {
        Value = value;
    }

    public static implicit operator ProgramId(DeltaId id) => new ProgramId(id.Value);
}