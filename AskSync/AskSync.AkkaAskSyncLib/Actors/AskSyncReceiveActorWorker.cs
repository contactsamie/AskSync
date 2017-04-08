using System;
using System.Threading;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Actors
{
    internal class AskSyncReceiveActorWorker : ActorBase
    {
        private string _messageId;
        
        public AskSyncReceiveActorWorker(SynchronousAskFactory synchronousAskFactory)
        {
            RetryMessageId =  Guid.NewGuid().ToString();
            SynchronousAskFactory = synchronousAskFactory;
        }

        private SynchronousAskFactory SynchronousAskFactory { get; }

        protected override bool Receive(object m)
        {
            return HandleReceive(m);
        }

        private bool HandleReceive(object m,ICanTell resolvedActorRef=null)
        {
            var askMessage = m as AskMessage;
            AskMessage message=null;
            if (askMessage != null)
            {
                 message = askMessage;
                _retryIdentificationCountDown = message.RetryIdentificationCount ?? 1;
                _retryIdentificationCount = _retryIdentificationCountDown;
                _identifyBeforeSending = message.IdentifyBeforeSending;
                SynchronousAskFactory.GetCacheService().AddOrUpdate(message.MessageId, message, null);
                _messageId = message.MessageId;
             
            }
            var useRetryMechanism = _identifyBeforeSending &&(m is ActorIdentity && ((ActorIdentity) m)?.MessageId?.ToString() == RetryMessageId);
            if (useRetryMechanism )
            {
                var cache = SynchronousAskFactory.GetCacheService().Read(_messageId);
                TryIdentifyAndSendUsingRetryMechanism(m, cache);
            }
            else
            {
                if (message == null)
                {
                    throw new Exception();
                }
                (resolvedActorRef ?? message.ActorRef).Tell(message.Message, Self);
            }
           
           
            return true;
        }
        private int _retryIdentificationCount;
        private int _retryIdentificationCountDown;
        private bool _identifyBeforeSending ;
        protected override bool AroundReceive(Receive receive, object message)
        {
            if (message is AskMessage)
            {
                return base.AroundReceive(receive, message);
            }
            var cache = SynchronousAskFactory.GetCacheService().Read(_messageId);
            FinalizeActorProcessing(message, cache);
            return true;
        }
        
        private readonly Func<int,TimeSpan> _defaultCalculateTimeBeforeRetry=(i)=>TimeSpan.FromMilliseconds(i*500);
        private void TryIdentifyAndSendUsingRetryMechanism(object message, Tuple<AskMessage, object> cache )
        {
          var calculateTimeBeforeRetry = cache.Item1.CalculateTimeBeforeRetry ?? _defaultCalculateTimeBeforeRetry;
            if (_retryIdentificationCountDown > 0)
            {
                _retryIdentificationCountDown--;
                var subject = ((ActorIdentity)message).Subject;
                if (subject != null)
                {
                    HandleReceive(cache.Item2, subject);
                }
                else
                {
                    Context.System.Scheduler.ScheduleTellOnce(
                        calculateTimeBeforeRetry(_retryIdentificationCount-_retryIdentificationCountDown)
                        , cache.Item1.ActorRef
                        , new Identify(RetryMessageId)
                        , Self);
                }
            }
            else
            {
                FinalizeActorProcessing(null, cache);
            }
           
        }

        public string RetryMessageId { get; set; }

        private void FinalizeActorProcessing(object message, Tuple<AskMessage, object> cache)
        {
            SynchronousAskFactory.GetCacheService().AddOrUpdate(_messageId, cache.Item1, message);
            //calling signal this way  rather than saving it as a state , slows down the system by 1/3 
            cache.Item1.Signal.Set();
            Self.Tell(PoisonPill.Instance);
        }
    }
}