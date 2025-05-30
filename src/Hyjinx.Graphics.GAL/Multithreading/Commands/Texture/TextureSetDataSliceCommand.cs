using Hyjinx.Common.Memory;
using Hyjinx.Graphics.GAL.Multithreading.Model;
using Hyjinx.Graphics.GAL.Multithreading.Resources;

namespace Hyjinx.Graphics.GAL.Multithreading.Commands.Texture;

struct TextureSetDataSliceCommand : IGALCommand, IGALCommand<TextureSetDataSliceCommand>
{
    public readonly CommandType CommandType => CommandType.TextureSetDataSlice;
    private TableRef<ThreadedTexture> _texture;
    private TableRef<MemoryOwner<byte>> _data;
    private int _layer;
    private int _level;

    public void Set(TableRef<ThreadedTexture> texture, TableRef<MemoryOwner<byte>> data, int layer, int level)
    {
        _texture = texture;
        _data = data;
        _layer = layer;
        _level = level;
    }

    public static void Run(ref TextureSetDataSliceCommand command, ThreadedRenderer threaded, IRenderer renderer)
    {
        ThreadedTexture texture = command._texture.Get(threaded);
        texture.Base.SetData(command._data.Get(threaded), command._layer, command._level);
    }
}