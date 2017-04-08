namespace AskSync.AkkaAskSyncLib.Services
{
    public class AskSyncRetryableTimeoutException : AskSyncException
    {
        public AskSyncRetryableTimeoutException(string message) : base(message)
        {
        }
    }
}