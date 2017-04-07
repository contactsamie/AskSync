# AskSync

![akkaasksync](https://cloud.githubusercontent.com/assets/2102748/24671142/8d40bca8-193e-11e7-800a-4073063b286d.png)

This is a very simple Akka.NET extension that enables you to do an Ask without async await, you avoid all async/await and more issues with both local and remoting scenarios


CAUTION :  Doing an ask is considered an anti-pattern. Using this results in a slightly slower communication than even the regular Ask, as this is just a wrapper (To understand, why Ask is usually a bad pattern for inter-actors communication see http://bartoszsypytkowski.com/dont-ask-tell-2/). Some times though, you want to communicate with actors from 'outside'. And some times you want to do so without 'awaiting' on it or dealing with contexts


[![NuGet version](https://img.shields.io/nuget/v/AkkaAskSync.svg?style=flat-square)](https://www.nuget.org/packages/AkkaAskSync)




To install AkkaAskSync, run the following command in the Package Manager Console

    Install-Package AkkaAskSync

Intead of

    var result = await MyActor.Ask<ActorIdentity>(new Identify(null));

You do 

    var result = MyActor.AskSync<ActorIdentity>(new Identify(null));

For remoting, provide your existing actor system , do 

    var result = MyActor.AskSync<ActorIdentity>(new Identify(null),MyActorSystem);

NOTE : An AskSync to a remote actor may not work unless you provide your existing actorsystem with remoting enabled. For more info in configuing/enabling remoting see  http://getakka.net/docs/Remoting#using-remoting  For more information about the traditional Ask see http://getakka.net/docs/Working%20with%20actors#ask-send-and-receive-future
