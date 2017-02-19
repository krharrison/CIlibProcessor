using System;
using NDesk.Options;
using System.IO;
using CIlibProcessor.Common;
using System.Linq;
using System.Collections.Generic;

namespace ConvergenceProcessor
{
	class MainClass
	{
		static string file;

        static string directory;
        static int value = -1;
        static bool divergence;

        //static string measureName = "AverageParticleMovement";
        //static string outputDirName = "AverageParticleMovement";
        //static string outputFile = "Convergence.csv";
        //static string normalizedFile = "ConvergenceNorm.csv";

        static string measureName = "Fitness";
        static string outputDirName = "Fitness";
        static string outputFile = "Performance.csv";
        static string normalizedFile = "PerformanceNorm.csv";

		public static void Main (string[] args)
		{
			options.Parse(args);

            if(string.IsNullOrEmpty(file)) ProcessMovement();
            else ProcessConvergence();
		}

        static void ProcessMovement()
        {
            if(!Directory.Exists(directory))
            {
                Console.WriteLine("Invalid directory, exiting.");
                Environment.Exit(1);
            }

            if(value < 0)
            {
                Console.WriteLine("Value must be positive, exiting.");
                Environment.Exit(1);
            }

            string outputDir = Path.Combine(directory, outputDirName);
            if(!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string output = Path.Combine(outputDir, value + ".csv");

            char[] nameSplit = { '_' };
            string[] nameTokens;
            double inertia;
            double social;
            double cognitive;

            using(TextWriter writer = new StreamWriter(output))
            {
				foreach(string movementfile in Directory.EnumerateFiles(directory, "*.txt"))
                {
					//Algorithm alg = CIlibParserOld.Parse(movementfile);
					CIlibParser parser = new FullParser();
					Algorithm alg = parser.Parse(movementfile);
                    Measurement measure = alg.Measurements.Find(x => x.Name == measureName);
                    int iterations = 0;

                    if(divergence)
                    {
                        if(measure.FinalIteration.Average < value) iterations = measure.MaximumIterations;
                        else
                        {
                            IterationStats stats = measure.Stats.Skip(1).First(x => x.Average >= value);
                            iterations = stats.Iteration;
                        }
                    }
                    else
                    {  //check if the final value is less than the threshold, if not set it to max iters immediately
                        if(measure.FinalIteration.Average > value) iterations = measure.MaximumIterations;
                        else
                        {
                            IterationStats stats = measure.Stats.Skip(1).First(x => x.Average <= value);
                            iterations = stats.Iteration;
                        }
                    }
                    nameTokens = alg.Name.Split(nameSplit, StringSplitOptions.RemoveEmptyEntries);

                    if(!double.TryParse(nameTokens[1], out inertia))
                    {
                        Console.WriteLine("Error parsing inertia from: {0}", nameTokens[1]);
                    }

                    if(!double.TryParse(nameTokens[2], out cognitive))
                    {
                        Console.WriteLine("Error parsing cognitive from: {0}", nameTokens[2]);
                    }

                    if(!double.TryParse(nameTokens[3], out social))
                    {
                        Console.WriteLine("Error parsing social from: {0}", nameTokens[3]);
                    }

                    writer.WriteLine("{0},{1},{2}", cognitive + social, inertia, iterations);
                }

                writer.Close();
            }
        }

        class Performance
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

        static void ProcessConvergence()
        {
            if(!File.Exists(file)) {
                Console.WriteLine("Invalid file, exiting.");
                Environment.Exit(1);
            }

            string path = Path.GetDirectoryName(file);
            string output = Path.Combine(path, outputFile);

            List<Performance> values = new List<Performance>();

            using(TextReader reader = new StreamReader(file))
            using(TextWriter writer = new StreamWriter(output))
            {
                string line = reader.ReadLine();
                char[] lineSplit = { '&' };
                char[] nameSplit = { '_' };
                string[] lineTokens;
                string[] nameTokens;
                double inertia;
                double social;
                double cognitive;
                double average;
                            
                while(line != null)
                {
                    lineTokens = line.Split(lineSplit, StringSplitOptions.RemoveEmptyEntries);
                    nameTokens = lineTokens[0].Split(nameSplit, StringSplitOptions.RemoveEmptyEntries);

                    if(!double.TryParse(nameTokens[1], out inertia))
                    {
                        Console.WriteLine("Error parsing inertia from: {0}", nameTokens[1]);
                    }

                    if(!double.TryParse(nameTokens[2], out cognitive))
                    {
                        Console.WriteLine("Error parsing cognitive from: {0}", nameTokens[2]);
                    }

                    if(!double.TryParse(nameTokens[3], out social))
                    {
                        Console.WriteLine("Error parsing social from: {0}", nameTokens[3]);
                    }

                    if(!double.TryParse(lineTokens[3], out average))
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
            string normalizedOutput = Path.Combine(path, normalizedFile);
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
		static OptionSet options = new OptionSet
		{
			{ "f|file=", "The file to parse. ", v => file = v },
            { "d|directory=", "The directory to parse. ", v => directory = v },
            { "m|movement=", "The movement value.", v => int.TryParse(v, out value)},
            { "D|divergent", "Set this flag if divergence behviour is to be examined", v => divergence = true}
		};
	}
}
