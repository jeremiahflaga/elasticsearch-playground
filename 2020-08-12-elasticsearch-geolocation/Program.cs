using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ElasticSearchGeolocationExample
{
    class Program
    {
        const double baseLatitude = 10.330114;
        const double baseLongitude = 123.898578;

        static void Main(string[] args)
        {
            var peopleService = new PeopleService();
            peopleService.IndexSampleData(baseLatitude, baseLatitude);

            var people = peopleService.GetAll(baseLatitude, baseLongitude);
            foreach (var person in people)
                Console.WriteLine($"{person.Id}: {person.FirstName} {person.LastName}");
        }
    }
}
