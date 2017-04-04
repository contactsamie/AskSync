using System.Threading;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Actors
{
    internal class AskSyncReceiveActor : ActorBase
    {
        private string _messageId;
        private ManualResetEventSlim _signal;

        public AskSyncReceiveActor(SynchronousAskFactory synchronousAskFactory)
        {
            SynchronousAskFactory = synchronousAskFactory;
        }

        private SynchronousAskFactory SynchronousAskFactory { get; }

        protected override bool Receive(object m)
        {
            if (!(m is AskMessage)) return true;
            var message = (AskMessage) m;
            SynchronousAskFactory.GetCacheService().AddOrUpdate(message.MessageId, message.ActorRef, null);
            _messageId = message.MessageId;
            _signal = message.Signal;

            message.ActorRef.Tell(message.Message, Self);


            return true;
        }

        protected override bool AroundReceive(Receive receive, object message)
        {
            if (message is AskMessage)
            {
                return base.AroundReceive(receive, message);
            }
            var cache = SynchronousAskFactory.GetCacheService().Read(_messageId);
            SynchronousAskFactory.GetCacheService().AddOrUpdate(_messageId, cache.Item1, message);
            _signal.Set();
            Self.Tell(PoisonPill.Instance);
            return true;
        }
    }
}