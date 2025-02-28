using Ryujinx.Common.Logging;
using Hyjinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Silk.NET.Vulkan;
using System;
using BlendFactor = Silk.NET.Vulkan.BlendFactor;
using BlendOp = Silk.NET.Vulkan.BlendOp;
using CompareOp = Silk.NET.Vulkan.CompareOp;
using Format = Hyjinx.Graphics.GAL.Format;
using FrontFace = Silk.NET.Vulkan.FrontFace;
using IndexType = Silk.NET.Vulkan.IndexType;
using PrimitiveTopology = Silk.NET.Vulkan.PrimitiveTopology;
using StencilOp = Silk.NET.Vulkan.StencilOp;

namespace Hyjinx.Graphics.Vulkan
{
    static class EnumConversion
    {
        public static ShaderStageFlags Convert(this ShaderStage stage)
        {
            return stage switch
            {
                ShaderStage.Vertex => ShaderStageFlags.VertexBit,
                ShaderStage.Geometry => ShaderStageFlags.GeometryBit,
                ShaderStage.TessellationControl => ShaderStageFlags.TessellationControlBit,
                ShaderStage.TessellationEvaluation => ShaderStageFlags.TessellationEvaluationBit,
                ShaderStage.Fragment => ShaderStageFlags.FragmentBit,
                ShaderStage.Compute => ShaderStageFlags.ComputeBit,
                _ => LogInvalidAndReturn(stage, nameof(ShaderStage), (ShaderStageFlags)0),
            };
        }

        public static PipelineStageFlags ConvertToPipelineStageFlags(this ShaderStage stage)
        {
            return stage switch
            {
                ShaderStage.Vertex => PipelineStageFlags.VertexShaderBit,
                ShaderStage.Geometry => PipelineStageFlags.GeometryShaderBit,
                ShaderStage.TessellationControl => PipelineStageFlags.TessellationControlShaderBit,
                ShaderStage.TessellationEvaluation => PipelineStageFlags.TessellationEvaluationShaderBit,
                ShaderStage.Fragment => PipelineStageFlags.FragmentShaderBit,
                ShaderStage.Compute => PipelineStageFlags.ComputeShaderBit,
                _ => LogInvalidAndReturn(stage, nameof(ShaderStage), (PipelineStageFlags)0),
            };
        }

        public static ShaderStageFlags Convert(this ResourceStages stages)
        {
            ShaderStageFlags stageFlags = stages.HasFlag(ResourceStages.Compute)
                ? ShaderStageFlags.ComputeBit
                : ShaderStageFlags.None;

            if (stages.HasFlag(ResourceStages.Vertex))
            {
                stageFlags |= ShaderStageFlags.VertexBit;
            }

            if (stages.HasFlag(ResourceStages.TessellationControl))
            {
                stageFlags |= ShaderStageFlags.TessellationControlBit;
            }

            if (stages.HasFlag(ResourceStages.TessellationEvaluation))
            {
                stageFlags |= ShaderStageFlags.TessellationEvaluationBit;
            }

            if (stages.HasFlag(ResourceStages.Geometry))
            {
                stageFlags |= ShaderStageFlags.GeometryBit;
            }

            if (stages.HasFlag(ResourceStages.Fragment))
            {
                stageFlags |= ShaderStageFlags.FragmentBit;
            }

            return stageFlags;
        }

        public static DescriptorType Convert(this ResourceType type)
        {
            return type switch
            {
                ResourceType.UniformBuffer => DescriptorType.UniformBuffer,
                ResourceType.StorageBuffer => DescriptorType.StorageBuffer,
                ResourceType.Texture => DescriptorType.SampledImage,
                ResourceType.Sampler => DescriptorType.Sampler,
                ResourceType.TextureAndSampler => DescriptorType.CombinedImageSampler,
                ResourceType.Image => DescriptorType.StorageImage,
                ResourceType.BufferTexture => DescriptorType.UniformTexelBuffer,
                ResourceType.BufferImage => DescriptorType.StorageTexelBuffer,
                _ => throw new ArgumentException($"Invalid resource type \"{type}\"."),
            };
        }

        public static SamplerAddressMode Convert(this AddressMode mode)
        {
            return mode switch
            {
                AddressMode.Clamp => SamplerAddressMode.ClampToEdge, // TODO: Should be clamp.
                AddressMode.Repeat => SamplerAddressMode.Repeat,
                AddressMode.MirrorClamp => SamplerAddressMode.ClampToEdge, // TODO: Should be mirror clamp.
                AddressMode.MirrorClampToEdge => SamplerAddressMode.MirrorClampToEdgeKhr,
                AddressMode.MirrorClampToBorder => SamplerAddressMode.ClampToBorder, // TODO: Should be mirror clamp to border.
                AddressMode.ClampToBorder => SamplerAddressMode.ClampToBorder,
                AddressMode.MirroredRepeat => SamplerAddressMode.MirroredRepeat,
                AddressMode.ClampToEdge => SamplerAddressMode.ClampToEdge,
                _ => LogInvalidAndReturn(mode, nameof(AddressMode), SamplerAddressMode.ClampToEdge), // TODO: Should be clamp.
            };
        }

        public static BlendFactor Convert(this Hyjinx.Graphics.GAL.BlendFactor factor)
        {
            return factor switch
            {
                Hyjinx.Graphics.GAL.BlendFactor.Zero or Hyjinx.Graphics.GAL.BlendFactor.ZeroGl => BlendFactor.Zero,
                Hyjinx.Graphics.GAL.BlendFactor.One or Hyjinx.Graphics.GAL.BlendFactor.OneGl => BlendFactor.One,
                Hyjinx.Graphics.GAL.BlendFactor.SrcColor or Hyjinx.Graphics.GAL.BlendFactor.SrcColorGl => BlendFactor.SrcColor,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrcColor or Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrcColorGl => BlendFactor.OneMinusSrcColor,
                Hyjinx.Graphics.GAL.BlendFactor.SrcAlpha or Hyjinx.Graphics.GAL.BlendFactor.SrcAlphaGl => BlendFactor.SrcAlpha,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrcAlpha or Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrcAlphaGl => BlendFactor.OneMinusSrcAlpha,
                Hyjinx.Graphics.GAL.BlendFactor.DstAlpha or Hyjinx.Graphics.GAL.BlendFactor.DstAlphaGl => BlendFactor.DstAlpha,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusDstAlpha or Hyjinx.Graphics.GAL.BlendFactor.OneMinusDstAlphaGl => BlendFactor.OneMinusDstAlpha,
                Hyjinx.Graphics.GAL.BlendFactor.DstColor or Hyjinx.Graphics.GAL.BlendFactor.DstColorGl => BlendFactor.DstColor,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusDstColor or Hyjinx.Graphics.GAL.BlendFactor.OneMinusDstColorGl => BlendFactor.OneMinusDstColor,
                Hyjinx.Graphics.GAL.BlendFactor.SrcAlphaSaturate or Hyjinx.Graphics.GAL.BlendFactor.SrcAlphaSaturateGl => BlendFactor.SrcAlphaSaturate,
                Hyjinx.Graphics.GAL.BlendFactor.Src1Color or Hyjinx.Graphics.GAL.BlendFactor.Src1ColorGl => BlendFactor.Src1Color,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrc1Color or Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrc1ColorGl => BlendFactor.OneMinusSrc1Color,
                Hyjinx.Graphics.GAL.BlendFactor.Src1Alpha or Hyjinx.Graphics.GAL.BlendFactor.Src1AlphaGl => BlendFactor.Src1Alpha,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrc1Alpha or Hyjinx.Graphics.GAL.BlendFactor.OneMinusSrc1AlphaGl => BlendFactor.OneMinusSrc1Alpha,
                Hyjinx.Graphics.GAL.BlendFactor.ConstantColor => BlendFactor.ConstantColor,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusConstantColor => BlendFactor.OneMinusConstantColor,
                Hyjinx.Graphics.GAL.BlendFactor.ConstantAlpha => BlendFactor.ConstantAlpha,
                Hyjinx.Graphics.GAL.BlendFactor.OneMinusConstantAlpha => BlendFactor.OneMinusConstantAlpha,
                _ => LogInvalidAndReturn(factor, nameof(Hyjinx.Graphics.GAL.BlendFactor), BlendFactor.Zero),
            };
        }

        public static BlendOp Convert(this AdvancedBlendOp op)
        {
            return op switch
            {
                AdvancedBlendOp.Zero => BlendOp.ZeroExt,
                AdvancedBlendOp.Src => BlendOp.SrcExt,
                AdvancedBlendOp.Dst => BlendOp.DstExt,
                AdvancedBlendOp.SrcOver => BlendOp.SrcOverExt,
                AdvancedBlendOp.DstOver => BlendOp.DstOverExt,
                AdvancedBlendOp.SrcIn => BlendOp.SrcInExt,
                AdvancedBlendOp.DstIn => BlendOp.DstInExt,
                AdvancedBlendOp.SrcOut => BlendOp.SrcOutExt,
                AdvancedBlendOp.DstOut => BlendOp.DstOutExt,
                AdvancedBlendOp.SrcAtop => BlendOp.SrcAtopExt,
                AdvancedBlendOp.DstAtop => BlendOp.DstAtopExt,
                AdvancedBlendOp.Xor => BlendOp.XorExt,
                AdvancedBlendOp.Plus => BlendOp.PlusExt,
                AdvancedBlendOp.PlusClamped => BlendOp.PlusClampedExt,
                AdvancedBlendOp.PlusClampedAlpha => BlendOp.PlusClampedAlphaExt,
                AdvancedBlendOp.PlusDarker => BlendOp.PlusDarkerExt,
                AdvancedBlendOp.Multiply => BlendOp.MultiplyExt,
                AdvancedBlendOp.Screen => BlendOp.ScreenExt,
                AdvancedBlendOp.Overlay => BlendOp.OverlayExt,
                AdvancedBlendOp.Darken => BlendOp.DarkenExt,
                AdvancedBlendOp.Lighten => BlendOp.LightenExt,
                AdvancedBlendOp.ColorDodge => BlendOp.ColordodgeExt,
                AdvancedBlendOp.ColorBurn => BlendOp.ColorburnExt,
                AdvancedBlendOp.HardLight => BlendOp.HardlightExt,
                AdvancedBlendOp.SoftLight => BlendOp.SoftlightExt,
                AdvancedBlendOp.Difference => BlendOp.DifferenceExt,
                AdvancedBlendOp.Minus => BlendOp.MinusExt,
                AdvancedBlendOp.MinusClamped => BlendOp.MinusClampedExt,
                AdvancedBlendOp.Exclusion => BlendOp.ExclusionExt,
                AdvancedBlendOp.Contrast => BlendOp.ContrastExt,
                AdvancedBlendOp.Invert => BlendOp.InvertExt,
                AdvancedBlendOp.InvertRGB => BlendOp.InvertRgbExt,
                AdvancedBlendOp.InvertOvg => BlendOp.InvertOvgExt,
                AdvancedBlendOp.LinearDodge => BlendOp.LineardodgeExt,
                AdvancedBlendOp.LinearBurn => BlendOp.LinearburnExt,
                AdvancedBlendOp.VividLight => BlendOp.VividlightExt,
                AdvancedBlendOp.LinearLight => BlendOp.LinearlightExt,
                AdvancedBlendOp.PinLight => BlendOp.PinlightExt,
                AdvancedBlendOp.HardMix => BlendOp.HardmixExt,
                AdvancedBlendOp.Red => BlendOp.RedExt,
                AdvancedBlendOp.Green => BlendOp.GreenExt,
                AdvancedBlendOp.Blue => BlendOp.BlueExt,
                AdvancedBlendOp.HslHue => BlendOp.HslHueExt,
                AdvancedBlendOp.HslSaturation => BlendOp.HslSaturationExt,
                AdvancedBlendOp.HslColor => BlendOp.HslColorExt,
                AdvancedBlendOp.HslLuminosity => BlendOp.HslLuminosityExt,
                _ => LogInvalidAndReturn(op, nameof(AdvancedBlendOp), BlendOp.Add),
            };
        }

        public static BlendOp Convert(this Hyjinx.Graphics.GAL.BlendOp op)
        {
            return op switch
            {
                Hyjinx.Graphics.GAL.BlendOp.Add or Hyjinx.Graphics.GAL.BlendOp.AddGl => BlendOp.Add,
                Hyjinx.Graphics.GAL.BlendOp.Subtract or Hyjinx.Graphics.GAL.BlendOp.SubtractGl => BlendOp.Subtract,
                Hyjinx.Graphics.GAL.BlendOp.ReverseSubtract or Hyjinx.Graphics.GAL.BlendOp.ReverseSubtractGl => BlendOp.ReverseSubtract,
                Hyjinx.Graphics.GAL.BlendOp.Minimum or Hyjinx.Graphics.GAL.BlendOp.MinimumGl => BlendOp.Min,
                Hyjinx.Graphics.GAL.BlendOp.Maximum or Hyjinx.Graphics.GAL.BlendOp.MaximumGl => BlendOp.Max,
                _ => LogInvalidAndReturn(op, nameof(Hyjinx.Graphics.GAL.BlendOp), BlendOp.Add),
            };
        }

        public static BlendOverlapEXT Convert(this AdvancedBlendOverlap overlap)
        {
            return overlap switch
            {
                AdvancedBlendOverlap.Uncorrelated => BlendOverlapEXT.UncorrelatedExt,
                AdvancedBlendOverlap.Disjoint => BlendOverlapEXT.DisjointExt,
                AdvancedBlendOverlap.Conjoint => BlendOverlapEXT.ConjointExt,
                _ => LogInvalidAndReturn(overlap, nameof(AdvancedBlendOverlap), BlendOverlapEXT.UncorrelatedExt),
            };
        }

        public static CompareOp Convert(this Hyjinx.Graphics.GAL.CompareOp op)
        {
            return op switch
            {
                Hyjinx.Graphics.GAL.CompareOp.Never or Hyjinx.Graphics.GAL.CompareOp.NeverGl => CompareOp.Never,
                Hyjinx.Graphics.GAL.CompareOp.Less or Hyjinx.Graphics.GAL.CompareOp.LessGl => CompareOp.Less,
                Hyjinx.Graphics.GAL.CompareOp.Equal or Hyjinx.Graphics.GAL.CompareOp.EqualGl => CompareOp.Equal,
                Hyjinx.Graphics.GAL.CompareOp.LessOrEqual or Hyjinx.Graphics.GAL.CompareOp.LessOrEqualGl => CompareOp.LessOrEqual,
                Hyjinx.Graphics.GAL.CompareOp.Greater or Hyjinx.Graphics.GAL.CompareOp.GreaterGl => CompareOp.Greater,
                Hyjinx.Graphics.GAL.CompareOp.NotEqual or Hyjinx.Graphics.GAL.CompareOp.NotEqualGl => CompareOp.NotEqual,
                Hyjinx.Graphics.GAL.CompareOp.GreaterOrEqual or Hyjinx.Graphics.GAL.CompareOp.GreaterOrEqualGl => CompareOp.GreaterOrEqual,
                Hyjinx.Graphics.GAL.CompareOp.Always or Hyjinx.Graphics.GAL.CompareOp.AlwaysGl => CompareOp.Always,
                _ => LogInvalidAndReturn(op, nameof(Hyjinx.Graphics.GAL.CompareOp), CompareOp.Never),
            };
        }

        public static CullModeFlags Convert(this Face face)
        {
            return face switch
            {
                Face.Back => CullModeFlags.BackBit,
                Face.Front => CullModeFlags.FrontBit,
                Face.FrontAndBack => CullModeFlags.FrontAndBack,
                _ => LogInvalidAndReturn(face, nameof(Face), CullModeFlags.BackBit),
            };
        }

        public static FrontFace Convert(this Hyjinx.Graphics.GAL.FrontFace frontFace)
        {
            // Flipped to account for origin differences.
            return frontFace switch
            {
                Hyjinx.Graphics.GAL.FrontFace.Clockwise => FrontFace.CounterClockwise,
                Hyjinx.Graphics.GAL.FrontFace.CounterClockwise => FrontFace.Clockwise,
                _ => LogInvalidAndReturn(frontFace, nameof(Hyjinx.Graphics.GAL.FrontFace), FrontFace.Clockwise),
            };
        }

        public static IndexType Convert(this Hyjinx.Graphics.GAL.IndexType type)
        {
            return type switch
            {
                Hyjinx.Graphics.GAL.IndexType.UByte => IndexType.Uint8Ext,
                Hyjinx.Graphics.GAL.IndexType.UShort => IndexType.Uint16,
                Hyjinx.Graphics.GAL.IndexType.UInt => IndexType.Uint32,
                _ => LogInvalidAndReturn(type, nameof(Hyjinx.Graphics.GAL.IndexType), IndexType.Uint16),
            };
        }

        public static Filter Convert(this MagFilter filter)
        {
            return filter switch
            {
                MagFilter.Nearest => Filter.Nearest,
                MagFilter.Linear => Filter.Linear,
                _ => LogInvalidAndReturn(filter, nameof(MagFilter), Filter.Nearest),
            };
        }

        public static (Filter, SamplerMipmapMode) Convert(this MinFilter filter)
        {
            return filter switch
            {
                MinFilter.Nearest => (Filter.Nearest, SamplerMipmapMode.Nearest),
                MinFilter.Linear => (Filter.Linear, SamplerMipmapMode.Nearest),
                MinFilter.NearestMipmapNearest => (Filter.Nearest, SamplerMipmapMode.Nearest),
                MinFilter.LinearMipmapNearest => (Filter.Linear, SamplerMipmapMode.Nearest),
                MinFilter.NearestMipmapLinear => (Filter.Nearest, SamplerMipmapMode.Linear),
                MinFilter.LinearMipmapLinear => (Filter.Linear, SamplerMipmapMode.Linear),
                _ => LogInvalidAndReturn(filter, nameof(MinFilter), (Filter.Nearest, SamplerMipmapMode.Nearest)),
            };
        }

        public static PrimitiveTopology Convert(this Hyjinx.Graphics.GAL.PrimitiveTopology topology)
        {
            return topology switch
            {
                Hyjinx.Graphics.GAL.PrimitiveTopology.Points => PrimitiveTopology.PointList,
                Hyjinx.Graphics.GAL.PrimitiveTopology.Lines => PrimitiveTopology.LineList,
                Hyjinx.Graphics.GAL.PrimitiveTopology.LineStrip => PrimitiveTopology.LineStrip,
                Hyjinx.Graphics.GAL.PrimitiveTopology.Triangles => PrimitiveTopology.TriangleList,
                Hyjinx.Graphics.GAL.PrimitiveTopology.TriangleStrip => PrimitiveTopology.TriangleStrip,
                Hyjinx.Graphics.GAL.PrimitiveTopology.TriangleFan => PrimitiveTopology.TriangleFan,
                Hyjinx.Graphics.GAL.PrimitiveTopology.LinesAdjacency => PrimitiveTopology.LineListWithAdjacency,
                Hyjinx.Graphics.GAL.PrimitiveTopology.LineStripAdjacency => PrimitiveTopology.LineStripWithAdjacency,
                Hyjinx.Graphics.GAL.PrimitiveTopology.TrianglesAdjacency => PrimitiveTopology.TriangleListWithAdjacency,
                Hyjinx.Graphics.GAL.PrimitiveTopology.TriangleStripAdjacency => PrimitiveTopology.TriangleStripWithAdjacency,
                Hyjinx.Graphics.GAL.PrimitiveTopology.Patches => PrimitiveTopology.PatchList,
                Hyjinx.Graphics.GAL.PrimitiveTopology.Polygon => PrimitiveTopology.TriangleFan,
                Hyjinx.Graphics.GAL.PrimitiveTopology.Quads => throw new NotSupportedException("Quad topology is not available in Vulkan."),
                Hyjinx.Graphics.GAL.PrimitiveTopology.QuadStrip => throw new NotSupportedException("QuadStrip topology is not available in Vulkan."),
                _ => LogInvalidAndReturn(topology, nameof(Hyjinx.Graphics.GAL.PrimitiveTopology), PrimitiveTopology.TriangleList),
            };
        }

        public static StencilOp Convert(this Hyjinx.Graphics.GAL.StencilOp op)
        {
            return op switch
            {
                Hyjinx.Graphics.GAL.StencilOp.Keep or Hyjinx.Graphics.GAL.StencilOp.KeepGl => StencilOp.Keep,
                Hyjinx.Graphics.GAL.StencilOp.Zero or Hyjinx.Graphics.GAL.StencilOp.ZeroGl => StencilOp.Zero,
                Hyjinx.Graphics.GAL.StencilOp.Replace or Hyjinx.Graphics.GAL.StencilOp.ReplaceGl => StencilOp.Replace,
                Hyjinx.Graphics.GAL.StencilOp.IncrementAndClamp or Hyjinx.Graphics.GAL.StencilOp.IncrementAndClampGl => StencilOp.IncrementAndClamp,
                Hyjinx.Graphics.GAL.StencilOp.DecrementAndClamp or Hyjinx.Graphics.GAL.StencilOp.DecrementAndClampGl => StencilOp.DecrementAndClamp,
                Hyjinx.Graphics.GAL.StencilOp.Invert or Hyjinx.Graphics.GAL.StencilOp.InvertGl => StencilOp.Invert,
                Hyjinx.Graphics.GAL.StencilOp.IncrementAndWrap or Hyjinx.Graphics.GAL.StencilOp.IncrementAndWrapGl => StencilOp.IncrementAndWrap,
                Hyjinx.Graphics.GAL.StencilOp.DecrementAndWrap or Hyjinx.Graphics.GAL.StencilOp.DecrementAndWrapGl => StencilOp.DecrementAndWrap,
                _ => LogInvalidAndReturn(op, nameof(Hyjinx.Graphics.GAL.StencilOp), StencilOp.Keep),
            };
        }

        public static ComponentSwizzle Convert(this SwizzleComponent swizzleComponent)
        {
            return swizzleComponent switch
            {
                SwizzleComponent.Zero => ComponentSwizzle.Zero,
                SwizzleComponent.One => ComponentSwizzle.One,
                SwizzleComponent.Red => ComponentSwizzle.R,
                SwizzleComponent.Green => ComponentSwizzle.G,
                SwizzleComponent.Blue => ComponentSwizzle.B,
                SwizzleComponent.Alpha => ComponentSwizzle.A,
                _ => LogInvalidAndReturn(swizzleComponent, nameof(SwizzleComponent), ComponentSwizzle.Zero),
            };
        }

        public static ImageType Convert(this Target target)
        {
            return target switch
            {
                Target.Texture1D or
                Target.Texture1DArray or
                Target.TextureBuffer => ImageType.Type1D,
                Target.Texture2D or
                Target.Texture2DArray or
                Target.Texture2DMultisample or
                Target.Cubemap or
                Target.CubemapArray => ImageType.Type2D,
                Target.Texture3D => ImageType.Type3D,
                _ => LogInvalidAndReturn(target, nameof(Target), ImageType.Type2D),
            };
        }

        public static ImageViewType ConvertView(this Target target)
        {
            return target switch
            {
                Target.Texture1D => ImageViewType.Type1D,
                Target.Texture2D or Target.Texture2DMultisample => ImageViewType.Type2D,
                Target.Texture3D => ImageViewType.Type3D,
                Target.Texture1DArray => ImageViewType.Type1DArray,
                Target.Texture2DArray => ImageViewType.Type2DArray,
                Target.Cubemap => ImageViewType.TypeCube,
                Target.CubemapArray => ImageViewType.TypeCubeArray,
                _ => LogInvalidAndReturn(target, nameof(Target), ImageViewType.Type2D),
            };
        }

        public static ImageAspectFlags ConvertAspectFlags(this Format format)
        {
            return format switch
            {
                Format.D16Unorm or Format.D32Float or Format.X8UintD24Unorm => ImageAspectFlags.DepthBit,
                Format.S8Uint => ImageAspectFlags.StencilBit,
                Format.D24UnormS8Uint or
                Format.D32FloatS8Uint or
                Format.S8UintD24Unorm => ImageAspectFlags.DepthBit | ImageAspectFlags.StencilBit,
                _ => ImageAspectFlags.ColorBit,
            };
        }

        public static ImageAspectFlags ConvertAspectFlags(this Format format, DepthStencilMode depthStencilMode)
        {
            return format switch
            {
                Format.D16Unorm or Format.D32Float or Format.X8UintD24Unorm => ImageAspectFlags.DepthBit,
                Format.S8Uint => ImageAspectFlags.StencilBit,
                Format.D24UnormS8Uint or
                Format.D32FloatS8Uint or
                Format.S8UintD24Unorm => depthStencilMode == DepthStencilMode.Stencil ? ImageAspectFlags.StencilBit : ImageAspectFlags.DepthBit,
                _ => ImageAspectFlags.ColorBit,
            };
        }

        public static LogicOp Convert(this LogicalOp op)
        {
            return op switch
            {
                LogicalOp.Clear => LogicOp.Clear,
                LogicalOp.And => LogicOp.And,
                LogicalOp.AndReverse => LogicOp.AndReverse,
                LogicalOp.Copy => LogicOp.Copy,
                LogicalOp.AndInverted => LogicOp.AndInverted,
                LogicalOp.Noop => LogicOp.NoOp,
                LogicalOp.Xor => LogicOp.Xor,
                LogicalOp.Or => LogicOp.Or,
                LogicalOp.Nor => LogicOp.Nor,
                LogicalOp.Equiv => LogicOp.Equivalent,
                LogicalOp.Invert => LogicOp.Invert,
                LogicalOp.OrReverse => LogicOp.OrReverse,
                LogicalOp.CopyInverted => LogicOp.CopyInverted,
                LogicalOp.OrInverted => LogicOp.OrInverted,
                LogicalOp.Nand => LogicOp.Nand,
                LogicalOp.Set => LogicOp.Set,
                _ => LogInvalidAndReturn(op, nameof(LogicalOp), LogicOp.Copy),
            };
        }

        public static BufferAllocationType Convert(this BufferAccess access)
        {
            BufferAccess memType = access & BufferAccess.MemoryTypeMask;

            if (memType == BufferAccess.HostMemory || access.HasFlag(BufferAccess.Stream))
            {
                return BufferAllocationType.HostMapped;
            }
            else if (memType == BufferAccess.DeviceMemory)
            {
                return BufferAllocationType.DeviceLocal;
            }
            else if (memType == BufferAccess.DeviceMemoryMapped)
            {
                return BufferAllocationType.DeviceLocalMapped;
            }

            return BufferAllocationType.Auto;
        }

        private static T2 LogInvalidAndReturn<T1, T2>(T1 value, string name, T2 defaultValue = default)
        {
            Logger.Debug?.Print(LogClass.Gpu, $"Invalid {name} enum value: {value}.");

            return defaultValue;
        }
    }
}
