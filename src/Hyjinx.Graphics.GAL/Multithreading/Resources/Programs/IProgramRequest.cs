namespace Hyjinx.Graphics.GAL.Multithreading.Resources.Programs
{
    interface IProgramRequest
    {
        ThreadedProgram Threaded { get; set; }
        IProgram Create(IRenderer renderer);
    }
}