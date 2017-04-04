﻿using System;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    public static class AskExtensions
    {
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

        internal static ActorSystem ActorSystem { set; get; }

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
            ActorSystem = GetOrCreatedActorSystem(ActorSystem, options);
            var synchronousAsk = SynchronousAskFactory.GetSynchronousAsk();
            return synchronousAsk.AskSyncInternal<T>(
                ActorSystem, actorRef, whatToAsk, timeout, options.ExecutionId, SynchronousAskFactory);
        }


        private static ActorSystem GetOrCreatedActorSystem(ActorSystem savedSystem, AskSyncOptions options)
        {
            //todo clear confusion regarding these parameters
            ActorSystem result;
            if (options.ExistingActorSystem != null)
            {
                result = options.ExistingActorSystem;
            }
            else if (options.UseDefaultRemotingActorSystemConfig)
            {
                result = savedSystem ??
                         ActorSystem.Create(SystemName, DefaultRemotingActorSystemConfig(options.DefaultRemotingPort));
            }
            else if (!string.IsNullOrEmpty(options.ActorSystemConfig))
            {
                result = savedSystem ?? ActorSystem.Create(SystemName, options.ActorSystemConfig);
            }
            else
            {
                result = savedSystem ?? ActorSystem.Create(SystemName);
            }
            return result;
        }
    }
}