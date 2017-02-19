using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CIlibProcessor.Common
{
	public abstract class CIlibParser
	{
		protected ConcurrentDictionary<string, Measurement> measurements;
		protected string[] columnArray;
		protected readonly char[] splitChars = { ' ' };

		/// <summary>
		/// Parse a single CIlib output file.
		/// </summary>
		/// <param name="filename">The name of the file to parse.</param>
		public virtual Algorithm Parse(string filename)
		{
			measurements = new ConcurrentDictionary<string, Measurement>();

			if (!File.Exists(filename))
			{
				throw new ArgumentException("File does not exist!");
			}

			//use the filename as the algorithm name
			string name = Path.GetFileNameWithoutExtension(filename);

			using (StreamReader reader = File.OpenText(filename))
			{
				ReadHeader(reader);
				ReadBody(reader, filename);
			}

			return new Algorithm(name, measurements.Values.ToList());
		}

		public abstract void ReadBody(StreamReader reader, string filename);

		/// <summary>
		/// Parse and entire directory of CIlib output files, returning a list of Algorithms.
		/// </summary>
		/// <returns>The list of algorithms from this directory.</returns>
		/// <param name="directory">Directory.</param>
		public List<Algorithm> ParseDirectory(string directory)
		{
			List<Algorithm> algorithms = new List<Algorithm>();

			foreach (string file in Directory.EnumerateFiles(directory, "*.txt"))
			{
				algorithms.Add(Parse(file));
			}

			return algorithms;
		}

		/// <summary>
		/// Reads the header.
		/// </summary>
		/// <param name="reader">An opened StreamReader which has not been read from.</param>
		protected virtual void ReadHeader(StreamReader reader)
		{
			string line = reader.ReadLine(); //ignore iteration line, which is assumed to be the first line

			List<string> columns = new List<string>();
			//while (line.StartsWith("#", StringComparison.Ordinal)) //process header to decipher the column structure
			//{
			do
			{
				line = reader.ReadLine();
				string[] tokens = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
				string columnName = tokens[3];
				columns.Add(columnName);

				//line = reader.ReadLine();
			} while (reader.Peek() == '#');
			//}

			//create an easily indexed array of column names
			columnArray = columns.ToArray();
		}

		protected virtual void ReadLine(string line)
		{
			string[] tokens = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
			int iter;
			if (!int.TryParse(tokens[0], out iter))
			{
				Console.WriteLine("Iteration is not the first column.");
			}

			string columnName;

			int index = 1; //the index of the column being parsed

			//loop through each column
			while (index < columnArray.Length)
			{
				columnName = columnArray[index];

				List<double> values = new List<double>();

				//loop through the various columns (i.e., runs) for this measure
				while (index <= columnArray.Length && columnArray[index - 1] == columnName)
				{
					values.Add(double.Parse(tokens[index]));
					index++;
				}

				IterationStats stats = new IterationStats(iter, values);

				measurements.AddOrUpdate(columnName,
					x => //add function
					{
						Measurement meas = new Measurement(columnName);
						meas.AddIterationStatistics(iter, stats);
						return meas;
					},
					(name, meas) => //update function
					{
						meas.AddIterationStatistics(iter, stats);
						return meas;
					}
				);
			}
		}
	}
}
