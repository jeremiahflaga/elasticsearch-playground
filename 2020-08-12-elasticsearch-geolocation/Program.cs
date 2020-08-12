using Nest;
using System;

namespace ElasticSearchGeolocationExample
{
    class Program
    {
        private static GeoLocation baseGeolocation = GeoLocation.TryCreate(10.330114, 123.898578);
        static void Main(string[] args)
        {
            var client = CreateClient();
            Index(client);
            var searchResponse = Search(client);

            Console.WriteLine($"People sorted by distance from ({baseGeolocation}):");
            foreach (var item in searchResponse.Documents)
                Console.WriteLine($"{item.Id}: {item.FirstName} {item.LastName} ({item.Location})");
        }

        private static ElasticClient CreateClient()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                            .DefaultIndex("people");

            // enable trace on your ConnectionSettings object: https://stackoverflow.com/a/28940234/1451757
            settings.EnableDebugMode();
            // log all requests made to Elastic: https://stackoverflow.com/a/50023531/1451757
            settings.OnRequestCompleted(call =>
            {
                if (call.RequestBodyInBytes != null)
                    System.Diagnostics.Debug.Write(System.Text.Encoding.UTF8.GetString(call.RequestBodyInBytes));
            });

            var client = new ElasticClient(settings);
            var createIndexResponse = client.Indices.Create("people", c => c
                .Map<Person>(p => p
                    //automatically create the mapping from the type
                    .AutoMap()
                    .Properties(pr => pr.GeoPoint(g => g.Name(n => n.Location)))
                )
            );

            return client;
        }

        private static void Index(ElasticClient client)
        {
            var person = new Person
            {
                Id = 1,
                FirstName = "Jboy",
                LastName = "Flaga",
                Location = GeoLocation.TryCreate(41.12, -71.34),
            };
            var indexResponse = client.IndexDocument(person);

            var person2 = new Person
            {
                Id = 2,
                FirstName = "Jboy 222",
                LastName = "Flaga",
                Location = GeoLocation.TryCreate(10.317985, 123.891128),
            };
            var indexResponse2 = client.IndexDocument(person2);

            var person3 = new Person
            {
                Id = 3,
                FirstName = "Jboy 333",
                LastName = "Flaga",
                Location = GeoLocation.TryCreate(7.051762, 125.590022),
            };
            var indexResponse3 = client.IndexDocument(person3);
        }

        private static ISearchResponse<Person> Search(ElasticClient client)
        {
            return client.Search<Person>(s => s
                            .From(0)
                            .Size(10)
                            .Query(q => q
                                 .Match(m => m
                                    .Field(f => f.FirstName)
                                    .Query("Jboy")
                                 )
                            )
                            .Sort(ss => ss.GeoDistance(g => g
                                  .Field(f => f.Location)
                                  .Order(SortOrder.Ascending)
                                  .DistanceType(GeoDistanceType.Plane)
                                  .Points(baseGeolocation)))
            );
        }
    }
}
