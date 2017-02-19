using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIlibProcessor.Common;
using CIlibProcessor.Statistics;

namespace StaticProcessor
{
	public static class StaticRanker
	{
		public static void Rank(List<Algorithm> algorithms, string outputDirectory, double alpha = 0.05, bool maximize = false)
		{
			//for each measurement
			for (int i = 0; i < algorithms[0].Measurements.Count; i++)
			{
				List<RankOutput> output = new List<RankOutput>();
				foreach (Algorithm alg in algorithms)
				{
					output.Add(new RankOutput(alg.Name,0,0));
				}

				//get final iteration statistics for the current measurement
				IEnumerable<IterationStats> stats = algorithms.Select(x => x.Measurements[i].FinalIteration);

				//perform a Kruskal-Wallis test to assess if there are differences among all the results
				double kwp = KruskalWallisTest.Calculate(stats);
				if (kwp < alpha) //significant difference exists
				{

					//perform a pairwise Mann-Whitney U test with Holm correction to assess individual differences
					double[,] mwp = MannWhitneyUTest.PairwiseCalculate(stats, true);

					for (int j = 0; j < mwp.GetLength(0); j++) //row
					{
						for (int k = 0; k < mwp.GetLength(1); k++) //col
						{
							//there is a significant difference, comapre medians to assign best/worse
							if (!double.IsNaN(mwp[j, k]) && mwp[j, k] < alpha)
							{
								//note: j index starts at second algorithm (i.e., j[0] = alg[1])
								int rowIndex = j + 1;

								int best;
								int worst;
								if (stats.ElementAt(rowIndex).Median > stats.ElementAt(k).Median) //alg 1 is better (assuming max)
								{
									best = rowIndex;
									worst = k;
								}
								else //alg 2 is better (assuming max)
								{
									best = k;
									worst = rowIndex;
								}

								if (maximize == false) //swap best and worse for minimization
								{
									int temp = best;
									best = worst;
									worst = temp;
								}

								output[best].Wins++;
								output[worst].Losses++;
							}
						}
					}
				}


				string ROuputDir = Path.Combine(outputDirectory, "R_output");     //directory to save output from the R script
				if (!Directory.Exists(ROuputDir))         //create the output directory if it doesn't exist
					Directory.CreateDirectory(ROuputDir);	

				//write the difference results to a CSV file
				using (TextWriter writer = new StreamWriter(Path.Combine(ROuputDir, string.Format("{0}_diff-{1}.csv",
				                                                                                  algorithms[0].Measurements[i].Name,
				                                                                                  algorithms[0].Measurements[i].FinalIteration.Iteration))))
				{
					writer.WriteLine("Algorithm,Wins,Losses,Difference,Rank");
					foreach (RankOutput rankOutput in output.OrderBy(x => x.Algorithm))
					{
						int rank = output.Count(x => x.Difference > rankOutput.Difference) + 1; //this is horrible runtime, but correctly ranks the solutions
						writer.WriteLine($"{rankOutput.Algorithm},{rankOutput.Wins:F0},{rankOutput.Losses:F0},{rankOutput.Difference:F0},{rank}");
					}
				}


			}

		}
	}
}
