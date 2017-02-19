using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CIlibProcessor.Common
{
    //TODO: need a way of determining whether a given measure if minimization or maximization.
    //[DebuggerDisplay("Name = {Name}")]
    public class Measurement
    {
        /// <summary>
        /// The iteration statistics.
        /// </summary>
        readonly ConcurrentDictionary<int, IterationStats> _iterationStatistics;

        /// <summary>
        /// Gets the name of this measure.
        /// </summary>
        /// <value>The name of this measure.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the iteration statistics.
        /// </summary>
        /// <value>The iteration statistics.</value>
        public IEnumerable<IterationStats> Stats => _iterationStatistics.Values;

        public IterationStats this [int iteration] => _iterationStatistics.ContainsKey(iteration) ? _iterationStatistics[iteration] : null;

        /// <summary>
        /// Return the iteration statistics of the final iteration.
        /// </summary>
        /// <value>The iteration statistics of the final iteration.</value>
        public IterationStats FinalIteration => _iterationStatistics[MaximumIterations];

        public IEnumerable<int> Iterations => _iterationStatistics.Keys;

        int maxIters = -1;

        public int MaximumIterations => maxIters;

        //public bool Minimization{ get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Measurement"/> class.
        /// </summary>
        /// <param name="name">The measure name.</param>
        public Measurement(string name)
        {
            Name = name;
            _iterationStatistics = new ConcurrentDictionary<int, IterationStats>();
        }

        /// <summary>
        /// Adds the iteration statistics to the list.
        /// </summary>
        /// <param name="iteration">The iteration which we are adding.</param>
        /// <param name="stats">The iteration statistics to add.</param>
        public void AddIterationStatistics(int iteration, IterationStats stats)
        {
            _iterationStatistics.TryAdd(iteration, stats);
            
            if (iteration > maxIters) maxIters = iteration; //check if the new iteration is greater than previous maximum
        }

        public override string ToString()
        {
            return $"[Measurement: Name={Name}]";
        }
    }
}

