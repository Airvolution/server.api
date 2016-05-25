using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace server_api.Utilities
{
    public class AQIFormula
    {
        public static int CalculateAqi(Tuple<Tuple<double, double>, Tuple<int, int>> breakPoints, double value)
        {
            int Ilo = breakPoints.Item2.Item1;
            int Ihi = breakPoints.Item2.Item2;
            double BPlo = breakPoints.Item1.Item1;
            double BPhi = breakPoints.Item1.Item2;

            return (int)(((Ihi - Ilo) / (BPhi - BPlo)) * (value - BPlo) + Ilo);
        }

        public static int CalculateCategory(int aqi)
        {
            if (aqi <= 50)
            {
                return 1;
            }

            if (aqi <= 100)
            {
                return 2;
            }

            if (aqi <= 150)
            {
                return 3;
            }

            if (aqi <= 200)
            {
                return 4;
            }

            if (aqi <= 300)
            {
                return 5;
            }

            return 6;
        }
    }
}