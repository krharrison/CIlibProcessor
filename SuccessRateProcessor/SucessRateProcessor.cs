using System;
using System.Collections.Generic;
using System.IO;
using CIlibProcessor.Common;
using CIlibProcessor.Common.Parser;
using NDesk.Options;

namespace SuccessRateProcessor
{
    internal class SuccessRateProcessor
    {
        
        private static string _directory;
        private static bool _singleFile;
        private static double? _accuracy;
        private static int _iteration = -1;
        
        public static void Main(string[] args)
        {
            Options.Parse(args);

            //check that the file exists before attempting to parse
            if (!Directory.Exists(_directory))
            {
                Console.WriteLine($"Directory \"{_directory}\" does not exist");
                return;
            }

            CIlibParser parser;

            if (_iteration < 0) //parse only final output
                parser = new FinalOutputParser();
            else //parse entire file
                parser = new SingleIterationParser(_iteration);

            //parse either a single file or directory, depending on context
            List<Algorithm> algorithms = _singleFile ? new List<Algorithm> {parser.Parse(_directory)} : parser.ParseDirectory(_directory);
            
            SuccessRateCalculator calculator = new SuccessRateCalculator(_accuracy);
            calculator.Calculate(algorithms);
        }
        
        /// <summary>
        /// Specify the command line arguments.
        /// </summary>
        private static readonly OptionSet Options = new OptionSet
        {
            { "d|directory=", "The files to process.", v => _directory = v },
            { "f|file", "Summarize a single file", v => _singleFile =  true},
            { "a|accuracy=","The fixed accuracy level.", v => _accuracy = double.Parse(v) },
            { "i|iteration=", "The iteration to summarize", v => _iteration = int.Parse(v) }
        };
    }
}