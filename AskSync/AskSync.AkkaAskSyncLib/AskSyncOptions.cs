using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    public class AskSyncOptions
    {
        public AskSyncOptions()
        {
            DefaultRemotingPort = 0;
        }

        public string ExecutionId { set; get; }
        public string ActorSystemConfig { set; get; }
        public bool UseDefaultRemotingActorSystemConfig { set; get; }
        public int DefaultRemotingPort { set; get; }
        public ActorSystem ExistingActorSystem { get; set; }
        /// <summary>
        /// Under high workload, feel free to increase the WorkerActorPoolSize for massive improvement in throuput!
        /// </summary>
        public int WorkerActorPoolSize { get; set; }
    }
}