using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Services;

namespace AskSync.AkkaAskSyncLib
{
    internal class SynchronousAskFactory
    {
        private static IAskSynchronously AskSynchronously { set; get; }
        private static readonly ICacheService CacheService = new ConcurrentDictionaryCacheService();

        public IAskSynchronously GetSynchronousAsk()
        {
            AskSynchronously = AskSynchronously ?? new DefaultAskSynchronously();
            return AskSynchronously;
        }

        public ICacheService GetCacheService()
        {
            return CacheService;
        }
    }
}