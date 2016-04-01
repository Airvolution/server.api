using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using server_api.Models;

namespace server_api.Utilities
{
    public class So2Aqi
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
            if (value <= 0.034)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.0, 0.034);
                Tuple<int, int> I = new Tuple<int, int>(0, 50);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.144)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.035, 0.144);
                Tuple<int, int> I = new Tuple<int, int>(51, 100);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.224)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.145, 0.224);
                Tuple<int, int> I = new Tuple<int, int>(101, 150);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.304)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.225, 0.304);
                Tuple<int, int> I = new Tuple<int, int>(151, 200);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.604)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.305, 0.604);
                Tuple<int, int> I = new Tuple<int, int>(201, 300);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            if (value <= 0.804)
            {
                Tuple<double, double> BP = new Tuple<double, double>(0.605, 0.804);
                Tuple<int, int> I = new Tuple<int, int>(301, 400);
                return new Tuple<Tuple<double, double>, Tuple<int, int>>(BP, I);
            }

            return new Tuple<Tuple<double, double>, Tuple<int, int>>(new Tuple<double, double>(0.0804, 1.004), new Tuple<int, int>(401, 500));
        }
    }
}