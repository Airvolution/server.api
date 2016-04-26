using System;

namespace server_api.Utilities
{
    public class Pm25Aqi
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
            if (value <= 12.0)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.0, 12.0);
                Tuple<int, int> I = new Tuple<int, int>(0, 50);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 35.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(12.0, 35.4);
                Tuple<int, int> I = new Tuple<int, int>(51, 100);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 55.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(35.4, 55.4);
                Tuple<int, int> I = new Tuple<int, int>(101, 150);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 150.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(55.4, 150.4);
                Tuple<int, int> I = new Tuple<int, int>(151, 200);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 250.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(150.4, 250.4);
                Tuple<int, int> I = new Tuple<int, int>(201, 300);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 350.4)
            {
                Tuple<double, double> BP = new Tuple<double, double>(250.4, 350.4);
                Tuple<int, int> I = new Tuple<int, int>(301, 400);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            return new Tuple<Tuple<double, double>, Tuple<int, int>>(new Tuple<double, double>(350.4, 500), new Tuple<int, int>(401, 500));
        }
    }
}