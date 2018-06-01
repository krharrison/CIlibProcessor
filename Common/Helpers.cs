using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CIlibProcessor.Common
{
	public static class Helpers
	{
		public static double Median(this IEnumerable<double> list)
		{
			double[] temp = list.ToArray();
			int count = temp.Length;
			if (count == 0) throw new InvalidOperationException("Empty collection");
			if (count == 1) return temp[0];

			Array.Sort(temp);

			if (count % 2 != 0) return temp[count / 2];
			double a = temp[count / 2 - 1];
			double b = temp[count / 2];
			return (a + b) / 2;
			// otherwise, count is odd, return the middle element
		}

		/// <summary>
		/// No idea what this is, copied directly from the C code.
		/// </summary>
		private static readonly int[] Sincs = {
			1073790977, 268460033, 67121153, 16783361, 4197377, 1050113,
			262913, 65921, 16577, 4193, 1073, 281, 77, 23, 8, 1, 0
		};

		/// <summary>
		/// Return the relative rank of each element. Ties are returned as the average.
		/// </summary>
		/// <param name="values">Values.</param>
		public static OrderResult Rank<T>(IEnumerable<T> values) where T : IComparable<T>
		{
			T[] x = values.ToArray();
		    int n = x.Length;
			int j, t, hi = n - 1;

		    int[] ranks = new int[n];
			for (int a = 0; a < ranks.Length; a++)
			{
				ranks[a] = a;
			}

			//TODO: handle na?

			for (t = 0; Sincs[t] > hi + 1; t++) { } //yes, this is meant to be an empty loop
														 //if (n < 2) return;

			for (int h = Sincs[t]; t < 16; h = Sincs[++t])
			{
				//R_CheckUserInterrupt();	 
				for (int i = h; i <= hi; i++)
				{
					var itmp = ranks[i];
					j = i;
					while (j >= h && Greater(x, ranks[j - h], itmp))
					{
						ranks[j] = ranks[j - h];
					    j -= h;
					}
					ranks[j] = itmp;
				}
			}

		    //here is the tie handling
			double[] dRanks = new double[n];
			for (int i = 0; i < n; i = j + 1)
			{
				j = i;
				while (j < n - 1 && x[ranks[j]].Equals(x[ranks[j + 1]])) j++; //TODO: is equals ok here?

				double avgRank = (i + j + 2) / 2.0;
				for (int k = i; k <= j; k++)
				{
					dRanks[ranks[k]] = avgRank;
				}
			}

			return new OrderResult(dRanks.ToList(), ranks.ToList());
		}

		public static bool Greater<T>(T[] x, int a, int b) where T : IComparable<T>
		{
			return x[a].CompareTo(x[b]) > 0; /*|| (x[a].Equals(x[b]) && a > b)*/ //TODO: does order matter here?
		}


		/// <summary>
		/// Order the list of values, maintining the original index
		/// </summary>
		/// <param name="values">Values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<int> Order<T>(IEnumerable<T> values) where T : IComparable<T>
		{
			OrderResult result = Rank(values);

			List<int> order = new List<int>(result.Indexes.Count);
			foreach (int index in result.Indexes)
			{
				order.Add(index + 1);
			}

			return order;
			//	return result.Indexes.Select(x => x + 1); //TODO: this needs a fixin..
			//return values.Select((n, i) => new NumberRank<T>(n, i, 0)).OrderBy(n => n.Number);
		}

		public static Dictionary<double, int> NumTies(IEnumerable<double> values)
		{
			Dictionary<double, int> nties = new Dictionary<double, int>();
			foreach (double d in values)
			{
				if (nties.ContainsKey(d)) nties[d]++;
				else nties.Add(d, 1);
			}

			return nties;
		}

		/// <summary>
		/// Calculate the cumulative max, wuch that result[i] = max(values[0], ..., values[i])
		/// </summary>
		/// <returns>The max.</returns>
		/// <param name="values">Values.</param>
		public static List<double> CumulativeMax(IEnumerable<double> values)
		{
			double max = double.MinValue;
			List<double> result = new List<double>();

			foreach (double val in values)
			{
				if (double.IsNaN(val)) continue; //ignore NaN
				max = Math.Max(max, val);
				result.Add(max);
			}

			return result;
		}

		//TODO: finish
		public static void WriteRankOtputFile(string filename, IEnumerable<RankOutput> outputs)
		{
			outputs = outputs.OrderBy(x => x.Algorithm); //sort them things
			IEnumerable<double> ranks = Rank(outputs.Select(x => -x.Difference)).Ranks;
			using (TextWriter writer = new StreamWriter(filename))
			{
				writer.WriteLine("Algorithm,Wins,Losses,Difference,Rank");
				using (var sequenceEnum = ranks.GetEnumerator())
				{
					foreach (RankOutput rankOutput in outputs)
					{
						double rank = sequenceEnum.Current;
						writer.WriteLine($"{rankOutput.Algorithm},{rankOutput.Wins:F0},{rankOutput.Losses:F0},{rankOutput.Difference:F0},{rank}");
					}
				}
			}
		}


	}
}

