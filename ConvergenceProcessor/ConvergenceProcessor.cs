using System;
using NDesk.Options;
using System.IO;
using CIlibProcessor.Common;
using System.Linq;
using System.Collections.Generic;
using CIlibProcessor.Common.Parser;

namespace ConvergenceProcessor
{
    internal class MainClass
	{
	    private static string _file;

	    private static string _directory;
	    private static int _value = -1;
	    private static bool _divergence;

        //static string measureName = "AverageParticleMovement";
        //static string outputDirName = "AverageParticleMovement";
        //static string outputFile = "Convergence.csv";
        //static string normalizedFile = "ConvergenceNorm.csv";

	    private const string MeasureName = "Fitness";
	    private const string OutputDirName = "Fitness";
	    private const string OutputFile = "Performance.csv";
	    private const string NormalizedFile = "PerformanceNorm.csv";

	    public static void Main (string[] args)
		{
			Options.Parse(args);

            if(string.IsNullOrEmpty(_file)) ProcessMovement();
            else ProcessConvergence();
		}

	    private static void ProcessMovement()
        {
            if(!Directory.Exists(_directory))
            {
                Console.WriteLine("Invalid directory, exiting.");
                Environment.Exit(1);
            }

            if(_value < 0)
            {
                Console.WriteLine("Value must be positive, exiting.");
                Environment.Exit(1);
            }

            string outputDir = Path.Combine(_directory, OutputDirName);
            if(!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string output = Path.Combine(outputDir, _value + ".csv");

            char[] nameSplit = { '_' };

            using(TextWriter writer = new StreamWriter(output))
            {
				foreach(string movementfile in Directory.EnumerateFiles(_directory, "*.txt"))
                {
					//Algorithm alg = CIlibParserOld.Parse(movementfile);
					CIlibParser parser = new FullParser();
					Algorithm alg = parser.Parse(movementfile);
                    Measurement measure = alg.Measurements.Find(x => x.Name == MeasureName);
                    int iterations;

                    if(_divergence)
                    {
                        if(measure.FinalIteration.Average < _value) iterations = measure.MaximumIterations;
                        else
                        {
                            IterationStats stats = measure.Stats.Skip(1).First(x => x.Average >= _value);
                            iterations = stats.Iteration;
                        }
                    }
                    else
                    {  //check if the final value is less than the threshold, if not set it to max iters immediately
                        if(measure.FinalIteration.Average > _value) iterations = measure.MaximumIterations;
                        else
                        {
                            IterationStats stats = measure.Stats.Skip(1).First(x => x.Average <= _value);
                            iterations = stats.Iteration;
                        }
                    }
                    var nameTokens = alg.Name.Split(nameSplit, StringSplitOptions.RemoveEmptyEntries);

                    if(!double.TryParse(nameTokens[1], out var inertia))
                    {
                        Console.WriteLine("Error parsing inertia from: {0}", nameTokens[1]);
                    }

                    if(!double.TryParse(nameTokens[2], out var cognitive))
                    {
                        Console.WriteLine("Error parsing cognitive from: {0}", nameTokens[2]);
                    }

                    if(!double.TryParse(nameTokens[3], out var social))
                    {
                        Console.WriteLine("Error parsing social from: {0}", nameTokens[3]);
                    }

                    writer.WriteLine("{0},{1},{2}", cognitive + social, inertia, iterations);
                }

                writer.Close();
            }
        }

	    private class Performance
        {
            public double C { get; }
            public double W { get; }
            public double Value {get; }

            public Performance(double c, double w, double val)
            {
                C = c;
                W = w;
                Value = val;
            }
        }

	    private static void ProcessConvergence()
        {
            if(!File.Exists(_file)) {
                Console.WriteLine("Invalid file, exiting.");
                Environment.Exit(1);
            }

            string path = Path.GetDirectoryName(_file);
            string output = Path.Combine(path, OutputFile);

            List<Performance> values = new List<Performance>();

            using(TextReader reader = new StreamReader(_file))
            using(TextWriter writer = new StreamWriter(output))
            {
                string line = reader.ReadLine();
                char[] lineSplit = { '&' };
                char[] nameSplit = { '_' };

                while(line != null)
                {
                    var lineTokens = line.Split(lineSplit, StringSplitOptions.RemoveEmptyEntries);
                    var nameTokens = lineTokens[0].Split(nameSplit, StringSplitOptions.RemoveEmptyEntries);

                    if(!double.TryParse(nameTokens[1], out var inertia))
                    {
                        Console.WriteLine("Error parsing inertia from: {0}", nameTokens[1]);
                    }

                    if(!double.TryParse(nameTokens[2], out var cognitive))
                    {
                        Console.WriteLine("Error parsing cognitive from: {0}", nameTokens[2]);
                    }

                    if(!double.TryParse(nameTokens[3], out var social))
                    {
                        Console.WriteLine("Error parsing social from: {0}", nameTokens[3]);
                    }

                    if(!double.TryParse(lineTokens[3], out var average))
                    {
                        Console.WriteLine("Error parsing average from: {0}", lineTokens[3]);
                    }

                    writer.WriteLine("{0},{1},{2}", cognitive + social, inertia, average);
                    values.Add(new Performance(cognitive+social, inertia, average));

                    line = reader.ReadLine();
                }

                writer.Close();
                reader.Close();
            }

            double max = values.Max(x => x.Value);
            double min = values.Min(x => x.Value);
            string normalizedOutput = Path.Combine(path, NormalizedFile);
            using(TextWriter writer = new StreamWriter(normalizedOutput))
            {
                foreach(Performance p in values)
                {
                    double norm = (p.Value - min) / (max - min);
                    writer.WriteLine($"{p.C},{p.W},{norm}");
                }

                writer.Close();
            }
                
        }

		/// <summary>
		/// Specify the command line arguments.
		/// </summary>
		private static readonly OptionSet Options = new OptionSet
		{
			{ "f|file=", "The file to parse. ", v => _file = v },
            { "d|directory=", "The directory to parse. ", v => _directory = v },
            { "m|movement=", "The movement value.", v => int.TryParse(v, out _value)},
            { "D|divergent", "Set this flag if divergence behviour is to be examined", v => _divergence = true}
		};
	}
}
