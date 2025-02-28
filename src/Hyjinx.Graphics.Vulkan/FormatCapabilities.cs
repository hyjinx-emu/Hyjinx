using Ryujinx.Common.Logging;
using Hyjinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using Format = Hyjinx.Graphics.GAL.Format;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Hyjinx.Graphics.Vulkan
{
    class FormatCapabilities
    {
        private static readonly Format[] _scaledFormats = {
            Format.R8Uscaled,
            Format.R8Sscaled,
            Format.R16Uscaled,
            Format.R16Sscaled,
            Format.R8G8Uscaled,
            Format.R8G8Sscaled,
            Format.R16G16Uscaled,
            Format.R16G16Sscaled,
            Format.R8G8B8Uscaled,
            Format.R8G8B8Sscaled,
            Format.R16G16B16Uscaled,
            Format.R16G16B16Sscaled,
            Format.R8G8B8A8Uscaled,
            Format.R8G8B8A8Sscaled,
            Format.R16G16B16A16Uscaled,
            Format.R16G16B16A16Sscaled,
            Format.R10G10B10A2Uscaled,
            Format.R10G10B10A2Sscaled,
        };

        private static readonly Format[] _intFormats = {
            Format.R8Uint,
            Format.R8Sint,
            Format.R16Uint,
            Format.R16Sint,
            Format.R8G8Uint,
            Format.R8G8Sint,
            Format.R16G16Uint,
            Format.R16G16Sint,
            Format.R8G8B8Uint,
            Format.R8G8B8Sint,
            Format.R16G16B16Uint,
            Format.R16G16B16Sint,
            Format.R8G8B8A8Uint,
            Format.R8G8B8A8Sint,
            Format.R16G16B16A16Uint,
            Format.R16G16B16A16Sint,
            Format.R10G10B10A2Uint,
            Format.R10G10B10A2Sint,
        };

        private readonly FormatFeatureFlags[] _bufferTable;
        private readonly FormatFeatureFlags[] _optimalTable;

        private readonly Vk _api;
        private readonly PhysicalDevice _physicalDevice;

        public FormatCapabilities(Vk api, PhysicalDevice physicalDevice)
        {
            _api = api;
            _physicalDevice = physicalDevice;

            int totalFormats = Enum.GetNames(typeof(Format)).Length;

            _bufferTable = new FormatFeatureFlags[totalFormats];
            _optimalTable = new FormatFeatureFlags[totalFormats];
        }

        public bool BufferFormatsSupport(FormatFeatureFlags flags, params Format[] formats)
        {
            foreach (Format format in formats)
            {
                if (!BufferFormatSupports(flags, format))
                {
                    return false;
                }
            }

            return true;
        }

        public bool OptimalFormatsSupport(FormatFeatureFlags flags, params Format[] formats)
        {
            foreach (Format format in formats)
            {
                if (!OptimalFormatSupports(flags, format))
                {
                    return false;
                }
            }

            return true;
        }

        public bool BufferFormatSupports(FormatFeatureFlags flags, Format format)
        {
            var formatFeatureFlags = _bufferTable[(int)format];

            if (formatFeatureFlags == 0)
            {
                _api.GetPhysicalDeviceFormatProperties(_physicalDevice, FormatTable.GetFormat(format), out var fp);
                formatFeatureFlags = fp.BufferFeatures;
                _bufferTable[(int)format] = formatFeatureFlags;
            }

            return (formatFeatureFlags & flags) == flags;
        }

        public bool SupportsScaledVertexFormats()
        {
            // We want to check is all scaled formats are supported,
            // but if the integer variant is not supported either,
            // then the format is likely not supported at all,
            // we ignore formats that are entirely unsupported here.

            for (int i = 0; i < _scaledFormats.Length; i++)
            {
                if (!BufferFormatSupports(FormatFeatureFlags.VertexBufferBit, _scaledFormats[i]) &&
                    BufferFormatSupports(FormatFeatureFlags.VertexBufferBit, _intFormats[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool BufferFormatSupports(FormatFeatureFlags flags, VkFormat format)
        {
            _api.GetPhysicalDeviceFormatProperties(_physicalDevice, format, out var fp);

            return (fp.BufferFeatures & flags) == flags;
        }

        public bool OptimalFormatSupports(FormatFeatureFlags flags, Format format)
        {
            var formatFeatureFlags = _optimalTable[(int)format];

            if (formatFeatureFlags == 0)
            {
                _api.GetPhysicalDeviceFormatProperties(_physicalDevice, FormatTable.GetFormat(format), out var fp);
                formatFeatureFlags = fp.OptimalTilingFeatures;
                _optimalTable[(int)format] = formatFeatureFlags;
            }

            return (formatFeatureFlags & flags) == flags;
        }

        public VkFormat ConvertToVkFormat(Format srcFormat)
        {
            var format = FormatTable.GetFormat(srcFormat);

            var requiredFeatures = FormatFeatureFlags.SampledImageBit |
                                   FormatFeatureFlags.TransferSrcBit |
                                   FormatFeatureFlags.TransferDstBit;

            if (srcFormat.IsDepthOrStencil())
            {
                requiredFeatures |= FormatFeatureFlags.DepthStencilAttachmentBit;
            }
            else if (srcFormat.IsRtColorCompatible())
            {
                requiredFeatures |= FormatFeatureFlags.ColorAttachmentBit;
            }

            if (srcFormat.IsImageCompatible())
            {
                requiredFeatures |= FormatFeatureFlags.StorageImageBit;
            }

            if (!OptimalFormatSupports(requiredFeatures, srcFormat) || (IsD24S8(srcFormat) && VulkanConfiguration.ForceD24S8Unsupported))
            {
                // The format is not supported. Can we convert it to a higher precision format?
                if (IsD24S8(srcFormat))
                {
                    format = VkFormat.D32SfloatS8Uint;
                }
                else if (srcFormat == Format.R4G4B4A4Unorm)
                {
                    format = VkFormat.R4G4B4A4UnormPack16;
                }
                else
                {
                    Logger.Error?.Print(LogClass.Gpu, $"Format {srcFormat} is not supported by the host.");
                }
            }

            return format;
        }

        public VkFormat ConvertToVertexVkFormat(Format srcFormat)
        {
            var format = FormatTable.GetFormat(srcFormat);

            if (!BufferFormatSupports(FormatFeatureFlags.VertexBufferBit, srcFormat) ||
                (IsRGB16IntFloat(srcFormat) && VulkanConfiguration.ForceRGB16IntFloatUnsupported))
            {
                // The format is not supported. Can we convert it to an alternative format?
                switch (srcFormat)
                {
                    case Format.R16G16B16Float:
                        format = VkFormat.R16G16B16A16Sfloat;
                        break;
                    case Format.R16G16B16Sint:
                        format = VkFormat.R16G16B16A16Sint;
                        break;
                    case Format.R16G16B16Uint:
                        format = VkFormat.R16G16B16A16Uint;
                        break;
                    default:
                        Logger.Error?.Print(LogClass.Gpu, $"Format {srcFormat} is not supported by the host.");
                        break;
                }
            }

            return format;
        }

        public static bool IsD24S8(Format format)
        {
            return format == Format.D24UnormS8Uint || format == Format.S8UintD24Unorm || format == Format.X8UintD24Unorm;
        }

        private static bool IsRGB16IntFloat(Format format)
        {
            return format == Format.R16G16B16Float ||
                   format == Format.R16G16B16Sint ||
                   format == Format.R16G16B16Uint;
        }
    }
}
