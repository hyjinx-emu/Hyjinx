using LibHac.Common;
using LibHac.Ncm;
using System;

namespace LibHac.Lr;

public class RegisteredLocationResolver : IDisposable
{
    private SharedRef<IRegisteredLocationResolver> _interface;

    public RegisteredLocationResolver(ref SharedRef<IRegisteredLocationResolver> baseInterface)
    {
        _interface = SharedRef<IRegisteredLocationResolver>.CreateMove(ref baseInterface);
    }

    public void Dispose()
    {
        _interface.Destroy();
    }

    public Result ResolveProgramPath(out Path path, ProgramId id) =>
        _interface.Get.ResolveProgramPath(out path, id);

    public Result RegisterProgramPath(in Path path, ProgramId id, ProgramId ownerId) =>
        _interface.Get.RegisterProgramPath(in path, id, ownerId);

    public Result UnregisterProgramPath(ProgramId id) =>
        _interface.Get.UnregisterProgramPath(id);

    public Result RedirectProgramPath(in Path path, ProgramId id, ProgramId ownerId) =>
        _interface.Get.RedirectProgramPath(in path, id, ownerId);

    public Result ResolveHtmlDocumentPath(out Path path, ProgramId id) =>
        _interface.Get.ResolveHtmlDocumentPath(out path, id);

    public Result RegisterHtmlDocumentPath(in Path path, ProgramId id, ProgramId ownerId) =>
        _interface.Get.RegisterHtmlDocumentPath(in path, id, ownerId);

    public Result UnregisterHtmlDocumentPath(ProgramId id) =>
        _interface.Get.UnregisterHtmlDocumentPath(id);

    public Result RedirectHtmlDocumentPath(in Path path, ProgramId id) =>
        _interface.Get.RedirectHtmlDocumentPath(in path, id);

    public Result Refresh() =>
        _interface.Get.Refresh();

    public Result RefreshExcluding(ReadOnlySpan<ProgramId> ids) =>
        _interface.Get.RefreshExcluding(ids);
}