using System;

namespace Hyjinx.Graphics.GAL;

public interface IProgram : IDisposable
{
    ProgramLinkStatus CheckProgramLink(bool blocking);

    byte[] GetBinary();
}