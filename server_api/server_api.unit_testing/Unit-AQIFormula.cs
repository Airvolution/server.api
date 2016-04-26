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
            for (double d = 0; d < 12.1; d += 0.1)
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

            // 0 to 4.4 should be green
            for (double d = 0; d <= 4.4; d += 0.1)
            {
                result = CoAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(1, result.Item2);
            }
            // 4.5 to 9.4 should be yellow
            for (double d = 4.5; d <= 9.4; d += 0.1)
            {
                result = CoAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(2, result.Item2);
            }
            // 9.5 to 12.4 should be orange
            for (double d = 9.5; d <= 12.4; d += 0.1)
            {
                result = CoAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(3, result.Item2);
            }
            // 12.5 to 15.4 should be red
            for (double d = 12.5; d <= 15.4; d += 0.1)
            {
                result = CoAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(4, result.Item2);
            }
            // 15.5 to 30.4 should be purple
            for (double d = 15.5; d <= 30.4; d += 0.1)
            {
                result = CoAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(5, result.Item2);
            }
            // 30.5 to 50.4 should be maroon
            for (double d = 30.5; d <= 50.4; d += 0.1)
            {
                result = CoAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(6, result.Item2);
            }
        }

        [TestMethod]
        public void TestNo2AqiExpectedCategories()
        {
            Tuple<int, int> result = No2Aqi.CalculateAQIAndCategory(0);            

            // 0.65 to 1.24 should be purple
            for (double d = 0.65; d <= 1.24; d += 0.01)
            {
                result = No2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(5, result.Item2);
            }

            // 1.25 to 2.04 should be purple
            for (double d = 1.25; d <= 2.04; d += 0.01)
            {
                result = No2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(6, result.Item2);
            }
        }

        [TestMethod]
        public void TestOzoneAqiExpectedCategories()
        {
            Tuple<int, int> result = OzoneAqi.CalculateAQIAndCategory(0);

            // 0.125 to 0.164 should be orange
            for (double d = 0.125; d <= 0.164; d += 0.001)
            {
                result = OzoneAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(3, result.Item2);
            }

            // 0.165 to 0.204 should be red
            for (double d = 0.165; d <= 0.204; d += 0.001)
            {
                result = OzoneAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(4, result.Item2);
            }

            // 0.205 to 0.404 should be purple
            for (double d = 0.205; d <= 0.404; d += 0.001)
            {
                result = OzoneAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(5, result.Item2);
            }
            // 0.405 to 0.604 should be maroon
            for (double d = 0.405; d <= 0.604; d += 0.001)
            {
                result = OzoneAqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(6, result.Item2);
            }
        }

        [TestMethod]
        public void TestPm10AqiExpectedCategories()
        {
            Tuple<int, int> result = Pm10Aqi.CalculateAQIAndCategory(0);

            // 0 to 54.0 should be green
            for (double d = 0; d <= 54.0; d += 0.1)
            {
                result = Pm10Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(1, result.Item2);
            }

            // 54.1 to 154.0 should be yellow
            for (double d = 54.1; d <= 154.0; d += 0.1)
            {
                result = Pm10Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(2, result.Item2);
            }

            // 154.1 to 254.0 should be orange
            for (double d = 154.1; d <= 254.0; d += 0.1)
            {
                result = Pm10Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(3, result.Item2);
            }

            // 254.1 to 354.0 should be red
            for (double d = 254.1; d <= 354.0; d += 0.1)
            {
                result = Pm10Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(4, result.Item2);
            }

            // 354.1 to 424.0 should be purple
            for (double d = 354.1; d <= 424.0; d += 0.1)
            {
                result = Pm10Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(5, result.Item2);
            }

            // 424.1 to 604.0 should be maroon
            for (double d = 424.1; d <= 604.0; d += 0.1)
            {
                result = Pm10Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(6, result.Item2);
            }
        }

        [TestMethod]
        public void TestSo2AqiExpectedCategories()
        {
            Tuple<int, int> result = So2Aqi.CalculateAQIAndCategory(0);

            // 0.0 to 0.034 should be green
            for (double d = 0.0; d <= 0.034; d += 0.001)
            {
                result = So2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(1, result.Item2);
            }

            // 0.035 to 0.144 should be yellow
            for (double d = 0.035; d <= 0.144; d += 0.001)
            {
                result = So2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(2, result.Item2);
            }

            // 0.145 to 0.244 should be orange
            for (double d = 0.145; d <= 0.224; d += 0.001)
            {
                result = So2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(3, result.Item2);
            }

            // 0.245 to 0.304 should be red
            for (double d = 0.225; d <= 0.304; d += 0.001)
            {
                result = So2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(4, result.Item2);
            }

            // 0.305 to 0.604 should be purple
            for (double d = 0.305; d <= 0.604; d += 0.001)
            {
                result = So2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(5, result.Item2);
            }

            // 0.605 to 1.004 should be maroon
            for (double d = 0.605; d <= 1.004; d += 0.001)
            {
                result = So2Aqi.CalculateAQIAndCategory(d);
                Assert.AreEqual(6, result.Item2);
            }
        }
    
    }
}
