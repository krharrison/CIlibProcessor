using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIlibProcessor.Common;

namespace SummaryStatistics
{
    public class Summarizer
    {
        private double? _minimum;

        public Summarizer(double? minimum = null)
        {
            _minimum = minimum;
        }

        public void Summarize(List<Algorithm> algorithms, string outputPath, bool verbose)
        {
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
                        writer.WriteLine("{0} & {1:e2} & {2:e2} & {3:e2} & {4:e2} & {5:e2} \\\\ \\hline", alg.Name, CheckMin(stats.Min), CheckMin(stats.Median), CheckMin(stats.Average), CheckMin(stats.StandardDeviation), CheckMin(stats.Max));
                        if (verbose) Console.WriteLine("{0,10} | {1:0.000} | {2:0.000} | {3:0.000} | {4:0.000} | {5:0.000}", alg.Name, CheckMin(stats.Min), CheckMin(stats.Median), CheckMin(stats.Average), CheckMin(stats.StandardDeviation), CheckMin(stats.Max));
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
        private double CheckMin(double value)
        {
            if (!_minimum.HasValue)
                return value;

            return value < _minimum.Value ? 0 : value;
        }
    }
}