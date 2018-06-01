namespace CIlibProcessor.Common
{
	public class RankOutput
	{

		public string Algorithm { get; }

		public double Wins { get; set; }

		public double Losses { get; set; }

		/// <summary>
		/// Gets the normalized difference between wins and losses.
		/// </summary>
		/// <value>The difference.</value>
		public double Difference => Wins - Losses;

	    public int Rank { get; set; }

		public RankOutput(string algorithm, double wins, double losses, int rank = int.MaxValue)
		{
			Algorithm = algorithm;
			Wins = wins;
			Losses = losses;
			Rank = rank;
		}

	}
}
