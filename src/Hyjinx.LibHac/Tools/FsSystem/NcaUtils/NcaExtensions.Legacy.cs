#if IS_LEGACY_ENABLED

using LibHac.Common;

namespace LibHac.Tools.FsSystem.NcaUtils;

public static partial class NcaExtensions
{
    public static Validity VerifyNca(this Nca nca, IProgressReport? logger = null, bool quiet = false)
    {
        for (int i = 0; i < 3; i++)
        {
            if (nca.CanOpenSection(i))
            {
                Validity sectionValidity = VerifySection(nca, i, logger, quiet);

                if (sectionValidity == Validity.Invalid)
                    return Validity.Invalid;
            }
        }

        return Validity.Valid;
    }

    private static Validity VerifySection(this Nca nca, int index, IProgressReport? logger = null, bool quiet = false)
    {
        NcaFsHeader sect = nca.GetFsHeader(index);
        NcaHashType hashType = sect.HashType;
        if (hashType != NcaHashType.Sha256 && hashType != NcaHashType.Ivfc)
            return Validity.Unchecked;

        var stream = nca.OpenStorage(index, IntegrityCheckLevel.IgnoreOnInvalid, true)
            as HierarchicalIntegrityVerificationStorage;
        if (stream == null)
            return Validity.Unchecked;

        if (!quiet)
            logger?.LogMessage($"Verifying section {index}...");
        Validity validity = stream.Validate(true, logger);

        return validity;
    }

    public static Validity VerifyNca(this Nca nca, Nca patchNca, IProgressReport? logger = null, bool quiet = false)
    {
        for (int i = 0; i < 3; i++)
        {
            if (patchNca.CanOpenSection(i))
            {
                Validity sectionValidity = VerifySection(nca, patchNca, i, logger, quiet);

                if (sectionValidity == Validity.Invalid)
                    return Validity.Invalid;
            }
        }

        return Validity.Valid;
    }

    private static Validity VerifySection(this Nca nca, Nca patchNca, int index, IProgressReport? logger = null, bool quiet = false)
    {
        NcaFsHeader sect = nca.GetFsHeader(index);
        NcaHashType hashType = sect.HashType;
        if (hashType != NcaHashType.Sha256 && hashType != NcaHashType.Ivfc)
            return Validity.Unchecked;

        var stream = nca.OpenStorageWithPatch(patchNca, index, IntegrityCheckLevel.IgnoreOnInvalid, true)
            as HierarchicalIntegrityVerificationStorage;
        if (stream == null)
            return Validity.Unchecked;

        if (!quiet)
            logger?.LogMessage($"Verifying section {index}...");
        Validity validity = stream.Validate(true, logger);

        return validity;
    }
}

#endif