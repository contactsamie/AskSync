
using System.Collections.Generic;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Actors
{
    internal class AskSyncReceiveActor : ReceiveActor
    {
        private readonly Stack<IActorRef> _oneOffWorkerActors = new Stack<IActorRef>();
        public AskSyncReceiveActor(SynchronousAskFactory synchronousAskFactory)
        {
            int bufferActorCount = 10;
            var props = Props.Create(() => new AskSyncReceiveActorWorker(synchronousAskFactory));
            RebuildOneOffActorStack(bufferActorCount, props);
            Receive<AskMessage>(message =>
            {
                var workerActor = _oneOffWorkerActors.Pop();
                workerActor.Forward(message);
                if (_oneOffWorkerActors.Count == 0)
                {
                    RebuildOneOffActorStack(bufferActorCount, props);
                }
            });
        }

        private void RebuildOneOffActorStack(int bufferActorCount, Props props)
        {
            for (var i = 0; i < bufferActorCount; i++)
            {
                _oneOffWorkerActors.Push(Context.System.ActorOf(props));
            }
        }

    }
}

/*using Akka.Actor;
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


 
     
     */
