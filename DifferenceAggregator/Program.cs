using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIlibProcessor.Common;
using NDesk.Options;

namespace DifferenceAggregator
{
    /// <summary>
    /// This program reads a list of CSV difference files, aggregates the differences scores, then writes an aggregated file.
    /// </summary>
    internal class Program
    {
        private static readonly List<string> Files = new List<string>();
        private static string _output = "aggregate.csv";
        private static string _averages = "average.csv";

        private static void Main(string[] args)
        {
            Options.Parse(args);

            Dictionary<string, List<RankOutput>> result = new Dictionary<string, List<RankOutput>>();
            foreach(string file in Files)
            {
                //check that the file exists before attempting to parse
                if(!File.Exists(file))
                {
                    Console.WriteLine($"File \"{file}\" does not exist");
                    return;
                }

                string contents;
                //read the entire file
                using(StreamReader reader = new StreamReader(file))
                {
                    reader.ReadLine(); //discard the header line
                    contents = reader.ReadToEnd();
                }


                //parse the file, add the results to the aggregate
                foreach(string line in contents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] tokens = line.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    string name = tokens[0];
                    double wins = double.Parse(tokens[1]);
                    double losses = double.Parse(tokens[2]);
                    int rank = int.Parse(tokens[4]);

                    if(result.ContainsKey(name))
                        result[name].Add(new RankOutput(name, wins, losses, rank));
                    else
                        result.Add(name, new List<RankOutput>{ new RankOutput(name, wins, losses, rank) });
                }

            }
            //create directory for output if it doesn't exist
            DirectoryInfo dir = Directory.GetParent(_output);
            if(!dir.Exists)
                dir.Create();

            Dictionary<string, RankOutput> aggregate = new Dictionary<string, RankOutput>();

            int numResults = result.Values.First().Count;

            using(TextWriter avgWriter = new StreamWriter(_averages))
            {    
                avgWriter.WriteLine("Algorithm,Wins,Losses,Diff,Rank,Rank Deviation,Best Rank Freq");
                foreach(var pair in result)
                {
                    double wins = 0;
                    double losses = 0;
                    int rankSum = 0;
                    int bestCount = 0;

                    //aggregate!
                    foreach(RankOutput r in pair.Value)
                    {
                        wins += r.Wins;
                        losses += r.Losses;
                        rankSum += r.Rank;
                        if(r.Rank == 1) bestCount++;
                    }

                    aggregate.Add(pair.Key, new RankOutput(pair.Key, wins, losses, rankSum));

                    //handle the average stuffs
                    double avgWins = wins / numResults;
                    double avgLosses = losses / numResults;
                    double avgDiff = avgWins - avgLosses;
                    double avgRank = rankSum / (double)numResults;

                    //the standard deviation of the ranks
                    double devSum = pair.Value.Sum(x => Math.Pow(x.Rank - avgRank, 2));
                    double rankDev = Math.Sqrt(devSum / (numResults - 1));

                    avgWriter.WriteLine($"{pair.Key},{avgWins:0.000},{avgLosses:0.000},{avgDiff:0.000},{avgRank:0.000},{rankDev:0.000},{bestCount}");
                }
            }


            //write the aggregate results
            using(TextWriter writer = new StreamWriter(_output))
            {
                writer.WriteLine("Algorithm,Wins,Losses,Difference,Rank");
                foreach(var pair in aggregate.OrderBy(x => x.Key))
                {
                    RankOutput rankOutput = pair.Value;
                    int rank = aggregate.Count(x => x.Value.Difference > rankOutput.Difference) + 1; //this is horrible runtime, but correctly ranks the solutions
                    writer.WriteLine($"{rankOutput.Algorithm},{rankOutput.Wins:0},{rankOutput.Losses:0},{rankOutput.Difference:0},{rank}");
                }
            }
                

            using(TextWriter writer = new StreamWriter($"{_output}.tex"))
            {
                //write header
                writer.WriteLine("\\begin{table}");
                writer.WriteLine("\\centering");
                writer.WriteLine("\\caption{Overall Difference and Rank}");
                writer.WriteLine("\\label{tbl:results}");
                writer.WriteLine("\\begin{tabular}{l|rr}");

                //write column names
                writer.WriteLine(" & \\multicolumn{2}{c}{\\textbf{MEASURE}} \\\\");
                writer.WriteLine("\\ \\textbf{Algorithm} &  Difference & Rank \\\\ \\hline");

                //write the body
                foreach(var pair in aggregate.OrderBy(x => x.Key))
                {
                    int rank = aggregate.Count(x => x.Value.Difference > pair.Value.Difference) + 1; //this is horrible runtime, but correctly ranks the solutions
                    //bold the rank 1
                    //if (rank == 1)
                    //{
                    //    writer.WriteLine("{0} & {1:0.000} & \\textbf{{1}} \\\\", pair.Key, pair.Value.Difference);
                    //}
                    //else
                    writer.WriteLine($"{pair.Key} & {pair.Value.Difference:0.000} & {rank}  \\\\");
                }

                //write footer
                writer.WriteLine("\\end{tabular}");
                writer.WriteLine("\\end{table}");
            } 
        }

		/// <summary>
		/// Specify the command line arguments.
		/// </summary>
		private static readonly OptionSet Options = new OptionSet
		{
			{ "f|file=", "The files to process.", v => Files.Add(v) },
			{ "o|outfile=", "The file to use as output.", v => _output = v },
			{ "a|avgfile=", "The file to use as output for the averages.", v => _averages = v }
		};
	}
}
