using System.Collections.Generic;

namespace Nancy.Hal.Tests
{
    public class PetOwner
    {
        public string Name { get; set; }
        public bool Happy { get; set; }

        public IList<Animal> Pets { get; set; }
        public Animal LiveStock { get; set; }
    }

    public class Animal
    {
        public string Type { get; set; }
    }
}