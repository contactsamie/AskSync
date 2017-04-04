using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Services;

namespace AskSync.AkkaAskSyncLib
{
    internal class SynchronousAskFactory
    {
        private static readonly IAskSynchronously AskSynchronously = new DefaultAskSynchronously();
        // private static readonly IAskSynchronously AskSynchronously = new NoLockingAskSynchronously();
        // private static readonly ICacheService CacheService = new ConcurrentDictionaryCacheService();
        private static readonly ICacheService CacheService = new SynchronizedCacheService();

        public IAskSynchronously GetSynchronousAsk()
        {
            return AskSynchronously;
        }

        public ICacheService GetCacheService()
        {
            return CacheService;
        }
    }
}