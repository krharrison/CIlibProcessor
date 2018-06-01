using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIlibProcessor.Common;

namespace SuccessRateProcessor
{
    public class SuccessRateCalculator
    {
        private double accuracy;
        
        public SuccessRateCalculator(double? accuracy)
        {
            this.accuracy = accuracy ?? 10e-8;
        }
        
        public void Calculate(List<Algorithm> algorithms)
        {
            char[] nameSplit = { '_' };

            using (TextWriter writer = new StreamWriter("test.csv"))
            {

                foreach (var algorithm in algorithms)
                {
                    var nameTokens = algorithm.Name.Split(nameSplit, StringSplitOptions.RemoveEmptyEntries);

                    if (!double.TryParse(nameTokens[1], out var inertia))
                    {
                        Console.WriteLine("Error parsing inertia from: {0}", nameTokens[1]);
                    }

                    if (!double.TryParse(nameTokens[2], out var cognitive))
                    {
                        Console.WriteLine("Error parsing cognitive from: {0}", nameTokens[2]);
                    }

                    if (!double.TryParse(nameTokens[3], out var social))
                    {
                        Console.WriteLine("Error parsing social from: {0}", nameTokens[3]);
                    }


                    IterationStats stats = algorithm.Measurements.Find(x => x.Name == "Fitness").FinalIteration;
                    int count = stats.Values.Count(x => x < accuracy);

                    double successRate = (double) count / stats.Values.Count;

                    string successString = "";
                    if (successRate < double.Epsilon) //s = 0.0
                        successString = "F";
                    else if (successRate < 0.2) //0 < s < 0.2
                        successString = "VP";
                    else if (successRate < 0.4) //0.2 < s < 0.4
                        successString = "P";
                    else if (successRate < 0.6) //0.4 < s < 0.6
                        successString = "A";
                    else if (successRate < 0.8) //0.6 < s < 0.8
                        successString = "G";
                    else if (successRate < 1.0) //s = 1.0
                        successString = "VG";
                    else
                        successString = "S";


                    writer.WriteLine($"{inertia},{cognitive},{social},{successRate},{successString}");
                }

            }
        }
    }
}