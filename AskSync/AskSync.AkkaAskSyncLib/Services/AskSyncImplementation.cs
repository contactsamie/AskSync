using System;
using Akka.Actor;
using Akka.Routing;
using AskSync.AkkaAskSyncLib.Actors;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal  class AskSyncImplementation
    {
        internal static T AskSyncImpl<T>(
            ICanTell actorRef
            , object whatToAsk
            , TimeSpan? timeout = null
            , AskSyncOptions options = null)
        {
            //var makeThreadSafe = true;
            //if (makeThreadSafe)
            //{
            //    WorkerActor = null;
            //    ActorSystem = null;
            //}

        options = options ?? new AskSyncOptions();
            ActorSystem = GetOrCreatedActorSystem(
                ActorSystem
                , options
                , WorkerActor
                );

          WorkerActor = options.ExistingActorSystem != null || WorkerActor == null
              ? ActorSystem
              .ActorOf(Props.Create(() => new AskSyncCoOrdinatorActor(SynchronousAskFactory))
              /*.WithRouter(new RoundRobinPool(100))*/
              )
              : WorkerActor;

            var synchronousAsk = SynchronousAskFactory.GetSynchronousAsk();

            return synchronousAsk.AskSyncInternal<T>(
                WorkerActor
                , actorRef
                , whatToAsk
                , timeout
                , options.ExecutionId
                , options.WorkerActorPoolSize
                , SynchronousAskFactory
                , options.RetryIdentificationCount
                , options.IdentifyBeforeSending
                , options.CalculateTimeBeforeRetry
                );

        }
        internal static SynchronousAskFactory SynchronousAskFactory = new SynchronousAskFactory();
        internal static string SystemName = "AskSyncActorSystem-" ;
        internal static Func<int, string> DefaultRemotingActorSystemConfig = p =>
            $@"
                akka {{ 
                    actor {{
                        provider =""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }}
                    remote {{
                        helios.tcp {{                            
                            transport-protocol = tcp
                            port = {p}
                            hostname = localhost
                        }}
                    }}
                }}";

        internal static IActorRef WorkerActor { get; set; }
        internal static ActorSystem ActorSystem { set; get; }
        internal static ActorSystem GetOrCreatedActorSystem(
            ActorSystem savedSystem
            , AskSyncOptions options
            , IActorRef savedWorkeActorRef
            )
        {
            //todo clear confusion regarding these parameters
            ActorSystem result;

            if (options.ExistingActorSystem != null)
            {
                result = options.ExistingActorSystem;
            }
            else if (options.UseDefaultRemotingActorSystemConfig)
            {
                result = savedSystem ?? ActorSystem.Create(SystemName + Guid.NewGuid(), DefaultRemotingActorSystemConfig(options.DefaultRemotingPort));
            }
            else if (!string.IsNullOrEmpty(options.ActorSystemConfig))
            {
                result = savedSystem ?? ActorSystem.Create(SystemName + Guid.NewGuid(), options.ActorSystemConfig);
            }
            else
            {
                result = savedSystem ?? ActorSystem.Create(SystemName + Guid.NewGuid());
            }
            return result;
        }
    }
}
/*
 internal static Func<int, string> DefaultRemotingActorSystemConfig = p =>
            $@"
                akka {{ 
                    actor {{
                        provider =""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }}
                    remote {{
                        helios.tcp {{                            
                            transport-protocol = tcp
                            port = {p}
                            hostname = localhost
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
     
*/
