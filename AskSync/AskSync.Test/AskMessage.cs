using Akka.Actor;

namespace AskSync.Test
{
    public class AskMessage
    {
        public string messageId;
        public IActorRef Actor;
        public object Message { get; set; }
    }
}