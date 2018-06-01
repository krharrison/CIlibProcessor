using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CIlibProcessor.Common.Parser
{
	public abstract class CIlibParser
	{
		protected ConcurrentDictionary<string, Measurement> Measurements;
		protected string[] ColumnArray;
		protected readonly char[] SplitChars = { ' ' };

		/// <summary>
		/// Parse a single CIlib output file.
		/// </summary>
		/// <param name="filename">The name of the file to parse.</param>
		public virtual Algorithm Parse(string filename)
		{
			Measurements = new ConcurrentDictionary<string, Measurement>();

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

			return new Algorithm(name, Measurements.Values.ToList());
		}

		public abstract void ReadBody(StreamReader reader, string filename);

		/// <summary>
		/// Parse and entire directory of CIlib output files, returning a list of Algorithms.
		/// </summary>
		/// <returns>The list of algorithms from this directory.</returns>
		/// <param name="directory">Directory.</param>
		public List<Algorithm> ParseDirectory(string directory)
		{
			return Directory.EnumerateFiles(directory, "*.txt").Select(Parse).ToList();
		}

		/// <summary>
		/// Reads the header.
		/// </summary>
		/// <param name="reader">An opened StreamReader which has not been read from.</param>
		protected virtual void ReadHeader(StreamReader reader)
		{
			// ReSharper disable once RedundantAssignment
			string line = reader.ReadLine(); //ignore iteration line, which is assumed to be the first line

			List<string> columns = new List<string>();
			do
			{
				line = reader.ReadLine();
				string[] tokens = line?.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
				string columnName = tokens?[3];
				columns.Add(columnName);
			} while (reader.Peek() == '#');

			//create an easily indexed array of column names
			ColumnArray = columns.ToArray();
		}

		/// <summary>
		/// Read an individual line from a CIlib output file
		/// </summary>
		/// <param name="line">The line to be read.</param>
		protected virtual void ReadLine(string line)
		{
			string[] tokens = line.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
			if (!int.TryParse(tokens[0], out var iter))
			{
				Console.WriteLine("Iteration is not the first column.");
			}

			string columnName;

			int index = 0; //the index of the column being parsed

			//loop through each column
			while (index < ColumnArray.Length)
			{
				columnName = ColumnArray[index];

				List<double> values = new List<double>();

				//loop through the various columns (i.e., runs) for this measure
				while (index < ColumnArray.Length && ColumnArray[index] == columnName)
				{
					values.Add(double.Parse(tokens[index + 1])); //add 1 to offset iteration as first column
					index++;
				}

				IterationStats stats = new IterationStats(iter, values);

				Measurements.AddOrUpdate(columnName,
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
