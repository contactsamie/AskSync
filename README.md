# AskSync


[![NuGet version](https://img.shields.io/nuget/v/AkkaAskSync.svg?style=flat-square)](https://www.nuget.org/packages/AkkaAskSync)

This is a very simple Akka.NET extension that enables you to do an Ask without async await, you avoid all async/await and more issues with both local and remoting scenarios

Intead of

    var result = await MyActor.Ask<ActorIdentity>(new Identify(null));

You do 

    var result = MyActor.AskSync<ActorIdentity>(new Identify(null));

For remoting, provide you existing actor system , do 

    var result = MyActor.AskSync<ActorIdentity>(new Identify(null),MyActorSystem);

NOTE : An AskSync to a remote actor may not work unless you provide your existing actorsystem with remothing enabled. For more info in configuing/enabling remoting see  http://getakka.net/docs/Remoting#using-remoting

For more information about the traditional Ask see http://getakka.net/docs/Working%20with%20actors#ask-send-and-receive-future
