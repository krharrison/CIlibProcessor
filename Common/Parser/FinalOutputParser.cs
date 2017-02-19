using System.IO;
using System.Linq;
using MiscUtil.IO;

namespace CIlibProcessor.Common
{
	/// <summary>
	/// Reads only the final line of output from a CIlib output file.
	/// </summary>
	public class FinalOutputParser : CIlibParser
	{
		/// <summary>
		/// Uses a ReverseLineReader to read only the final line of output.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="filename">Filename.</param>
		public override void ReadBody(StreamReader reader, string filename)
		{
			ReverseLineReader revReader = new ReverseLineReader(filename);
			string line = revReader.First();
			ReadLine(line);
		}
	}
}
