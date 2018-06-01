using System.Collections.Generic;

namespace CIlibProcessor.Common
{
	public class OrderResult
	{
		public List<double> Ranks { get; }
		public List<int> Indexes { get; }

		public OrderResult(List<double> ranks, List<int> indexes)
		{
			Ranks = ranks;
			Indexes = indexes;
		}
	}
}
