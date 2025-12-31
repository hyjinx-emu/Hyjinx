using LibHac.Common;
using LibHac.Fs;
using LibHac.Gc.Writer;
using System;

namespace LibHac.Gc;

public class GameCardEmulated : IGcApi
{
    public IGcWriterApi Writer { get; }
    
    public void InsertGameCard(in SharedRef<IStorage> storage)
    {
        throw new NotImplementedException();
    }

    public void RemoveGameCard()
    {
        throw new NotImplementedException();
    }

    public void PresetInternalKeys(ReadOnlySpan<byte> gameCardKey, ReadOnlySpan<byte> gameCardCertificate)
    {
        throw new NotImplementedException();
    }

    public void Initialize(Memory<byte> workBuffer, ulong deviceBufferAddress)
    {
        throw new NotImplementedException();
    }

    public void FinalizeGc()
    {
        throw new NotImplementedException();
    }

    public void PowerOffGameCard()
    {
        throw new NotImplementedException();
    }

    public void RegisterDeviceVirtualAddress(Memory<byte> buffer, ulong deviceBufferAddress)
    {
        throw new NotImplementedException();
    }

    public void UnregisterDeviceVirtualAddress(Memory<byte> buffer, ulong deviceBufferAddress)
    {
        throw new NotImplementedException();
    }

    public Result GetInitializationResult()
    {
        throw new NotImplementedException();
    }

    public Result Activate()
    {
        throw new NotImplementedException();
    }

    public void Deactivate()
    {
        throw new NotImplementedException();
    }

    public Result SetCardToSecureMode()
    {
        throw new NotImplementedException();
    }

    public Result Read(Span<byte> destination, uint pageAddress, uint pageCount)
    {
        throw new NotImplementedException();
    }

    public void PutToSleep()
    {
        throw new NotImplementedException();
    }

    public void Awaken()
    {
        throw new NotImplementedException();
    }

    public bool IsCardInserted()
    {
        throw new NotImplementedException();
    }

    public bool IsCardActivationValid()
    {
        throw new NotImplementedException();
    }

    public Result GetCardStatus(out GameCardStatus outStatus)
    {
        throw new NotImplementedException();
    }

    public Result GetCardDeviceId(Span<byte> destBuffer)
    {
        throw new NotImplementedException();
    }

    public Result GetCardDeviceCertificate(Span<byte> destBuffer)
    {
        throw new NotImplementedException();
    }

    public Result ChallengeCardExistence(Span<byte> responseBuffer, ReadOnlySpan<byte> challengeSeedBuffer, ReadOnlySpan<byte> challengeValueBuffer)
    {
        throw new NotImplementedException();
    }

    public Result GetCardImageHash(Span<byte> destBuffer)
    {
        throw new NotImplementedException();
    }

    public Result GetGameCardIdSet(out GameCardIdSet outGcIdSet)
    {
        throw new NotImplementedException();
    }

    public void RegisterDetectionEventCallback(Action<object> function, object args)
    {
        throw new NotImplementedException();
    }

    public void UnregisterDetectionEventCallback()
    {
        throw new NotImplementedException();
    }

    public Result GetCardHeader(Span<byte> destBuffer)
    {
        throw new NotImplementedException();
    }

    public Result GetErrorInfo(out GameCardErrorReportInfo outErrorReportInfo)
    {
        throw new NotImplementedException();
    }
}