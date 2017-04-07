using System.Threading;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib.Messages
{
    internal class AskMessage
    {
        public AskMessage(string messageId, ICanTell actor, object message, ManualResetEventSlim signal, int workerActorPoolSize)
        {
            MessageId = messageId;
            ActorRef = actor;
            Message = message;
            Signal = signal;
            WorkerActorPoolSize = workerActorPoolSize;
        }

        public string MessageId { get; private set; }
        public ICanTell ActorRef { get; private set; }
        public object Message { get; private set; }
        public ManualResetEventSlim Signal { get; private set; }
        public int WorkerActorPoolSize { get; private set; }
    }
}