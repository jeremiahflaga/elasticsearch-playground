using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // using scan & scroll: https://stackoverflow.com/a/27960442/1451757
        // https://discuss.elastic.co/t/elasticsearch-search-query-to-retrieve-all-records-nest/52562/6
        // https://www.elastic.co/guide/en/elasticsearch/client/net-api/1.x/scroll.html
        // https://stackoverflow.com/questions/37780803/elasticsearch-search-query-to-retrieve-all-records-nest
        public IEnumerable<PersonViewModel> GetAll(double baseLatitude, double baseLongitude)
        {
            GeoLocation baseGeolocation = GeoLocation.TryCreate(baseLatitude, baseLongitude);
            var client = CreateClient();
            List<string> indexedList = new List<string>();

            var scanResults = client.Search<Person>(s => s
                .From(0)
                .Size(2000)
                .Source(sf => sf
                    .Includes(i => i
                        .Fields(
                            f => f.Id,
                            f => f.FirstName,
                            f => f.LastName
                        )
                    )
                )
                .Scroll("10m")
                .Sort(ss => ss.GeoDistance(g => g
                        .Field(f => f.Location)
                        .Order(SortOrder.Ascending)
                        .DistanceType(GeoDistanceType.Plane)
                        .Points(baseGeolocation)))
            );

            var results = client.Scroll<Person>("10m", scanResults.ScrollId);
            while (results.Documents.Any())
            {
                foreach (var person in results.Documents)
                    yield return PersonViewModel.From(person);

                results = client.Scroll<Person>("10m", results.ScrollId);
            }
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
