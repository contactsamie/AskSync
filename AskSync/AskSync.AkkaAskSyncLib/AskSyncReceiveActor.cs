using System;
using System.Threading;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    internal class AskSyncReceiveActor : ActorBase
    {
        private string _messageId = null;
        private ManualResetEventSlim _signal;
        protected override bool Receive(object m)
        {
            if (!(m is AskMessage)) return true;
            var message = (AskMessage) m;
            CacheFactory. Cache.Add(message.MessageId, new Tuple<IActorRef, object>(message.Actor,null));
            _messageId = message.MessageId;
            _signal = message.Signal;
            message.Actor.Tell(message.Message,Self);
            return true;
        }
        protected override bool AroundReceive(Receive receive, object message)
        {
            if (message is AskMessage)
            {
                return base.AroundReceive(receive, message);
            }
            var cache = CacheFactory.Cache.Read(_messageId);
            CacheFactory.Cache.AddOrUpdate(_messageId, new Tuple<IActorRef, object>(cache.Item1, message));
            _signal.Set();
            Self.Tell(PoisonPill.Instance);
            return true;
        }
    }
}