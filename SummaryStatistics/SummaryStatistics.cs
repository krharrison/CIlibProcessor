using System;
using NDesk.Options;
using CIlibProcessor.Common;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SummaryStatistics
{
	class Program
	{
		static string directory;
		static double? minimum;
		static bool verbose;
		static int iteration = -1;

		static void Main(string[] args)
		{
			options.Parse(args);

			//check that the file exists before attempting to parse
			if (!Directory.Exists(directory))
			{
				Console.WriteLine($"Directory \"{directory}\" does not exist");
				return;
			}

			CIlibParser parser;

			if (iteration < 0) //parse only final output
				parser = new FinalOutputParser();
			else //parse entire file
				parser = new SingleIterationParser(iteration);

			List<Algorithm> algorithms = parser.ParseDirectory(directory);
			string outputPath = Path.Combine(directory, "summaries");

			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);

			foreach (string measure in algorithms[0].Measurements.Select(x => x.Name))
			{
				string fileName = Path.Combine(outputPath, $"{measure}.csv");
				using (TextWriter writer = new StreamWriter(fileName))
				{
					//writer.WriteLine("Algorithm,Mean,Stdandard Deviation,Min,Max");
					if (verbose)
					{
						Console.WriteLine(measure);
						Console.WriteLine("    Alg    |    Min    |   Median  |   Mean    |  Std.Dev  | Max");
					}
					foreach (Algorithm alg in algorithms)
					{
						IterationStats stats = alg.Measurements.Find(m => m.Name == measure).FinalIteration; //get the stats for the associated measure																																														//writer.WriteLine("{0},{1},{2},{3},{4}", alg.Name, stats.Average, stats.StandardDeviation, stats.Min, stats.Max);
						writer.WriteLine("{0} & {1:e2} & {2:e2} & {3:e2} & {4:e2} & {5:e2} \\\\ \\hline", alg.Name, checkMin(stats.Min), checkMin(stats.Median), checkMin(stats.Average), checkMin(stats.StandardDeviation), checkMin(stats.Max));
						if (verbose) Console.WriteLine("{0,10} | {1:0.000} | {2:0.000} | {3:0.000} | {4:0.000} | {5:0.000}", alg.Name, checkMin(stats.Min), checkMin(stats.Median), checkMin(stats.Average), checkMin(stats.StandardDeviation), checkMin(stats.Max));
					}
					if (verbose) Console.WriteLine();
				}
			}
		}

		/// <summary>
		/// Return 0 if the specified value is below the minimum.
		/// </summary>
		/// <returns>The minimum.</returns>
		/// <param name="value">Value.</param>
		static double checkMin(double value)
		{
			if (!minimum.HasValue)
				return value;

			return value < minimum.Value ? 0 : value;
		}

		/// <summary>
		/// Specify the command line arguments.
		/// </summary>
		static OptionSet options = new OptionSet
		{
			{ "d|directory=", "The directory to process.", v => directory = v },
			{ "m|min=","The minimal value. Values below the minimum are considered 0.", v => minimum = double.Parse(v) },
			{ "v|verbose", "Show console output.", v => verbose = true },
			{ "i|iteration=", "The iteration to summarize", v => iteration = int.Parse(v) }
		};
	}
}
