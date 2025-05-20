using System.Collections.Generic;

namespace Hyjinx.Horizon.Sdk.Sf;

interface IServiceObject
{
    IReadOnlyDictionary<int, CommandHandler> GetCommandHandlers();
}