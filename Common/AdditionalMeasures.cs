using System;
using System.Linq;
using System.Collections.Generic;

namespace CIlibProcessor.Common
{
    public static class AdditionalMeasures
    {
        public static void AddConsistency(List<Algorithm> algorithms)
		{
            foreach(Algorithm algorithm in algorithms)
            {
                Measurement fitness = algorithm.Measurements.Find(x => x.Name == "Fitness");
                algorithm.Measurements.Insert(1, Consistency(fitness));
            }

        }

        /// <summary>
        /// The Consistency is the sum of the squared differences between the fitness and the average fitnesses.
        /// </summary>
        /// <param name="fitness">Fitness.</param>
        public static Measurement Consistency(Measurement fitness){
            double avg = fitness.FinalIteration.Average;

            Measurement result = new Measurement("Consistency");
            IterationStats stats = new IterationStats(1, fitness.FinalIteration.Values.Select(x => Math.Pow(x - avg, 2)));
            result.AddIterationStatistics(1, stats);
            return result;
        }

        /// <summary>
        /// The Success Rate is the percentage of the independent runs which reached specified accuracy levels. 
        /// A total of numLevels accuracy levels have been considered, with the accuracy levels starting at the best
        /// accuracy obtained by all algorithms, logarithmically increasing towards the worst accuracy.
        /// </summary>
        /// <param name="algorithms">Algorithms.</param>
        /// <param name="numLevels">The number of accuracy levels.</param>
        public static void AddSuccessRate(List<Algorithm> algorithms, int numLevels)
		{
            double bestFit = double.MaxValue;
            double worstFit = double.MinValue;

            var fitnesses = algorithms.Select(x => x.Measurements.Find(y => y.Name == "Fitness"));

            foreach(Measurement measure in fitnesses)
            {
                worstFit = Math.Max(worstFit, measure.FinalIteration.Max);
                bestFit = Math.Min(bestFit, measure.FinalIteration.Min);
            }
                
            List<double> accuracyLevels = LogarithmicallyIncreasingScale(bestFit, worstFit, numLevels);

            foreach(Algorithm algorithm in algorithms)
            {
                Measurement measure = algorithm.Measurements.Find(x => x.Name == "Fitness");
                double runs = measure.FinalIteration.Values.Count;
                List<double> successRate = new List<double>(numLevels);
                //TODO: this can be improved majorly!
                foreach(double level in accuracyLevels)
                {
                    //TODO: (1 - success) as R expects minimization and success rate is maximization
                    successRate.Add(1 - measure.FinalIteration.Values.Count(x => x < level) / runs);
                }

                Measurement result = new Measurement("SuccessRate");
                IterationStats stats = new IterationStats(1, successRate);
                result.AddIterationStatistics(1, stats);
                algorithm.Measurements.Insert(1, result);
            }
        }
                
        static List<double> LogarithmicallyIncreasingScale(double min, double max, int length)
		{
            max = Math.Log(max);
            min = Math.Log(min);
            double range = max - min;

            List<double> scale = new List<double>(length);
            for(int i = 1; i <= length; i++)
            {
                double value = (range * i / length) + min;
                scale.Add(Math.Exp(value));
            }

            return scale;
        }
    }
}

