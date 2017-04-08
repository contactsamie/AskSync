using System;

namespace AskSync.AkkaAskSyncLib.Services
{
    public class AskSyncException : Exception
    {
        public AskSyncException(string message): base(message) { }
    }
}