using System;
using server_api.Models;

namespace server_api.Utilities
{
    public class OzoneAqi
    {
        public static Tuple<int, int> CalculateAQIAndCategory(double value)
        {
            var breakPoints = BreakPoints(value);

            var aqi = AQIFormula.CalculateAqi(breakPoints, value);

            var category = AQIFormula.CalculateCategory(aqi);

            return new Tuple<int, int>(aqi, category);
        }

        public static Tuple<Tuple<double, double>, Tuple<int, int>> BreakPoints(double value)
        {
            // 1 hour Ozone AQI values are only generated for values above 100.
            // Pg. 14 - https://www3.epa.gov/ttn/caaa/t1/memoranda/rg701.pdf

            if (value <= 0.164)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.125, 0.164);
                Tuple<int, int> I = new Tuple<int, int>(101, 150);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.204)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.164, 0.204);
                Tuple<int, int> I = new Tuple<int, int>(151, 200);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.404)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.204, 0.404);
                Tuple<int, int> I = new Tuple<int, int>(201, 300);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.504)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.404, 0.504);
                Tuple<int, int> I = new Tuple<int, int>(301, 400);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            return new Tuple<Tuple<double, double>, Tuple<int, int>>(new Tuple<double, double>(0.504, 0.604), new Tuple<int, int>(401, 500));
        }
    }
}