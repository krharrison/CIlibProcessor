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
	    private bool _stats;
	    private double _average;
	    private double _standardDeviation;
	    private double _min;
	    private double _max;
	    private double _median;

        //the iteration which these values were recorded
        public int Iteration{ get; }

		/// <summary>
		/// Gets the average.
		/// </summary>
		/// <value>The average.</value>
		public double Average
		{
			get
			{
				if (!_stats) CalculateStats();
				return _average;
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
				if (!_stats) CalculateStats();
				return _standardDeviation;
			}
		}


		public double Min
		{
			get
			{
				if (!_stats) CalculateStats();
				return _min;
			}
		}

		public double Max
		{
			get
			{
				if (!_stats) CalculateStats();
				return _max;
			}
		}

		public double Median
		{
			get
			{
				if (!_stats) CalculateStats();
				return _median;
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

			_stats = false;
        }

	    private void CalculateStats()
		{
			_min = double.MaxValue;
			_max = double.MinValue;
			double sum = 0;
			int count = 0;

			foreach (double val in Values)
			{
				_min = Math.Min(_min, val);
				_max = Math.Max(_max, val);
				sum += val;
				count++;
			}

			_average = sum / count;

			//calculate standard deviation
			double devSum = Values.Sum(x => Math.Pow(x - _average, 2));
			_standardDeviation = Math.Sqrt(devSum / (Values.Count - 1));

			_median = Values.Median();

			_stats = true;
		}

        public override string ToString()
        {
            return $"[IterationStats: Iter={Iteration}, Avg={Average}, Std={StandardDeviation}]";
        }
    }
}

