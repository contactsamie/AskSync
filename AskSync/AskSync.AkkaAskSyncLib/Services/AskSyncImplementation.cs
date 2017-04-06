using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Actors;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal static class AskSyncImplementation
    {
        internal static T AskSyncImpl<T>(
            ICanTell actorRef
            , object whatToAsk
            , TimeSpan? timeout = null
            , AskSyncOptions options = null)
        {
            options = options ?? new AskSyncOptions();
            var result = GetOrCreatedActorSystem(
                ActorSystem
                , options
                , WorkerActor
                );

            ActorSystem = result.Item1;
            WorkerActor = result.Item2;
            var synchronousAsk = SynchronousAskFactory.GetSynchronousAsk();

            return synchronousAsk.AskSyncInternal<T>(
                WorkerActor
                , actorRef
                , whatToAsk
                , timeout
                , options.ExecutionId
                , SynchronousAskFactory
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

        internal static IActorRef WorkerActor { get; set; }

        internal static ActorSystem ActorSystem { set; get; }

        internal static Tuple<ActorSystem, IActorRef> GetOrCreatedActorSystem(
            ActorSystem savedSystem
            , AskSyncOptions options
            , IActorRef savedWorkeActorRef
            )
        {
            //todo clear confusion regarding these parameters
            ActorSystem result;
            IActorRef actorRef;
            options = options ?? new AskSyncOptions();
            if (options.ExistingActorSystem != null)
            {
                result = options.ExistingActorSystem;
                actorRef = result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory/*, options.ActorBufferSize*/)));
            }
            else if (options.UseDefaultRemotingActorSystemConfig)
            {
                result = savedSystem ?? ActorSystem.Create(SystemName + Guid.NewGuid(), DefaultRemotingActorSystemConfig(options.DefaultRemotingPort));
                actorRef = savedWorkeActorRef ?? result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory/*, options.ActorBufferSize*/)));
            }
            else if (!string.IsNullOrEmpty(options.ActorSystemConfig))
            {
                result = savedSystem ?? ActorSystem.Create(SystemName + Guid.NewGuid(), options.ActorSystemConfig);
                actorRef = savedWorkeActorRef ?? result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory/*, options.ActorBufferSize*/)));
            }
            else
            {
                result = savedSystem ?? ActorSystem.Create(SystemName + Guid.NewGuid());
                actorRef = savedWorkeActorRef ?? result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory/*, options.ActorBufferSize*/)));
            }
            return new Tuple<ActorSystem, IActorRef>(result, actorRef);
        }
    }
}