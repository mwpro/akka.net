using Akka.Actor;
using Akka.Cluster.Tools.Client;
using System;
using System.Collections.Immutable;

namespace RingOrderIssue
{
    class Program
    {
        static void Main(string[] args)
        {
            var set = ImmutableSortedSet<Address>.Empty.WithComparer(ClusterReceptionist.RingOrdering.Instance)
                .Union(new Address[0]);
            set = set.Add(Address.Parse("akka.tcp://MySystem@localhost:8081"));
            set = set.Add(Address.Parse("akka.tcp://MySystem@localhost:55597"));
            set = set.Add(Address.Parse("akka.tcp://MySystem@localhost:55622"));
            set = set.Remove(Address.Parse("akka.tcp://MySystem@localhost:55597"));
            Console.WriteLine(string.Join(", ", set)); // akka.tcp://MySystem@localhost:55597 still on the list...


            var a1 = Address.Parse("akka.tcp://MySystem@localhost:8081");
            var a2 = Address.Parse("akka.tcp://MySystem@localhost:55597");
            var a3 = Address.Parse("akka.tcp://MySystem@localhost:55622");

            Console.WriteLine(ClusterReceptionist.RingOrdering.Instance.Compare(a1, a2)); // -1 => a1 < a2
            Console.WriteLine(ClusterReceptionist.RingOrdering.Instance.Compare(a2, a3)); // -1 => a2 < a3
            Console.WriteLine(ClusterReceptionist.RingOrdering.Instance.Compare(a2, a1)); // 1 => a2 > a1 but we already know that a1 < a2?
            Console.WriteLine(ClusterReceptionist.RingOrdering.Instance.Compare(a3, a2)); // -1 => a3 < a2
            Console.ReadLine();

            // JVM
            // https://github.com/akka/akka/blob/master/akka-cluster-tools/src/main/scala/akka/cluster/client/ClusterClient.scala#L894-L899
        }
    }
}
