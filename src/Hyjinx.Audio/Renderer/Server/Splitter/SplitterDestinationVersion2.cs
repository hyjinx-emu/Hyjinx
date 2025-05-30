using Hyjinx.Audio.Renderer.Parameter;
using Hyjinx.Common.Memory;
using Hyjinx.Common.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hyjinx.Audio.Renderer.Server.Splitter;

/// <summary>
/// Server state for a splitter destination (version 2).
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x110, Pack = Alignment)]
public struct SplitterDestinationVersion2
{
    public const int Alignment = 0x10;

    /// <summary>
    /// The unique id of this <see cref="SplitterDestinationVersion2"/>.
    /// </summary>
    public int Id;

    /// <summary>
    /// The mix to output the result of the splitter.
    /// </summary>
    public int DestinationId;

    /// <summary>
    /// Mix buffer volumes storage.
    /// </summary>
    private MixArray _mix;
    private MixArray _previousMix;

    /// <summary>
    /// Pointer to the next linked element.
    /// </summary>
    private unsafe SplitterDestinationVersion2* _next;

    /// <summary>
    /// Set to true if in use.
    /// </summary>
    [MarshalAs(UnmanagedType.I1)]
    public bool IsUsed;

    /// <summary>
    /// Set to true if the internal state need to be updated.
    /// </summary>
    [MarshalAs(UnmanagedType.I1)]
    public bool NeedToUpdateInternalState;

    [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * Constants.MixBufferCountMax, Pack = 1)]
    private struct MixArray { }

    /// <summary>
    /// Mix buffer volumes.
    /// </summary>
    /// <remarks>Used when a splitter id is specified in the mix.</remarks>
    public Span<float> MixBufferVolume => SpanHelpers.AsSpan<MixArray, float>(ref _mix);

    /// <summary>
    /// Previous mix buffer volumes.
    /// </summary>
    /// <remarks>Used when a splitter id is specified in the mix.</remarks>
    public Span<float> PreviousMixBufferVolume => SpanHelpers.AsSpan<MixArray, float>(ref _previousMix);

    /// <summary>
    /// Get the reference of the next element or null if not present.
    /// </summary>
    public readonly ref SplitterDestinationVersion2 Next
    {
        get
        {
            unsafe
            {
                return ref Unsafe.AsRef<SplitterDestinationVersion2>(_next);
            }
        }
    }

    private Array2<BiquadFilterParameter> _biquadFilters;

    private Array2<bool> _isPreviousBiquadFilterEnabled;

    /// <summary>
    /// Create a new <see cref="SplitterDestinationVersion2"/>.
    /// </summary>
    /// <param name="id">The unique id of this <see cref="SplitterDestinationVersion2"/>.</param>
    public SplitterDestinationVersion2(int id) : this()
    {
        Id = id;
        DestinationId = Constants.UnusedMixId;

        ClearVolumes();
    }

    /// <summary>
    /// Update the <see cref="SplitterDestinationVersion2"/> from user parameter.
    /// </summary>
    /// <param name="parameter">The user parameter.</param>
    /// <param name="isPrevVolumeResetSupported">Indicates that the audio renderer revision in use supports explicitly resetting the volume.</param>
    public void Update<T>(in T parameter, bool isPrevVolumeResetSupported) where T : ISplitterDestinationInParameter
    {
        Debug.Assert(Id == parameter.Id);

        if (parameter.IsMagicValid() && Id == parameter.Id)
        {
            DestinationId = parameter.DestinationId;

            parameter.MixBufferVolume.CopyTo(MixBufferVolume);

            _biquadFilters = parameter.BiquadFilters;

            bool resetPrevVolume = isPrevVolumeResetSupported ? parameter.ResetPrevVolume : !IsUsed && parameter.IsUsed;
            if (resetPrevVolume)
            {
                MixBufferVolume.CopyTo(PreviousMixBufferVolume);

                NeedToUpdateInternalState = false;
            }

            IsUsed = parameter.IsUsed;
        }
    }

    /// <summary>
    /// Update the internal state of the instance.
    /// </summary>
    public void UpdateInternalState()
    {
        if (IsUsed && NeedToUpdateInternalState)
        {
            MixBufferVolume.CopyTo(PreviousMixBufferVolume);
        }

        NeedToUpdateInternalState = false;
    }

    /// <summary>
    /// Set the update internal state marker.
    /// </summary>
    public void MarkAsNeedToUpdateInternalState()
    {
        NeedToUpdateInternalState = true;
    }

    /// <summary>
    /// Return true if the <see cref="SplitterDestinationVersion2"/> is used and has a destination.
    /// </summary>
    /// <returns>True if the <see cref="SplitterDestinationVersion2"/> is used and has a destination.</returns>
    public readonly bool IsConfigured()
    {
        return IsUsed && DestinationId != Constants.UnusedMixId;
    }

    /// <summary>
    /// Get the volume for a given destination.
    /// </summary>
    /// <param name="destinationIndex">The destination index to use.</param>
    /// <returns>The volume for the given destination.</returns>
    public float GetMixVolume(int destinationIndex)
    {
        Debug.Assert(destinationIndex >= 0 && destinationIndex < Constants.MixBufferCountMax);

        return MixBufferVolume[destinationIndex];
    }

    /// <summary>
    /// Get the previous volume for a given destination.
    /// </summary>
    /// <param name="destinationIndex">The destination index to use.</param>
    /// <returns>The volume for the given destination.</returns>
    public float GetMixVolumePrev(int destinationIndex)
    {
        Debug.Assert(destinationIndex >= 0 && destinationIndex < Constants.MixBufferCountMax);

        return PreviousMixBufferVolume[destinationIndex];
    }

    /// <summary>
    /// Clear the volumes.
    /// </summary>
    public void ClearVolumes()
    {
        MixBufferVolume.Clear();
        PreviousMixBufferVolume.Clear();
    }

    /// <summary>
    /// Link the next element to the given <see cref="SplitterDestinationVersion2"/>.
    /// </summary>
    /// <param name="next">The given <see cref="SplitterDestinationVersion2"/> to link.</param>
    public void Link(ref SplitterDestinationVersion2 next)
    {
        unsafe
        {
            fixed (SplitterDestinationVersion2* nextPtr = &next)
            {
                _next = nextPtr;
            }
        }
    }

    /// <summary>
    /// Remove the link to the next element.
    /// </summary>
    public void Unlink()
    {
        unsafe
        {
            _next = null;
        }
    }

    /// <summary>
    /// Checks if any biquad filter is enabled.
    /// </summary>
    /// <returns>True if any biquad filter is enabled.</returns>
    public bool IsBiquadFilterEnabled()
    {
        return _biquadFilters[0].Enable || _biquadFilters[1].Enable;
    }

    /// <summary>
    /// Checks if any biquad filter was previously enabled.
    /// </summary>
    /// <returns>True if any biquad filter was previously enabled.</returns>
    public bool IsBiquadFilterEnabledPrev()
    {
        return _isPreviousBiquadFilterEnabled[0];
    }

    /// <summary>
    /// Gets the biquad filter parameters.
    /// </summary>
    /// <param name="index">Biquad filter index (0 or 1).</param>
    /// <returns>Biquad filter parameters.</returns>
    public ref BiquadFilterParameter GetBiquadFilterParameter(int index)
    {
        return ref _biquadFilters[index];
    }

    /// <summary>
    /// Checks if any biquad filter was previously enabled.
    /// </summary>
    /// <param name="index">Biquad filter index (0 or 1).</param>
    public void UpdateBiquadFilterEnabledPrev(int index)
    {
        _isPreviousBiquadFilterEnabled[index] = _biquadFilters[index].Enable;
    }
}