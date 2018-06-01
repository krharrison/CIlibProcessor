using System;
using NDesk.Options;
using CIlibProcessor.Common;
using System.IO;
using System.Collections.Generic;
using CIlibProcessor.Common.Parser;

namespace SummaryStatistics
{
	internal class Program
	{
	    private static string _files;
	    private static bool _singleFile;
	    private static double? _minimum;
	    private static bool _verbose;
	    private static int _iteration = -1;

		private static void Main(string[] args)
		{
			Options.Parse(args);

			//check that the file exists before attempting to parse
			if (!Directory.Exists(_files))
			{
				Console.WriteLine($"Directory \"{_files}\" does not exist");
				return;
			}

			CIlibParser parser;

			if (_iteration < 0) //parse only final output
				parser = new FinalOutputParser();
			else //parse entire file
				parser = new SingleIterationParser(_iteration);

			//parse either a single file or directory, depending on context
			List<Algorithm> algorithms = _singleFile ? new List<Algorithm> {parser.Parse(_files)} : parser.ParseDirectory(_files);

			string outputPath = Path.Combine(_files, "summaries");

			Summarizer sum = new Summarizer(_minimum);
		    sum.Summarize(algorithms, outputPath, _verbose);
		}



		/// <summary>
		/// Specify the command line arguments.
		/// </summary>
		private static readonly OptionSet Options = new OptionSet
		{
			{ "d|directory=", "The files to process.", v => _files = v },
		    {"f|file", "Summarize a single file", v => _singleFile =  true},
			{ "m|min=","The minimal value. Values below the minimum are considered 0.", v => _minimum = double.Parse(v) },
			{ "v|verbose", "Show console output.", v => _verbose = true },
			{ "i|iteration=", "The iteration to summarize", v => _iteration = int.Parse(v) }
		};
	}
}
