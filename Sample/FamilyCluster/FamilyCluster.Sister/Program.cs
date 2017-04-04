using System;
using System.Configuration;
using Akka.Actor;
using AskSync.AkkaAskSyncLib;
using FamilyCluster.Common;

namespace FamilyCluster.Sister
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting SisterSystem ...");
            using (var system = ActorSystem.Create("SisterSystem"))
            {
                var client = ConfigurationManager.AppSettings["client"];
                var sisterEchoActor = system.ActorOf(Props.Create(() => new EchoActor()), "SisterEchoActor");

                while (true)
                {
                    var message = Console.ReadLine();
                 var result=   system.ActorSelection(client).AskSync(new Hello("From SisterSystem to client at " + client + message),null, new AskSyncOptions() {ActorSystemConfig = $@"
                akka {{ 
                    remote {{
                        helios.tcp {{                            
                            transport-protocol = tcp
                            port = 0
                            hostname = localhost
                        }}
                    }}
                    stdout-loglevel = DEBUG
                    loglevel = DEBUG
                    debug {{  
                            receive = on 
                            unhandled = on
                    }}
                }}" });
                    Console.WriteLine(result);
                }
            }
        }
    }
}