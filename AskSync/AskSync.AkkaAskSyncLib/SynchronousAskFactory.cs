using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Services;

namespace AskSync.AkkaAskSyncLib
{
    internal class SynchronousAskFactory
    {
        private readonly IAskSynchronously _askSynchronously = new DefaultAskSynchronously();
        // private static readonly IAskSynchronously AskSynchronously = new NoLockingAskSynchronously();
        // private static readonly ICacheService CacheService = new ConcurrentDictionaryCacheService();
        private readonly ICacheService _cacheService = new SynchronizedCacheService();

        public IAskSynchronously GetSynchronousAsk()
        {
            return _askSynchronously;
        }

        public ICacheService GetCacheService()
        {
            return _cacheService;
        }
    }
}