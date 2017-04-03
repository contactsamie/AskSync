using Akka.Actor;
using System;

namespace AskSync.AkkaAskSyncLib
{
    public static class AskExtensions
    {
        internal static ActorSystem ActorSystem { set; get; }
        internal static SynchronousAskFactory SynchronousAskFactory = new SynchronousAskFactory();
        internal static string SystemName = "AskSyncActorSystem-" + Guid.NewGuid();
        internal static Func<int, string> DefaultRemotingActorSystemConfig = (p) =>
               $@"
                akka {{ 
                    actor {{
                        
                    }}
                    remote {{
                        helios.tcp {{                            
                            transport-protocol = tcp
                            port = {p}
                            hostname = 0.0.0.0
                            #public-hostname = localhost
                            #send-buffer-size = 512000b
                            #receive-buffer-size = 512000b
                            #maximum-frame-size = 1024000b
                            #tcp-keepalive = on
                        }}
                        transport-failure-detector {{
                            #heartbeat-interval = 60 s # default 4s
                            #acceptable-heartbeat-pause = 20 s # default 10s
                        }}
                    }}

                    #stdout-loglevel = DEBUG
                    #loglevel = DEBUG
                    debug {{  
                            #receive = on 
                            #autoreceive = on
                            #lifecycle = on
                            #event-stream = on
                            #unhandled = on
                    }}
                }}";
        public static object AskSync(
            this ICanTell actorRef
            , object whatToAsk
            , TimeSpan? timeout = null
            , AskSyncOptions options = null)
        {
            return actorRef.AskSync<object>(whatToAsk, timeout, options);
        }

        public static T AskSync<T>(
            this ICanTell actorRef
          , object whatToAsk
          , TimeSpan? timeout = null
          , AskSyncOptions options = null)
        {
            options = options ?? new AskSyncOptions();
            ActorSystem= ActorSystem?? GetOrCreatedActorSystem(options);
            var cacheService = SynchronousAskFactory.GetCacheService();
            var synchronousAsk = SynchronousAskFactory.GetSynchronousAsk();
            return synchronousAsk.AskSyncInternal<T>(ActorSystem, actorRef, whatToAsk, timeout, options.ExecutionId, SynchronousAskFactory);
        }

        private static ActorSystem GetOrCreatedActorSystem(AskSyncOptions options)
        {
            return  (options.UseDefaultRemotingActorSystemConfig
                ? (Akka.Actor.ActorSystem.Create(SystemName
                    , DefaultRemotingActorSystemConfig(options.DefaultRemotingPort)))
                : (string.IsNullOrEmpty(options.ActorSystemConfig)
                    ? Akka.Actor.ActorSystem.Create(SystemName)
                    : Akka.Actor.ActorSystem.Create(SystemName, options.ActorSystemConfig)));
        }
    }
}