using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Actors;
using AskSync.AkkaAskSyncLib.Services;

namespace AskSync.AkkaAskSyncLib
{
    public static class AskExtensions
    {
        public static T AskSync<T>(
           this ICanTell actorRef
           , object whatToAsk
           , TimeSpan? timeout = null
           , AskSyncOptions options = null)
        {
            return AskSyncImplementation.AskSyncImpl<T>(
                actorRef
                , whatToAsk
                , timeout
                , options
                );
        }

        public static object AskSync(
          this ICanTell actorRef
          , object whatToAsk
          , ActorSystem actorSystem)
        {
            if (actorSystem == null)
                throw new ArgumentNullException(nameof(actorSystem));
            return actorRef.AskSync<object>(
                whatToAsk
                , null
                , new AskSyncOptions()
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
            return actorRef.AskSync<object>(
                whatToAsk
                , timeout
                , new AskSyncOptions()
                {
                    ExistingActorSystem = actorSystem
                });
        }

        public static T AskSync<T>(
           this ICanTell actorRef
           , object whatToAsk
           , ActorSystem actorSystem)
        {
            return actorRef.AskSync<T>(
                whatToAsk
                , null
                , new AskSyncOptions()
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
            return actorRef.AskSync<T>(
                whatToAsk
                , timeout
                , new AskSyncOptions()
                {
                    ExistingActorSystem = actorSystem
                });
        }

        public static object AskSync(
           this ICanTell actorRef
           , object whatToAsk
           , AskSyncOptions options = null)
        {
            return actorRef.AskSync<object>(
                whatToAsk
                , null
                , options
                );
        }

        public static object AskSync(
            this ICanTell actorRef
            , object whatToAsk
            , TimeSpan? timeout
            , AskSyncOptions options = null)
        {
            return actorRef.AskSync<object>(
                whatToAsk
                , timeout
                , options
                );
        }
   
    }
}