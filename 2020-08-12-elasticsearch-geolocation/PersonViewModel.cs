using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearchGeolocationExample
{
    class PersonViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public static PersonViewModel From(Person person)
        {
            return new PersonViewModel
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
            };
        }
    }
}
