using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using CIlibProcessor.Common;

namespace CIlibProcessor.Statistics
{
	public static class MannWhitneyUTest
	{
		public static double Calculate(IEnumerable<double> x, IEnumerable<double> y, bool? exact = null, bool correct = true)
		{
			double pVal;

			//TODO: remove infinite/Nan x,y

			double nx = x.Count();
			double ny = y.Count();

		    if (nx < 1) throw new ArgumentException("Not enough finite x oberservations");
		    if (nx < 1) throw new ArgumentException("Not enough finite y oberservations");

			List<double> ranks = Helpers.Rank(x.Concat(y)).Ranks;

			//Console.WriteLine("Ranks: {0}", string.Join(",", ranks));

			if (!exact.HasValue)
			{
				exact = (nx < 50) && (ny < 50);
			}

			//sum the ranks of the x samples and subtract stuff..
			double statistic = ranks.Take((int)nx).Sum() - nx * (nx + 1) / 2;

			bool ties = ranks.Count() != ranks.Distinct().Count(); //TODO: better check for ties?

			if (exact.Value && !ties)
			{
				double p = 0;

				if (statistic > (nx * ny / 2)) p = pwilcox(statistic - 1, nx, ny, 0, 0);
				else p = pwilcox(statistic, nx, ny, 1, 0);

				pVal = Math.Min(2 * p, 1);
			}
			else
			{

				Dictionary<double, int> nties = Helpers.NumTies(ranks);

				double z = statistic - nx * ny / 2;
				double sigma = Math.Sqrt((nx * ny / 12) * ((nx + ny + 1) - nties.Sum(a => Math.Pow(a.Value, 3) - a.Value) / ((nx + ny) * (nx + ny - 1))));

				double correction = 0;
				if (correct)
				{
					correction = Math.Sign(z) * 0.5;
				}

				z = (z - correction) / sigma;

				bool lower = z < 0;
				pVal = 2 * NormalDistribution.pnorm(z, 0, 1, lower, false);
				//pVal = 2 * Math.Min(NormalDistribution.pnorm(z, 0, 1, true, false), NormalDistribution.pnorm(z, 0, 1, false, false));
			}

			return pVal;
		}


	    /// <summary>
	    /// Pairwise tests with (optional) Holm correction.
	    /// </summary>
	    /// <returns>Array of pairwise p-values.</returns>
	    /// <param name="statsEnum">Iteration statistics.</param>
	    /// <param name="correction">Use holm correction.</param>
	    public static double[,] PairwiseCalculate(IEnumerable<IterationStats> statsEnum, bool correction = true)
		{
			IterationStats[] stats = statsEnum.ToArray();
			double[,] results = new double[stats.Length - 1, stats.Length - 1];

			//TODO: parallel for?
			for (int i = 1; i < stats.Length; i++)
			//Parallel.For(1, stats.Length, i =>
			{
				for (int j = 0; j < stats.Length - 1; j++)
				{
					if (i > j) results[i - 1, j] = Calculate(stats[i].Values, stats[j].Values, false);
					else results[i - 1, j] = double.NaN; //empty entry in
				}
			}//);

			if (correction)
			{
				List<double> p = new List<double>();
				for (int i = 0; i < results.GetLength(1); i++) //columns
				{
					for (int j = 0; j < results.GetLength(0); j++)//rows
					{
						if (!double.IsNaN(results[j, i])) p.Add(results[j, i]);
					}
				}

				//ordered p-values
				var orderedP = Helpers.Rank(p);
				List<int> o = new List<int>(p.Count);
				foreach (int op in orderedP.Indexes)
				{
					o.Add(op + 1);
				}

				List<int> ro = Helpers.Order(o);
				List<double> stuff = new List<double>(p.Count);
				int index = 0;

				foreach (double val in p)
				{
					stuff.Add((p.Count - (index + 1) + 1) * p[o.ElementAt(index) - 1]);
					index++;
				}

				List<double> cumulativeMax = Helpers.CumulativeMax(stuff);

				List<double> pMin = new List<double>(p.Count);

				foreach (double c in cumulativeMax)
				{
					pMin.Add(Math.Min(1, c));
				}

				//order elements by values of r
				index = 0; //reuse the index variable
				for (int i = 0; i < results.GetLength(1); i++) //columns
				{
					for (int j = 0; j < results.GetLength(0); j++)//rows
					{
						if (!double.IsNaN(results[j, i])) results[j, i] = pMin[ro.ElementAt(index++) - 1];
					}
				}

			}

			return results;
		}

		//[DllImport("libRmath.so", CallingConvention = CallingConvention.Cdecl)]
		[DllImport("Rmath", CallingConvention = CallingConvention.Cdecl)]
		static extern double pwilcox(double q, double m, double n, int lower_tail, int log_p);
	}


}
