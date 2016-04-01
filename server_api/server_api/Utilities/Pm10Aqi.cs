using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using server_api.Models;

namespace server_api.Utilities
{
    public class Pm10Aqi
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
            if (value <= 54.0)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.0, 54.0);
                Tuple<int, int> I = new Tuple<int, int>(0, 50);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 154.0)
            {
                Tuple<double, double> BP = new Tuple<double, double>(55.0, 154.0);
                Tuple<int, int> I = new Tuple<int, int>(51, 100);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 254)
            {
                Tuple<double, double> BP = new Tuple<double, double>(155.0, 254.0);
                Tuple<int, int> I = new Tuple<int, int>(101, 150);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 354.0)
            {
                Tuple<double, double> BP = new Tuple<double, double>(255.0, 354.0);
                Tuple<int, int> I = new Tuple<int, int>(151, 200);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 424.0)
            {
                Tuple<double, double> BP = new Tuple<double, double>(355.0, 424.0);
                Tuple<int, int> I = new Tuple<int, int>(201, 300);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 504.0)
            {
                Tuple<double, double> BP = new Tuple<double, double>(425.0, 504.0);
                Tuple<int, int> I = new Tuple<int, int>(301, 400);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            return new Tuple<Tuple<double, double>, Tuple<int, int>>(new Tuple<double, double>(505, 604), new Tuple<int, int>(401, 500));
        }
    }
}