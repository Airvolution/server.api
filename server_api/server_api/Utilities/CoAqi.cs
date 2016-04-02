using System;
using server_api.Models;

namespace server_api.Utilities
{
    public class CoAqi
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
            if (value <= 4.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.0, 4.4);
                Tuple<int, int> I = new Tuple<int, int>(0, 50);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 9.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(4.5, 9.4);
                Tuple<int, int> I = new Tuple<int, int>(51, 100);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 12.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(9.5, 12.4);
                Tuple<int, int> I = new Tuple<int, int>(101, 150);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 15.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(12.5, 15.4);
                Tuple<int, int> I = new Tuple<int, int>(151, 200);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 30.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(15.5, 30.4);
                Tuple<int, int> I = new Tuple<int, int>(201, 300);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 40.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(35.5, 40.4);
                Tuple<int, int> I = new Tuple<int, int>(301, 400);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            return new Tuple<Tuple<double, double>, Tuple<int, int>>(new Tuple<double, double>(40.5, 50.4), new Tuple<int, int>(401, 500));
        }
    }
}