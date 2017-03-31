using System.Collections.Generic;
using System.Threading;

namespace AskSync.AkkaAskSyncLib.Common
{
    /// <summary>
    ///     Represents a lock that is used to manage access to a resource, allowing multiple threads for reading or exclusive
    ///     access for writing.
    ///     Ensure that TVal overrides Equals which will be used in implementing AddOrUpdate
    ///     todo checkout this claim https://www.codeproject.com/Articles/548406/Dictionary-plus-Locking-versus-ConcurrentDictionar
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    internal class SynchronizedCache<TKey, TVal>
    {
        public SynchronizedCache()
        {
        }

        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        }

        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, TVal> _innerCache = new Dictionary<TKey, TVal>();

        public int Count => _innerCache.Count;

        public TVal Read(TKey key)
        {
            _cacheLock.EnterReadLock();
            try
            {
                return _innerCache[key];
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        public void Add(TKey key, TVal value)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _innerCache.Add(key, value);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public bool AddWithTimeout(TKey key, TVal value, int timeout)
        {
            if (_cacheLock.TryEnterWriteLock(timeout))
            {
                try
                {
                    _innerCache.Add(key, value);
                }
                finally
                {
                    _cacheLock.ExitWriteLock();
                }
                return true;
            }
            return false;
        }

        public AddOrUpdateStatus AddOrUpdate(TKey key, TVal value)
        {
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                TVal result;
                if (_innerCache.TryGetValue(key, out result))
                {
                    if (Equals(result, value))
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    else
                    {
                        _cacheLock.EnterWriteLock();
                        try
                        {
                            _innerCache[key] = value;
                        }
                        finally
                        {
                            _cacheLock.ExitWriteLock();
                        }
                        return AddOrUpdateStatus.Updated;
                    }
                }
                else
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        _innerCache.Add(key, value);
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }

        public void Delete(TKey key)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _innerCache.Remove(key);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        ~SynchronizedCache()
        {
            _cacheLock?.Dispose();
        }

        public bool Contains(TKey id)
        {
            return _innerCache.ContainsKey(id);
        }
    }
}