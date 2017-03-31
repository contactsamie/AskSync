using Akka.Actor;
using System.Threading;
using AskSync.AkkaAskSyncLib.Services;

namespace AskSync.AkkaAskSyncLib.Messages
{
    internal class AskMessage
    {
        public AskMessage(
            string messageId
          , IActorRef actor
          , object message
          , ManualResetEventSlim signal
         )
        {
            MessageId = messageId;
            Actor = actor;
            Message = message;
            Signal = signal;
         
        }

        public string MessageId { get; private set; }
        public IActorRef Actor { get; private set; }
        public object Message { get; private set; }
        public ManualResetEventSlim Signal { get; private set; }
        
    }
}