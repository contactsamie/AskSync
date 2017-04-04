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
    }
}