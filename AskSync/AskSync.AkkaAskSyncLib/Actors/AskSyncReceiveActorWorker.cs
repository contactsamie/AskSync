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
                SynchronousAskFactory.GetCacheService().AddOrUpdate(message.MessageId, message, message.WhenonlyretrayableIdentifyFailsMessage);
                _messageId = message.MessageId;
             
            }
           
            if (!RetryStarted && _identifyBeforeSending)
            {
                var cache = SynchronousAskFactory.GetCacheService().Read(_messageId);
                TryIdentifyAndSendUsingRetryMechanism(m as ActorIdentity, cache);
            }
            else if(_identifyBeforeSending)
            {
                if (resolvedActorRef == null)
                {
                    throw new Exception();
                }
                (resolvedActorRef ).Tell(m, Self);
            }
            else
            {
                if (message == null)
                {
                    throw new Exception();
                }
                ( message.ActorRef).Tell(message.Message, Self);
            }
           
           
            return true;
        }

        public bool RetryStarted { get; set; }

        private int _retryIdentificationCount;
        private int _retryIdentificationCountDown;
        private bool _identifyBeforeSending ;
        protected override bool AroundReceive(Receive receive, object message)
        {
            if (message is AskMessage ||
                (message is ActorIdentity &&
                ((ActorIdentity)message)?.MessageId?.ToString() == RetryMessageId))
            {
                return base.AroundReceive(receive, message);
            }
            var cache = SynchronousAskFactory.GetCacheService().Read(_messageId);
            FinalizeActorProcessing(message, cache);
            return true;
        }
        
        private readonly Func<int,TimeSpan> _defaultCalculateTimeBeforeRetry=(i)=>TimeSpan.FromMilliseconds(i*500);
        private void TryIdentifyAndSendUsingRetryMechanism(ActorIdentity message, Tuple<AskMessage, object> cache )
        {
          var calculateTimeBeforeRetry = cache.Item1.CalculateTimeBeforeRetry ?? _defaultCalculateTimeBeforeRetry;
            if (_retryIdentificationCountDown >= 0)
            {
              
                var subject = message?.Subject;
                if (subject != null)
                {
                    RetryStarted = true;
                    HandleReceive(cache.Item1.Message, subject);
                }
                else
                {
                    _retryIdentificationCountDown--;
                    RetryStarted = false;
                    Context.System.Scheduler.ScheduleTellOnce(
                        calculateTimeBeforeRetry(_retryIdentificationCount-_retryIdentificationCountDown-1)
                        , cache.Item1.ActorRef
                        , new Identify(RetryMessageId)
                        , Self);
                }
            }
            else
            {
                FinalizeActorProcessing(cache.Item1.WhenonlyretrayableIdentifyFailsMessage, cache);
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