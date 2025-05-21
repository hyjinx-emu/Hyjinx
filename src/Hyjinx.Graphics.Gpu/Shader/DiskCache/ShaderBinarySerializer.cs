using Hyjinx.Common;
using Hyjinx.Common.Memory;
using Hyjinx.Graphics.GAL;
using Hyjinx.Graphics.Shader;
using Hyjinx.Graphics.Shader.Translation;
using System.Collections.Generic;
using System.IO;

namespace Hyjinx.Graphics.Gpu.Shader.DiskCache;

static class ShaderBinarySerializer
{
    public static byte[] Pack(ShaderSource[] sources)
    {
        using MemoryStream output = MemoryStreamManager.Shared.GetStream();

        output.Write(sources.Length);

        foreach (ShaderSource source in sources)
        {
            output.Write((int)source.Stage);
            output.Write(source.BinaryCode.Length);
            output.Write(source.BinaryCode);
        }

        return output.ToArray();
    }

    public static ShaderSource[] Unpack(CachedShaderStage[] stages, byte[] code)
    {
        using MemoryStream input = new(code);
        using BinaryReader reader = new(input);

        List<ShaderSource> output = new();

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            ShaderStage stage = (ShaderStage)reader.ReadInt32();
            int binaryCodeLength = reader.ReadInt32();
            byte[] binaryCode = reader.ReadBytes(binaryCodeLength);

            output.Add(new ShaderSource(binaryCode, stage, TargetLanguage.Spirv));
        }

        return output.ToArray();
    }
}