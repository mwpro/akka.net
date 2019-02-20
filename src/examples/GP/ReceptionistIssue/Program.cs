//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;

namespace ReceptionistIssue
{
    class Program
    {
        // how to reproduce?
        // run first seed node with dotnet .\ReceptionistIssue.dll 8081
        // run second node with dotnet .\ReceptionistIssue.dll
        // run third node with dotnet .\ReceptionistIssue.dll and close the second one while third is starting
        // first and third node should log having MemberRemoved but no nodes removed

        // 

        static void Main(string[] args)
        {
            // create a new actor system (a container for actors)
            int port = 0;
            if (args.Length > 0)
            {
                port = int.Parse(args[0]);
            }
            var system = ActorSystem.Create("MySystem", ConfigurationFactory.ParseString($@"
akka {{
   extensions = [""Akka.Cluster.Tools.Client.ClusterClientReceptionistExtensionProvider, Akka.Cluster.Tools""]
   actor.provider = cluster
    remote {{
        dot-netty.tcp {{
            port = {port}
            hostname = localhost
        }}
    }}
    cluster {{
       #seed-nodes = [""akka.tcp://MySystem@localhost:8081""] # address of seed node
    }}
}}
"));

            Cluster.Get(system).JoinSeedNodesAsync(new[] { Address.Parse("akka.tcp://MySystem@localhost:8081") }).Wait();

            // create actor and get a reference to it.
            // this will be an "ActorRef", which is not a 
            // reference to the actual actor instance
            // but rather a client or proxy to it
            var greeter = system.ActorOf<GreetingActor>("greeter");

            // send a message to the actor
            greeter.Tell(new GreetingActor.Greet("World"));


            var clusterClientReceptionist = ClusterClientReceptionist.Get(system);
            clusterClientReceptionist.RegisterService(greeter);

            // prevent the application from exiting before message is handled
            Console.ReadLine();

            CoordinatedShutdown.Get(system).Run(CoordinatedShutdown.ClrExitReason.Instance).Wait();
        }
    }
}

