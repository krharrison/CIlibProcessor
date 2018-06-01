using System;
using System.IO;
using System.Threading.Tasks;

namespace CIlibProcessor.Common.Parser
{

	/// <summary>
	/// Read only a single, specified iteration from a CIlib output file.
	/// </summary>
	public class SingleIterationParser : CIlibParser
	{
		private readonly int _iteration; 

		/// <summary>
		/// Initializes a new instance of the <see cref="T:CIlibProcessor.Common.Parser.SingleIterationParser"/> class.
		/// </summary>
		/// <param name="iteration">The iteration to parse.</param>
		public SingleIterationParser(int iteration)
		{
			_iteration = iteration;
		}

		/// <summary>
		/// Reads the entire file, then (in parallel) checks the start of each line to find the line
		/// which contains the required iteration. It then parses only the required line.
		/// If no line with the specified iteation is found, an exception is thrown.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="filename">Filename.</param>
		public override void ReadBody(StreamReader reader, string filename)
		{
			//read all lines, then use a parallel for-each loop to find the line we want
			string file = reader.ReadToEnd();
			reader.Close();
			string[] lines = file.Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			string pattern = _iteration + " "; //the string must begin with the iteration followed by a space
			string line = string.Empty;

			Parallel.ForEach(lines, (l, state)  =>
			{
				// ReSharper disable once InvertIf
				if (l.StartsWith(pattern, StringComparison.InvariantCulture))
				{
					line = l;
					state.Stop();
				}
			});

			if (string.IsNullOrEmpty(line)) throw new Exception($"File {filename} does not contain iteration {_iteration}");

			ReadLine(line);
		}
	}
}
