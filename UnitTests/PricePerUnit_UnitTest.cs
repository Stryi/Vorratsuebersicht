using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VorratsUebersicht;

namespace UnitTestProject
{
    [TestClass]
    public class PricePerUnit_UnitTest
    {
        [TestMethod]
        public void Test_01_l_ml()
        {
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "1",   "l"), "0.30 pro 100 ml");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "0.7", "l"), "0.43 pro 100 ml");
        }

        [TestMethod]
        public void Test_02_ml_ml()
        {
            Assert.AreEqual(PricePerUnit.Calculate("2", "100",   "ml"), "2.00 pro 100 ml");
            Assert.AreEqual(PricePerUnit.Calculate("2", "500",   "ml"), "0.40 pro 100 ml");
        }

        [TestMethod]
        public void Test_03_kg_g()
        {
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "1",   "kg"), "0.60 pro 100 g");
            Assert.AreEqual(PricePerUnit.Calculate("5.99", "0.5", "kg"), "1.20 pro 100 g");
        }

        [TestMethod]
        public void Test_04_g_g()
        {
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "100", "g"), "2.99 pro 100 g");
            Assert.AreEqual(PricePerUnit.Calculate("2.99", "200", "g"), "1.50 pro 100 g");
        }
    }
}
