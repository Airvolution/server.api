using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using server_api.Models;
using server_api.Utilities;

namespace server_api.unit_testing
{
    [TestClass]
    public class Unit_AQIFormula
    {
        [TestMethod]
        public void TestPm25AqiExpectedCategories()
        {
            Tuple<int,int> result = Pm25Aqi.CalculateAQIAndCategory(0);

            Assert.AreEqual(0, result.Item1);

            // 0 to 12.1 should be green
            for (double d = 0; d <= 12.1; d += 0.1)
            {
                result = Pm25Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(1, result.Item2);
            }

            // 12.2 to 35.4 should be yellow
            for (double d = 12.2; d <= 35.4; d += 0.1)
            {
                result = Pm25Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(2, result.Item2);
            }

            // 35.5 to 55.4 should be orange
            for (double d = 35.5; d <= 55.4; d += 0.1)
            {
                result = Pm25Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(3, result.Item2);
            }

            // 55.5 to 150.4 should be red
            for (double d = 55.5; d <= 55.4; d += 0.1)
            {
                result = Pm25Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(4, result.Item2);
            }

            // 150.5 to 250.4 should be purple
            for (double d = 150.5; d <= 250.4; d += 0.1)
            {
                result = Pm25Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(5, result.Item2);
            }

            // 250.5 to 350.4 should be maroon 
            for (double d = 250.5; d <= 500; d += 0.1)
            {
                result = Pm25Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(6, result.Item2);
            }
        }

        [TestMethod]
        public void TestCoAqiExpectedCategories()
        {
            Tuple<int, int> result = CoAqi.CalculateAQIAndCategory(0);

            Assert.AreEqual(0, result.Item1);

            // 0 to 12.1 should be green
            for (double d = 0; d <= 12.1; d += 0.1)
            {
                result = Pm25Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(1, result.Item2);
            }
        }

    }
}
