using Nest;
using System;
using System.Collections.Generic;

namespace ElasticSearchGeolocationExample
{
    class Program
    {
        private static GeoLocation baseGeolocation = GeoLocation.TryCreate(10.330114, 123.898578);
        static void Main(string[] args)
        {
            var client = CreateClient();
            Index(client);

            var searchAscendingResponse = SearchByDistance(client, SortOrder.Ascending);
            Console.WriteLine($"People sorted by distance from ({baseGeolocation}) ASCENDING:");
            foreach (var item in searchAscendingResponse.Documents)
                Console.WriteLine($"{item.Id}: {item.FirstName} {item.LastName} ({item.Location})");
            
            var searchDescendingResponse = SearchByDistance(client, SortOrder.Descending);
            Console.WriteLine($"\n\nPeople sorted by distance from ({baseGeolocation}) DESCENDING:");
            foreach (var item in searchDescendingResponse.Documents)
                Console.WriteLine($"{item.Id}: {item.FirstName} {item.LastName} ({item.Location})");
        }

        private static ElasticClient CreateClient()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                            .DefaultIndex("people");
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
            var people = new List<Person>();
            for (int i = 0; i < 3000; i++)
            {
                people.Add(new Person
                {
                    Id = i,
                    FirstName = Faker.Name.First(),
                    LastName = Faker.Name.Last(),
                    Location = GeolocationHelpers.GenerateRandomGeoLocation(baseGeolocation, 3000000),
                });
            }
             var indexResponse = client.IndexMany(people);
        }

        private static ISearchResponse<Person> SearchByDistance(ElasticClient client, SortOrder sortOrder)
        {
            return client.Search<Person>(s => s
                            .Sort(ss => ss.GeoDistance(g => g
                                  .Field(f => f.Location)
                                  .Order(sortOrder)
                                  .DistanceType(GeoDistanceType.Plane)
                                  .Points(baseGeolocation)))
            );
        }
    }
}
