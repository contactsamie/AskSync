using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;
using System.Threading;
using AskSync.AkkaAskSyncLib.Services;

namespace AskSync.AkkaAskSyncLib.Actors
{
    internal class AskSyncReceiveActor : ActorBase
    {
        private SynchronousAskFactory SynchronousAskFactory { set; get; }

       // private ResultData ResultData { set; get; }

        public AskSyncReceiveActor(SynchronousAskFactory synchronousAskFactory/*, ResultData resultData*/)
        {
            SynchronousAskFactory = synchronousAskFactory;
           // ResultData = resultData;
        }

        private string _messageId;
        private ManualResetEventSlim _signal;

        protected override bool Receive(object m)
        {
            if (!(m is AskMessage)) return true;
            var message = (AskMessage)m;
            SynchronousAskFactory.GetCacheService().AddOrUpdate(message.MessageId, message.Actor, null);
            _messageId = message.MessageId;
            _signal = message.Signal;
            message.Actor.Tell(message.Message, Self);
            return true;
        }

        protected override bool AroundReceive(Receive receive, object message)
        {
            if (message is AskMessage)
            {
                return base.AroundReceive(receive, message);
            }
            var cache = SynchronousAskFactory.GetCacheService().Read(_messageId);
            //ResultData.Result = message;
            SynchronousAskFactory.GetCacheService().AddOrUpdate(_messageId, cache.Item1, message);
            _signal.Set();
            Self.Tell(PoisonPill.Instance);
            return true;
        }
    }
}