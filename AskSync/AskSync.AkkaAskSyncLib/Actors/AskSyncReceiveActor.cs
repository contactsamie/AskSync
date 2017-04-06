
using System.Collections.Generic;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Actors
{
    internal class AskSyncReceiveActor : ReceiveActor
    {
        private readonly Stack<IActorRef> _oneOffWorkerActors = new Stack<IActorRef>();
        public AskSyncReceiveActor(SynchronousAskFactory synchronousAskFactory,int workerActorPoolSize = 10)
        {
            WorkerActorPoolSize = workerActorPoolSize;
            var props = Props.Create(() => new AskSyncReceiveActorWorker(synchronousAskFactory));
            RebuildOneOffActorStack(WorkerActorPoolSize, props);
            Receive<AskMessage>(message =>
            {
                var workerActor = _oneOffWorkerActors.Pop();
                workerActor.Forward(message);
                if (_oneOffWorkerActors.Count == 0)
                {
                    RebuildOneOffActorStack(WorkerActorPoolSize, props);
                }
            });
        }

        public int WorkerActorPoolSize { get; set; }

        private void RebuildOneOffActorStack(int bufferActorCount, Props props)
        {
            for (var i = 0; i < bufferActorCount+1; i++)
            {
                _oneOffWorkerActors.Push(Context.System.ActorOf(props));
            }
        }

    }
}

