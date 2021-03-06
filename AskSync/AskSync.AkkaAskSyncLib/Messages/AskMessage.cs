using System;
using System.Threading;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib.Messages
{
    internal class AskMessage
    {
        public AskMessage(
            string messageId
            , ICanTell actor
            , object message
            , ManualResetEventSlim signal
            , int workerActorPoolSize
            , int? retryIdentificationCount
            , bool identifyBeforeSending
            , Func<int, TimeSpan> calculateTimeBeforeRetry
            , string whenonlyretrayableIdentifyFailsMessage)
        {
            MessageId = messageId;
            ActorRef = actor;
            Message = message;
            Signal = signal;
            WorkerActorPoolSize = workerActorPoolSize;
            RetryIdentificationCount = retryIdentificationCount;
            IdentifyBeforeSending = identifyBeforeSending;
            CalculateTimeBeforeRetry = calculateTimeBeforeRetry;
            WhenonlyretrayableIdentifyFailsMessage = whenonlyretrayableIdentifyFailsMessage;
        }

        public string MessageId { get; private set; }
        public ICanTell ActorRef { get; private set; }
        public object Message { get; private set; }
        public ManualResetEventSlim Signal { get; private set; }
        public int WorkerActorPoolSize { get; private set; }
        public int? RetryIdentificationCount { get; private set; }
        public bool IdentifyBeforeSending { get; private set; }
        public Func<int, TimeSpan> CalculateTimeBeforeRetry { get; private set; }
        public string WhenonlyretrayableIdentifyFailsMessage { get; private set; }
    }
}