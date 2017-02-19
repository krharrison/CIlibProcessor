using System.Collections.Generic;

namespace CIlibProcessor.Common
{
	public class OrderResult
	{
		public List<double> Ranks { get; private set; }
		public List<int> Indexes { get; private set; }

		public OrderResult(List<double> ranks, List<int> indexes)
		{
			Ranks = ranks;
			Indexes = indexes;
		}
	}
}
