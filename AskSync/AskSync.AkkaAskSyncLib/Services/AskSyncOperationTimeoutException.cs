namespace AskSync.AkkaAskSyncLib.Services
{
    public class AskSyncOperationTimeoutException : AskSyncException
    {
        public AskSyncOperationTimeoutException(string message) : base(message)
        {
        }
    }
}