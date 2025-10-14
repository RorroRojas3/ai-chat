using System.Collections.Concurrent;

namespace RR.AI_Chat.Service
{
    public interface ISessionLockService
    {
        bool IsSessionBusy(Guid sessionId);
        Task<IDisposable> AcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default);
        Task<IDisposable?> TryAcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default);
    }

    public class SessionLockService : ISessionLockService, IDisposable
    {
        private class LockInfo
        {
            public SemaphoreSlim Semaphore { get; }
            public DateTime LastAccessed { get; set; }

            public LockInfo()
            {
                Semaphore = new SemaphoreSlim(1, 1);
                LastAccessed = DateTime.UtcNow;
            }
        }

        private class LockReleaser(SessionLockService service, Guid sessionId, SemaphoreSlim semaphore) : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                semaphore.Release();
                service.UpdateLastAccessed(sessionId);
            }
        }

        private readonly ConcurrentDictionary<Guid, LockInfo> _sessionLocks = new();
        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _lockExpirationTime = TimeSpan.FromMinutes(10);
        private bool _disposed;

        public SessionLockService()
        {
            _cleanupTimer = new Timer(CleanupStaleLocksCallback, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public bool IsSessionBusy(Guid sessionId)
        {
            if (_sessionLocks.TryGetValue(sessionId, out var lockInfo))
            {
                return lockInfo.Semaphore.CurrentCount == 0;
            }
            return false;
        }

        public async Task<IDisposable> AcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var lockInfo = GetOrCreateLockInfo(sessionId);
            await lockInfo.Semaphore.WaitAsync(cancellationToken);
            return new LockReleaser(this, sessionId, lockInfo.Semaphore);
        }

        public async Task<IDisposable?> TryAcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var lockInfo = GetOrCreateLockInfo(sessionId);
            var acquired = await lockInfo.Semaphore.WaitAsync(0, cancellationToken);
            return acquired ? new LockReleaser(this, sessionId, lockInfo.Semaphore) : null;
        }

        private LockInfo GetOrCreateLockInfo(Guid sessionId)
        {
            var lockInfo = _sessionLocks.GetOrAdd(sessionId, _ => new LockInfo());
            lockInfo.LastAccessed = DateTime.UtcNow;
            return lockInfo;
        }

        private void UpdateLastAccessed(Guid sessionId)
        {
            if (_sessionLocks.TryGetValue(sessionId, out var lockInfo))
            {
                lockInfo.LastAccessed = DateTime.UtcNow;
            }
        }

        private void CleanupStaleLocksCallback(object? state)
        {
            var now = DateTime.UtcNow;
            var staleKeys = _sessionLocks
                .Where(kvp => kvp.Value.Semaphore.CurrentCount > 0 && // Not currently locked
                             now - kvp.Value.LastAccessed > _lockExpirationTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in staleKeys)
            {
                if (_sessionLocks.TryRemove(key, out var lockInfo))
                {
                    lockInfo.Semaphore.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cleanupTimer?.Dispose();

            foreach (var lockInfo in _sessionLocks.Values)
            {
                lockInfo.Semaphore.Dispose();
            }
            _sessionLocks.Clear();

            GC.SuppressFinalize(this);
        }
    }
}