using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VorratsUebersicht;

namespace UnitTestProject
{
    [TestClass]
    public class PricePerUnit_UnitTest
    {
        [TestMethod]
        public void Test_01_Liter()
        {
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "0.2",  "l"), "100 ml = 1.50");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "0.25", "l"), "100 ml = 1.20");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "0.7",  "l"), "1 Liter = 4.27");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "1",    "l"), "");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "1.5",  "l"), "1 Liter = 1.99");
        }

        [TestMethod]
        public void Test_02_ml()
        {
            Assert.AreEqual(PricePerUnit.Calculate("2.49",  "20", "ml"), "100 ml = 12.45");
            Assert.AreEqual(PricePerUnit.Calculate("2.49", "100", "ml"), "");
            Assert.AreEqual(PricePerUnit.Calculate("2.49", "250", "ml"), "100 ml = 1.00");
            Assert.AreEqual(PricePerUnit.Calculate("2.49", "500", "ml"), "1 Liter = 4.98");
        }

        [TestMethod]
        public void Test_03_kg()
        {
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "0.1",  "kg"), "100 g = 5.99");
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "0.25", "kg"), "100 g = 2.40");
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "0.5",  "kg"), "1 kg = 11.98");
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "1",    "kg"), "");
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "1.5",  "kg"), "1 kg = 3.99");
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "2",    "kg"), "1 kg = 3.00");
        }

        [TestMethod]
        public void Test_04_g()
        {
            Assert.AreEqual(PricePerUnit.Calculate("2.99",  "20", "g"), "100 g = 14.95");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "100", "g"), "");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "200", "g"), "100 g = 1.50");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "250", "g"), "100 g = 1.20");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "300", "g"), "1 kg = 9.97");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "600", "g"), "1 kg = 4.98");
        }

        [TestMethod]
        public void Test_05_dl()
        {
            Assert.AreEqual(PricePerUnit.Calculate("2.49",  "10", "cl"), "100 ml = 2.49");
            Assert.AreEqual(PricePerUnit.Calculate("2.49",  "25", "cl"), "100 ml = 1.00");
            Assert.AreEqual(PricePerUnit.Calculate("2.49",  "70", "cl"), "1 Liter = 3.56");
            Assert.AreEqual(PricePerUnit.Calculate("2.49", "100", "cl"), "1 Liter = 2.49");
        }
    }
}
