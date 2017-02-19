using System;
using System.Collections.Generic;
using CIlibProcessor.Common;
using System.Linq;
using System.Runtime.InteropServices;

namespace CIlibProcessor.Statistics
{
	public static class KruskalWallisTest
	{
		public static double Calculate(IEnumerable<IterationStats> stats)
		{
			//TODO: verification stuffs

			//get list of all values
			IEnumerable<double> values = stats.SelectMany(x => x.Values);

			//rank the values
			List<double> ranks = Helpers.Rank(values).Ranks;

			//number of groups
			int k = stats.Count();

			if (k < 1) throw new ArgumentException("All observations are in the same group");

			int n = values.Count();

			Dictionary<double, int> nties = Helpers.NumTies(values);

			double statistic = 0;
			int index = 0;
			foreach (IterationStats group in stats)
			{
				int groupSize = group.Values.Count;
				double val = ranks.Skip(index).Take(groupSize).Sum();
				statistic += Math.Pow(val, 2) / groupSize;

				index += groupSize;
			}

			statistic = ((12 * statistic / (n * (n + 1)) - 3 * (n + 1)) / (1 - nties.Sum(a => Math.Pow(a.Value, 3) - a.Value) / (Math.Pow(n, 3) - n)));

			double pVal = pchisq(statistic, k - 1, 0, 0);

			return pVal;
		}

		[DllImport("Rmath", CallingConvention = CallingConvention.Cdecl)]
		static extern double pchisq(double x, double df, int lower_tail, int log_p);
	}
}
