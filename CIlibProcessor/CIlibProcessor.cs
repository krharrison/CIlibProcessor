using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIlibProcessor.Common;
using CIlibProcessor.Common.Parser;
using NDesk.Options;

namespace CIlibProcessor
{
    /// <summary>
    /// This program reads a directory of CIlib output files and creates a CSV for each measure.
    /// The CSV file contains the average measure value for each iteration and each algorithm
    /// </summary>
    internal class Program
    {
        private static string _directory;

        private static void Main(string[] args)
        {
            Options.Parse(args);

            //check that the file exists before attempting to parse
            if (!Directory.Exists(_directory))
            {
                Console.WriteLine("Directory \"{0}\" does not exist", _directory);
                return;
            }

            //List<Algorithm> algorithms = CIlibParserOld.ParseDirectory(directory);
            CIlibParser parser = new FullParser();
            List<Algorithm> algorithms = parser.ParseDirectory(_directory);

            string outputPath = Path.Combine(_directory, "averages");

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            foreach (string measure in algorithms[0].Measurements.Select(x => x.Name))
            {
                string fileName = Path.Combine(outputPath, $"{measure}.csv");
                using (TextWriter writer = new StreamWriter(fileName))
                {
                    writer.WriteLine("Iteration,{0}", string.Join(",", algorithms.Select(x => x.Name)));

                    //TODO: sort the iterations?
                    foreach (int iteration in algorithms[0].Measurements[0].Iterations.OrderBy(i => i))
                    {
                        string values = string.Join(",",
                            algorithms.Select(x => x.Measurements.Find(m => m.Name == measure)[iteration].Average));
                        writer.WriteLine($"{iteration}, {values}");
                    }
                }
            }
        }

        /// <summary>
        /// Specify the command line arguments.
        /// </summary>
        private static readonly OptionSet Options = new OptionSet
        {
            {"d|directory=", "The directory to process.", v => _directory = v}
        };
    }
}