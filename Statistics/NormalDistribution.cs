using System;
namespace CIlibProcessor.Statistics
{
	public static class NormalDistribution
	{
		static readonly double[] a = {
			2.2352520354606839287,
			161.02823106855587881,
			1067.6894854603709582,
			18154.981253343561249,
			0.065682337918207449113
		};

		static readonly double[] b = {
			47.20258190468824187,
			976.09855173777669322,
			10260.932208618978205,
			45507.789335026729956
		};

		static readonly double[] c = {
			0.39894151208813466764,
			8.8831497943883759412,
			93.506656132177855979,
			597.27027639480026226,
			2494.5375852903726711,
			6848.1904505362823326,
			11602.651437647350124,
			9842.7148383839780218,
			1.0765576773720192317e-8
		};
		static readonly double[] d = {
			22.266688044328115691,
			235.38790178262499861,
			1519.377599407554805,
			6485.558298266760755,
			18615.571640885098091,
			34900.952721145977266,
			38912.003286093271411,
			19685.429676859990727
		};
		static readonly double[] p = {
			0.21589853405795699,
			0.1274011611602473639,
			0.022235277870649807,
			0.001421619193227893466,
			2.9112874951168792e-5,
			0.02307344176494017303
		};
		static readonly double[] q = {
			1.28426009614491121,
			0.468238212480865118,
			0.0659881378689285515,
			0.00378239633202758244,
			7.29751555083966205e-5
		};


		public static double pnorm(double x, double mu, double sigma, bool lower_tail, bool log_p)
		{
		    double cp = 0;
			double lp = (x - mu) / sigma;

			x = lp;

			pnorm_both(x, ref lp, ref cp, (lower_tail ? 0 : 1), log_p);

			return (lower_tail ? lp : cp);
		}

		public static void pnorm_both(double x, ref double cum, ref double ccum, int i_tail, bool log_p)
		{
			double xden, xnum, temp, del = 0, xsq = 0;
			int i;

			double DBL_EPSILON = 10e-16;

			double eps = DBL_EPSILON * 0.5; //TODO: DBL_EPSILON precision

			/* i_tail in {0,1,2} =^= {lower, upper, both} */
			bool lower = i_tail != 1;
			bool upper = i_tail != 0;

			double y = Math.Abs(x); //TODO: fabs = Math.Abs?

			if (y <= 0.67448975)
			{ /* qnorm(3/4) = .6744.... -- earlier had 0.66291 */
				if (y > eps)
				{
					xsq = x * x;
					xnum = a[4] * xsq;
					xden = xsq;
					for (i = 0; i < 3; ++i)
					{
						xnum = (xnum + a[i]) * xsq;
						xden = (xden + b[i]) * xsq;
					}
				}
				else xnum = xden = 0.0;
				temp = x * (xnum + a[3]) / (xden + b[3]);
				if (lower) cum = 0.5 + temp;
				if (upper) ccum = 0.5 - temp;
				if (log_p)
				{
					if (lower) cum = Math.Log(cum);
					if (upper) ccum = Math.Log(ccum);
				}
			}
			else if (y <= 5.656854249492380195206754896838)
			{   //sqrt(32)
				xnum = c[8] * y;
				xden = y;
				for (i = 0; i < 7; ++i)
				{
					xnum = (xnum + c[i]) * y;
					xden = (xden + d[i]) * y;
				}
				temp = (xnum + c[7]) / (xden + d[7]);

				DoDel(y, ref x, ref xsq, ref del, ref cum, ref ccum, ref temp, log_p, lower, upper);
				SwapTail(x, ref temp, ref cum, ref ccum, lower);
			}
			else if ((log_p && y < 1e170)
					 || (lower && -37.5193 < x && x < 8.2924)
					 || (upper && -8.2924 < x && x < 37.5193))
			{
				xsq = 1.0 / (x * x); /* (1./x)*(1./x) might be better */
				xnum = p[5] * xsq;
				xden = xsq;
				for (i = 0; i < 4; ++i)
				{
					xnum = (xnum + p[i]) * xsq;
					xden = (xden + q[i]) * xsq;
				}
				temp = xsq * (xnum + p[4]) / (xden + q[4]);
				temp = (0.398942280401432677939946059934 - temp) / y; // 1/sqrt(2pi)

				DoDel(x, ref x, ref xsq, ref del, ref cum, ref ccum, ref temp, log_p, lower, upper);
				SwapTail(x, ref temp, ref cum, ref ccum, lower);
			}
			else
			{ /* large x such that probs are 0 or 1 */
				if (x > 0)
				{
					cum = (log_p ? 0 : 1);//R_D__1; 
					ccum = (log_p ? double.NegativeInfinity : 0);
				}
				else {
					cum = (log_p ? double.NegativeInfinity : 0);
					ccum = (log_p ? 0 : 1);
				}
			}

			//TODO: if no_denorms?

		}

	    private static void DoDel(double X, ref double x, ref double xsq, ref double del, ref double cum, ref double ccum, ref double temp, bool log_p, bool lower, bool upper) //TODO: pass x by reference or not?
		{
			xsq = Math.Truncate(X * 16) / 16;
			del = (X - xsq) * (X + xsq);
			if (log_p)
			{
				cum = (-xsq * xsq * 0.5) + (-del * 0.5) + Math.Log(temp);
				if ((lower && x > 0) || (upper && x <= 0))          //TODO: what is 0. in C?
					ccum = Math.Log(1 + (-Math.Exp(-xsq * xsq * 0.5) * Math.Exp(-del * 0.5) * temp));   //TODO: this is log1p	
			}
			else {
				cum = Math.Exp(-xsq * xsq * 0.5) * Math.Exp(-del * 0.5) * temp;
				ccum = 1.0 - cum;
			}
		}

	    private static void SwapTail(double x, ref double temp, ref double cum, ref double ccum, bool lower)
		{
			if (x > 0)
			{/* swap  ccum <--> cum */
				temp = cum;
				if (lower) cum = ccum;
				ccum = temp;    //TODO: weird logic...
			}
		}
	}
}
