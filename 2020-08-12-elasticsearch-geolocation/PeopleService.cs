using Nest;
using System;
using System.Collections.Generic;

namespace ElasticSearchGeolocationExample
{
    class PeopleService
    {
        private static Uri ElasticSearchUri = new Uri("http://localhost:9200");

        public void IndexSampleData(double baseLatitude, double baseLongitude)
        {
            GeoLocation baseGeolocation = GeoLocation.TryCreate(baseLatitude, baseLongitude);
            var client = CreateClient();
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

        public IEnumerable<PersonViewModel> GetAll(int page, int numberPerPage, double baseLatitude, double baseLongitude)
        {
            GeoLocation baseGeolocation = GeoLocation.TryCreate(baseLatitude, baseLongitude);
            var client = CreateClient();
            var response = client.Search<Person>(s => s
                .From(page * numberPerPage)
                .Size(numberPerPage)
                .Source(sf => sf
                    .Includes(i => i
                        .Fields(
                            f => f.Id,
                            f => f.FirstName,
                            f => f.LastName
                        )
                    )
                )
                .Sort(ss => ss.GeoDistance(g => g
                        .Field(f => f.Location)
                        .Order(SortOrder.Ascending)
                        .DistanceType(GeoDistanceType.Plane)
                        .Points(baseGeolocation)))
            );

            foreach (var person in response.Documents)
                yield return PersonViewModel.From(person);
        }

        private static ElasticClient CreateClient()
        {
            var settings = new ConnectionSettings(ElasticSearchUri)
                            .DefaultIndex("people");
            var client = new ElasticClient(settings);
            var indexExistsResponse = client.Indices.Exists("people");
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = client.Indices.Create("people", c => c
                    .Map<Person>(p => p
                        //automatically create the mapping from the type
                        .AutoMap()
                        .Properties(pr => pr.GeoPoint(g => g.Name(n => n.Location)))
                    )
                );
            }
            return client;
        }
    }
}
