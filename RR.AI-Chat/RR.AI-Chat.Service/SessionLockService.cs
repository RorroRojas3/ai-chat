using System.Collections.Concurrent;

namespace RR.AI_Chat.Service
{
    public interface ISessionLockService
    {
        /// <summary>
        /// Checks whether a session is currently locked and busy.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to check.</param>
        /// <returns><c>true</c> if the session is currently locked; otherwise, <c>false</c>.</returns>
        bool IsSessionBusy(Guid sessionId);

        /// <summary>
        /// Acquires an exclusive lock for the specified session, waiting indefinitely if necessary.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to lock.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is an <see cref="IDisposable"/> that releases the lock when disposed.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.</exception>
        Task<IDisposable> AcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to acquire an exclusive lock for the specified session without waiting.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to lock.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is an <see cref="IDisposable"/> that releases the lock when disposed, 
        /// or <c>null</c> if the lock could not be acquired immediately.
        /// </returns>
        Task<IDisposable?> TryAcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default);
    }

    public class SessionLockService : ISessionLockService, IDisposable
    {
        private class LockInfo
        {
            /// <summary>
            /// Gets the semaphore used to control access to the session.
            /// </summary>
            public SemaphoreSlim Semaphore { get; }

            /// <summary>
            /// Gets or sets the last time this lock was accessed.
            /// </summary>
            public DateTime LastAccessed { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LockInfo"/> class.
            /// </summary>
            public LockInfo()
            {
                Semaphore = new SemaphoreSlim(1, 1);
                LastAccessed = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Releases a session lock when disposed.
        /// </summary>
        private class LockReleaser(SessionLockService service, Guid sessionId, SemaphoreSlim semaphore) : IDisposable
        {
            private bool _disposed;

            /// <summary>
            /// Releases the semaphore and updates the last accessed time for the session.
            /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionLockService"/> class.
        /// </summary>
        /// <remarks>
        /// Starts a background timer that runs every 5 minutes to clean up stale locks.
        /// </remarks>
        public SessionLockService()
        {
            _cleanupTimer = new Timer(CleanupStaleLocksCallback, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <inheritdoc />
        public bool IsSessionBusy(Guid sessionId)
        {
            if (_sessionLocks.TryGetValue(sessionId, out var lockInfo))
            {
                return lockInfo.Semaphore.CurrentCount == 0;
            }
            return false;
        }

        /// <inheritdoc />
        public async Task<IDisposable> AcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var lockInfo = GetOrCreateLockInfo(sessionId);
            await lockInfo.Semaphore.WaitAsync(cancellationToken);
            return new LockReleaser(this, sessionId, lockInfo.Semaphore);
        }

        /// <inheritdoc />
        public async Task<IDisposable?> TryAcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var lockInfo = GetOrCreateLockInfo(sessionId);
            var acquired = await lockInfo.Semaphore.WaitAsync(0, cancellationToken);
            return acquired ? new LockReleaser(this, sessionId, lockInfo.Semaphore) : null;
        }

        /// <summary>
        /// Gets or creates lock information for the specified session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <returns>The <see cref="LockInfo"/> associated with the session.</returns>
        private LockInfo GetOrCreateLockInfo(Guid sessionId)
        {
            var lockInfo = _sessionLocks.GetOrAdd(sessionId, _ => new LockInfo());
            lockInfo.LastAccessed = DateTime.UtcNow;
            return lockInfo;
        }

        /// <summary>
        /// Updates the last accessed time for the specified session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        private void UpdateLastAccessed(Guid sessionId)
        {
            if (_sessionLocks.TryGetValue(sessionId, out var lockInfo))
            {
                lockInfo.LastAccessed = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Callback method that removes stale locks that have not been accessed recently and are not currently held.
        /// </summary>
        /// <param name="state">Not used.</param>
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

        /// <summary>
        /// Releases all resources used by the <see cref="SessionLockService"/>.
        /// </summary>
        /// <remarks>
        /// Disposes the cleanup timer and all semaphores, then clears the lock dictionary.
        /// </remarks>
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