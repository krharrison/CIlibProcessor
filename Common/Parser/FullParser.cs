using System;
using System.IO;
using System.Threading.Tasks;

namespace CIlibProcessor.Common
{
	/// <summary>
	/// Reads all lines from a CIlib ouput file in parallel.
	/// </summary>
	public class FullParser : CIlibParser
	{
		/// <summary>
		/// Reads all lines after the header in parallel.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="filename">Filename.</param>
		public override void ReadBody(StreamReader reader, string filename)
		{
			//read all lines, then parse in parallel
			string file = reader.ReadToEnd();
			reader.Close();
			string[] lines = file.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			Parallel.ForEach(lines, l =>
			{
				ReadLine(l);
			});
		}
	}
}
