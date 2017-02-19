using System;
using NDesk.Options;
using CIlibProcessor.Common;
using System.Collections.Generic;
using System.IO;

namespace StaticProcessor
{
    class StaticProcesssor
    {
        static string directory;
		static int iteration = -1;

        public static void Main(string[] args)
        {
            options.Parse(args);

            if (string.IsNullOrEmpty(directory))
            {
                Console.WriteLine("No directory specified, exiting.");
                Environment.Exit(1);
            }

            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Invalid directory, exiting.");
                Environment.Exit(1);
            }

			CIlibParser parser;

			if (iteration < 0) //parse only final output
				parser = new FinalOutputParser();
			else //parse specific iteration
				parser = new SingleIterationParser(iteration);
			
			List<Algorithm> algorithms = parser.ParseDirectory(directory);

			foreach (Algorithm alg in algorithms)
			{
				alg.Measurements.RemoveAll(x => x.Name != "Fitness");
			}

            //AdditionalMeasures.AddConsistency(algorithms);
            //AdditionalMeasures.AddSuccessRate(algorithms, 1000);

			StaticRanker.Rank(algorithms, directory);
        }

		/// <summary>
		/// Specify the command line arguments.
		/// </summary>
		static readonly OptionSet options = new OptionSet
		{
			{ "d|directory=", "The directory to parse. ", v => directory = v },
			{ "i|iteration=", "The iteration to summarize", v => iteration = int.Parse(v) }
		};
	}
}
