//-----------------------------------------------------------------------
// <copyright file="GreetingActor.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace ReceptionistIssue
{
    /// <summary>
    /// The actor class
    /// </summary>
    public class GreetingActor : ReceiveActor
    {
        public class Greet
        {
            public string Who { get; private set; }

            public Greet(string who)
            {
                Who = who;
            }
        }

        public GreetingActor()
        {
            // Tell the actor to respond to the Greet message
            ReceiveAsync<Greet>(GreetHandler);
        }

        private async Task GreetHandler(Greet greet)
        {
            Console.WriteLine("Hello {0}", greet.Who);
        }
    }
}

