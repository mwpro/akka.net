//-----------------------------------------------------------------------
// <copyright file="GreetingActor.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using MassTransit;

namespace HelloAkka
{
    /// <summary>
    /// The actor class
    /// </summary>
    public class GreetingActor : ReceiveActor
    {
        private IBusControl _bus;

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
            _bus = Bus.Factory.CreateUsingInMemory(cfg =>
            {
                cfg.ReceiveEndpoint("queue_name", ep =>
                {
                    //configure the endpoint
                });
            });

            // Tell the actor to respond to the Greet message
            ReceiveAsync<Greet>(GreetHandler);
        }

        private async Task GreetHandler(Greet greet)
        {
            Console.WriteLine("Hello {0}", greet.Who);

            await AsyncMethod()
                    .ConfigureAwait(false)
                ;

            Console.WriteLine("{0} completed simple async call", Self.Path);

            await _bus.Publish(greet).ConfigureAwait(false);

            Console.WriteLine("{0} completed MassTransit async call", Self.Path);
        }

        private Task TaskMethod()
        {
            Thread.Sleep(500);
            return Task.CompletedTask;
        }

        private async Task AsyncMethod()
        {
            Thread.Sleep(500);
        }
    }
}

