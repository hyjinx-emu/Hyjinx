using Hyjinx.Common;
using Hyjinx.Graphics.GAL;
using Hyjinx.Graphics.Shader;
using Hyjinx.Graphics.Shader.Translation;
using Silk.NET.Vulkan;
using System;
using Extent2D = Hyjinx.Graphics.GAL.Extents2D;
using Format = Silk.NET.Vulkan.Format;
using SamplerCreateInfo = Hyjinx.Graphics.GAL.SamplerCreateInfo;

namespace Hyjinx.Graphics.Vulkan.Effects;

internal class AreaScalingFilter : IScalingFilter
{
    private readonly VulkanRenderer _renderer;
    private PipelineHelperShader _pipeline;
    private ISampler _sampler;
    private ShaderCollection _scalingProgram;
    private Device _device;

    public float Level { get; set; }

    public AreaScalingFilter(VulkanRenderer renderer, Device device)
    {
        _device = device;
        _renderer = renderer;

        Initialize();
    }

    public void Dispose()
    {
        _pipeline.Dispose();
        _scalingProgram.Dispose();
        _sampler.Dispose();
    }

    public void Initialize()
    {
        _pipeline = new PipelineHelperShader(_renderer, _device);

        _pipeline.Initialize();

        var scalingShader = EmbeddedResources.Read("Hyjinx.Graphics.Vulkan/Effects/Shaders/AreaScaling.spv");

        var scalingResourceLayout = new ResourceLayoutBuilder()
            .Add(ResourceStages.Compute, ResourceType.UniformBuffer, 2)
            .Add(ResourceStages.Compute, ResourceType.TextureAndSampler, 1)
            .Add(ResourceStages.Compute, ResourceType.Image, 0, true).Build();

        _sampler = _renderer.CreateSampler(SamplerCreateInfo.Create(MinFilter.Linear, MagFilter.Linear));

        _scalingProgram = _renderer.CreateProgramWithMinimalLayout(new[]
        {
            new ShaderSource(scalingShader, ShaderStage.Compute, TargetLanguage.Spirv),
        }, scalingResourceLayout);
    }

    public void Run(
        TextureView view,
        CommandBufferScoped cbs,
        Auto<DisposableImageView> destinationTexture,
        Format format,
        int width,
        int height,
        Extent2D source,
        Extent2D destination)
    {
        _pipeline.SetCommandBuffer(cbs);
        _pipeline.SetProgram(_scalingProgram);
        _pipeline.SetTextureAndSampler(ShaderStage.Compute, 1, view, _sampler);

        ReadOnlySpan<float> dimensionsBuffer = stackalloc float[]
        {
            source.X1,
            source.X2,
            source.Y1,
            source.Y2,
            destination.X1,
            destination.X2,
            destination.Y1,
            destination.Y2,
        };

        int rangeSize = dimensionsBuffer.Length * sizeof(float);
        using var buffer = _renderer.BufferManager.ReserveOrCreate(_renderer, cbs, rangeSize);
        buffer.Holder.SetDataUnchecked(buffer.Offset, dimensionsBuffer);

        int threadGroupWorkRegionDim = 16;
        int dispatchX = (width + (threadGroupWorkRegionDim - 1)) / threadGroupWorkRegionDim;
        int dispatchY = (height + (threadGroupWorkRegionDim - 1)) / threadGroupWorkRegionDim;

        _pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(2, buffer.Range) });
        _pipeline.SetImage(0, destinationTexture);
        _pipeline.DispatchCompute(dispatchX, dispatchY, 1);
        _pipeline.ComputeBarrier();

        _pipeline.Finish();
    }
}