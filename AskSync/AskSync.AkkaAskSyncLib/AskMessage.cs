using System.Threading;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    internal class AskMessage
    {
        public string MessageId { get; set; }
        public IActorRef Actor { get; set; }
        public object Message { get; set; }
        public ManualResetEventSlim Signal { get; set; }
    }
}