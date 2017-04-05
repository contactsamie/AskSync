using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Actors
{
    internal class AskSyncReceiveActor : ReceiveActor
    {
        public AskSyncReceiveActor(SynchronousAskFactory synchronousAskFactory)
        {
            Receive<AskMessage>(message =>
            {
                var props = Props.Create(() => new AskSyncReceiveActorWorker(synchronousAskFactory));
                var workerActor = Context.System.ActorOf(props);
                workerActor.Forward(message);
            });
        }

        public IActorRef WorkerActor { get; set; }
    }
}