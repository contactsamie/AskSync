﻿using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Actors;

namespace AskSync.AkkaAskSyncLib
{
    public static class AskExtensions
    {
        internal static ActorSystem ActorSystem { set; get; }

        public static T AskSync<T>(
           this ICanTell actorRef
           , object whatToAsk
           , TimeSpan? timeout = null
           , AskSyncOptions options = null)
        {
            options = options ?? new AskSyncOptions();
            var  result = GetOrCreatedActorSystem(ActorSystem, options, WorkerActor);
            ActorSystem = result.Item1;
            WorkerActor = result.Item2;
            var synchronousAsk = SynchronousAskFactory.GetSynchronousAsk();
            return synchronousAsk.AskSyncInternal<T>(WorkerActor, actorRef, whatToAsk, timeout, options.ExecutionId, SynchronousAskFactory);
        }

        public static IActorRef WorkerActor { get; set; }

        public static object AskSync(
          this ICanTell actorRef
          , object whatToAsk
          , ActorSystem actorSystem)
        {
            if (actorSystem == null) throw new ArgumentNullException(nameof(actorSystem));
            return actorRef.AskSync<object>(whatToAsk, null, new AskSyncOptions()
            {
                ExistingActorSystem = actorSystem
            });
        }

        public static object AskSync(
            this ICanTell actorRef
            , object whatToAsk
            , TimeSpan? timeout
            , ActorSystem actorSystem)
        {
            return actorRef.AskSync<object>(whatToAsk, timeout, new AskSyncOptions()
            {
                ExistingActorSystem = actorSystem
            });
        }
        public static T AskSync<T>(
           this ICanTell actorRef
           , object whatToAsk
           , ActorSystem actorSystem)
        {
            return actorRef.AskSync<T>(whatToAsk, null, new AskSyncOptions()
            {
                ExistingActorSystem = actorSystem
            });
        }
        public static T AskSync<T>(
           this ICanTell actorRef
           , object whatToAsk
           , TimeSpan? timeout
           , ActorSystem actorSystem)
        {
            return actorRef.AskSync<T>(whatToAsk, timeout, new AskSyncOptions()
            {
                ExistingActorSystem = actorSystem
            });
        }
        public static object AskSync(
           this ICanTell actorRef
           , object whatToAsk
           , AskSyncOptions options = null)
        {
            return actorRef.AskSync<object>(whatToAsk, null, options);
        }
        public static object AskSync(
            this ICanTell actorRef
            , object whatToAsk
            , TimeSpan? timeout
            , AskSyncOptions options = null)
        {
            return actorRef.AskSync<object>(whatToAsk, timeout, options);
        }

        private static Tuple<ActorSystem,IActorRef> GetOrCreatedActorSystem(ActorSystem savedSystem, AskSyncOptions options, IActorRef savedWorkeActorRef)
        {
            //todo clear confusion regarding these parameters
            ActorSystem result;
            IActorRef actorRef;
            if (options.ExistingActorSystem != null)
            {
                result = options.ExistingActorSystem;
                actorRef = result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory)));
            }
            else if (options.UseDefaultRemotingActorSystemConfig)
            {
                result = savedSystem ??ActorSystem.Create(SystemName, DefaultRemotingActorSystemConfig(options.DefaultRemotingPort));
                actorRef = savedWorkeActorRef ??result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory)));
            }
            else if (!string.IsNullOrEmpty(options.ActorSystemConfig))
            {
                result = savedSystem ?? ActorSystem.Create(SystemName, options.ActorSystemConfig);
                actorRef = savedWorkeActorRef ??result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory)));
            }
            else
            {
                result = savedSystem ?? ActorSystem.Create(SystemName);
                actorRef = savedWorkeActorRef ??result.ActorOf(Props.Create(() => new AskSyncReceiveActor(SynchronousAskFactory)));
            }
            return new Tuple<ActorSystem, IActorRef>(result, actorRef);
        }

        internal static SynchronousAskFactory SynchronousAskFactory = new SynchronousAskFactory();
        internal static string SystemName = "AskSyncActorSystem-" + Guid.NewGuid();

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
    }
}