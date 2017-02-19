using System.Collections.Generic;
using System.Linq;

namespace CIlibProcessor.Common
{
    public class Algorithm
    {
        public string Name { get; }

        public List<Measurement> Measurements { get; private set; }

        public Algorithm(string name, IEnumerable<Measurement> measurements)
        {
            Name = name;
            Measurements = measurements.ToList();
        }

        public override string ToString()
        {
            return $"[Algorithm: Name={Name}]";
        }
    }
}

