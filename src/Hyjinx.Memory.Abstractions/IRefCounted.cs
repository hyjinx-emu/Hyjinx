namespace Hyjinx.Memory;

public interface IRefCounted
{
    void IncrementReferenceCount();
    void DecrementReferenceCount();
}