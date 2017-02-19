using System;
using System.Collections.Generic;
using System.Linq;

namespace CIlibProcessor.Common
{
    /// <summary>
    /// Represents the statistics for a particular iteration
    /// </summary>
    //[DebuggerDisplay("Iter = {Iteration}, Avg = {Average}")]
    public class IterationStats
    {
		bool stats;
		double average;
		double standardDeviation;
		double min;
		double max;
		double median;

        //the iteration which these values were recorded
        public int Iteration{ get; private set; }

		/// <summary>
		/// Gets the average.
		/// </summary>
		/// <value>The average.</value>
		public double Average
		{
			get
			{
				if (!stats) CalculateStats();
				return average;
			}
		}

		/// <summary>
		/// Gets the standard deviation.
		/// </summary>
		/// <value>The standard deviation.</value>
		public double StandardDeviation
		{
			get
			{
				if (!stats) CalculateStats();
				return standardDeviation;
			}
		}


		public double Min
		{
			get
			{
				if (!stats) CalculateStats();
				return min;
			}
		}

		public double Max
		{
			get
			{
				if (!stats) CalculateStats();
				return max;
			}
		}

		public double Median
		{
			get
			{
				if (!stats) CalculateStats();
				return median;
			}
		}

		/// <summary>
		/// Gets the values.
		/// </summary>
		/// <value>The values.</value>
		public List<double> Values { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationStats"/> class.
        /// This will calculate the average and standard deviation.
        /// </summary>
        /// <param name="iteration">Iteration.</param>
        /// <param name="values">Values.</param>
        public IterationStats(int iteration, IEnumerable<double> values)
        {
            Iteration = iteration;
            Values = values.ToList();

			stats = false;
        }

		void CalculateStats()
		{
			min = double.MaxValue;
			max = double.MinValue;
			double sum = 0;
			int count = 0;

			foreach (double val in Values)
			{
				min = Math.Min(min, val);
				max = Math.Max(max, val);
				sum += val;
				count++;
			}

			average = sum / count;

			//calculate standard deviation
			double devSum = Values.Sum(x => Math.Pow(x - average, 2));
			standardDeviation = Math.Sqrt(devSum / (Values.Count - 1));

			median = Values.Median();

			stats = true;
		}

        public override string ToString()
        {
            return $"[IterationStats: Iter={Iteration}, Avg={Average}, Std={StandardDeviation}]";
        }
    }
}

