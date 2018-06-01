using System;
using NDesk.Options;
using CIlibProcessor.Common;
using System.Collections.Generic;
using System.IO;
using CIlibProcessor.Common.Parser;

namespace StaticProcessor
{
	internal class StaticProcesssor
    {
	    private static string _directory;
	    private static int _iteration = -1;

        public static void Main(string[] args)
        {
            Options.Parse(args);

            if (string.IsNullOrEmpty(_directory))
            {
                Console.WriteLine("No directory specified, exiting.");
                Environment.Exit(1);
            }

            if (!Directory.Exists(_directory))
            {
                Console.WriteLine("Invalid directory, exiting.");
                Environment.Exit(1);
            }

			CIlibParser parser;

			if (_iteration < 0) //parse only final output
				parser = new FinalOutputParser();
			else //parse specific iteration
				parser = new SingleIterationParser(_iteration);
			
			List<Algorithm> algorithms = parser.ParseDirectory(_directory);

			foreach (Algorithm alg in algorithms)
			{
				alg.Measurements.RemoveAll(x => x.Name != "Fitness");
			}

            //AdditionalMeasures.AddConsistency(algorithms);
            //AdditionalMeasures.AddSuccessRate(algorithms, 1000);

			StaticRanker.Rank(algorithms, _directory);
        }

		/// <summary>
		/// Specify the command line arguments.
		/// </summary>
		private static readonly OptionSet Options = new OptionSet
		{
			{ "d|directory=", "The directory to parse. ", v => _directory = v },
			{ "i|iteration=", "The iteration to summarize", v => _iteration = int.Parse(v) }
		};
	}
}
