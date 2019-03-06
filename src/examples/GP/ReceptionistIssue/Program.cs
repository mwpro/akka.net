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
        // 1. run first seed node with dotnet .\ReceptionistIssue.dll 8081
        // 2. run second node with dotnet .\ReceptionistIssue.dll 55597
        // 3. run third node with dotnet .\ReceptionistIssue.dll 55622
        // 4. close the second by typing any key, first and third node should log receiving MemberRemoved but no nodes removed from _nodes collection
        // 5. starting second node again results in "An item with the same key has already been added. Key: [1661871636, akka.tcp://MySystem@localhost:55597]"
        //    exception on first and third node

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

