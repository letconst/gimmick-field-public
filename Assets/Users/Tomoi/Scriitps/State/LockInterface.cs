using System.Threading;

public interface LockInterface
{
    bool isLock { get; set; }
    void unLock(bool _unLockCountSkip);
}
