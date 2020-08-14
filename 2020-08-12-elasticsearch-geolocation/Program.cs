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

            int page = 0;
            int pageSize = 50;
            while (true)
            {
                var people = peopleService.GetAll(page, pageSize, baseLatitude, baseLongitude);
                if (people.Count() <= 0)
                    break;

                foreach (var person in people) 
                    Console.WriteLine($"{person.Id}: {person.FirstName} {person.LastName}");

                page++;

                Console.WriteLine("\nPRESS ENTER TO DISPLAY NEXT PAGE\n");
                Console.ReadLine();
            }
        }
    }
}
