using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VorratsUebersicht;

namespace UnitTestProject
{
    [TestClass]
    public class UnitConvert_UnitTest
    {
        [TestMethod]
        public void Test_01_GetCaloriePerUnit()
        {
            string calPerUnit;

            // Umechnung, z.B. 500 g Packung hat 500 kcal => 100 kcal/100 g
            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "g", "500");
            Assert.AreEqual(calPerUnit, "100");

            calPerUnit = UnitConvert.GetCaloriePerUnit("",    "g", "500");
            Assert.AreEqual(calPerUnit, "---");

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "",  "500");
            Assert.AreEqual(calPerUnit, "---");

            calPerUnit = UnitConvert.GetCaloriePerUnit("",    "",  "500");
            Assert.AreEqual(calPerUnit, "---");


            // Umechnung, z.B. 500 g Packung hat 500 kcal => 100 kcal/100 g
            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "g", "500");
            Assert.AreEqual(calPerUnit, "100");

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "g", "250");
            Assert.AreEqual(calPerUnit, "50");

            calPerUnit = UnitConvert.GetCaloriePerUnit("200", "g", "100");
            Assert.AreEqual(calPerUnit, "50");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.1", "kg", "30");
            Assert.AreEqual(calPerUnit, "30");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.2", "kg", "30");
            Assert.AreEqual(calPerUnit, "15");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.5", "kg", "300");
            Assert.AreEqual(calPerUnit, "60");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.1", "l", "120");
            Assert.AreEqual(calPerUnit, "120");

            // 1 Liter mit 50 kcal/100 ml
            calPerUnit = UnitConvert.GetCaloriePerUnit("0.5", "l", "120");
            Assert.AreEqual(calPerUnit, "24");

            calPerUnit = UnitConvert.GetCaloriePerUnit("1", "l", "120");
            Assert.AreEqual(calPerUnit, "12");

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "ml", "120");
            Assert.AreEqual(calPerUnit, "24");

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "ml", "0");
            Assert.AreEqual(calPerUnit, "0");

            // Einheit in Großbuchstaben

            // Umechnung, z.B. 500 g Packung hat 500 kcal => 100 kcal/100 g
            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "G", "500");
            Assert.AreEqual(calPerUnit, "100");

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "G", "250");
            Assert.AreEqual(calPerUnit, "50");

            calPerUnit = UnitConvert.GetCaloriePerUnit("200", "G", "100");
            Assert.AreEqual(calPerUnit, "50");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.1", "Kg", "30");
            Assert.AreEqual(calPerUnit, "30");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.2", "KG", "30");
            Assert.AreEqual(calPerUnit, "15");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.5", "kG", "300");
            Assert.AreEqual(calPerUnit, "60");

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.1", "L", "120");
            Assert.AreEqual(calPerUnit, "120");

            // 1 Liter mit 50 kcal/100 ml
            calPerUnit = UnitConvert.GetCaloriePerUnit("0.5", "L", "120");
            Assert.AreEqual(calPerUnit, "24");

            calPerUnit = UnitConvert.GetCaloriePerUnit("1", "L", "120");
            Assert.AreEqual(calPerUnit, "12");

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "Ml", "120");
            Assert.AreEqual(calPerUnit, "24");

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "ML", "0");
            Assert.AreEqual(calPerUnit, "0");
        }


        [TestMethod]
        public void Test_02_GetGesamtCalorie()
        {
            string gesCal;

            // Umrechnung, z.B. 500 g mit 30 kcal/100g => 150 kcal
            gesCal = UnitConvert.GetGesamtCalorie("",    "g", "30");
            Assert.AreEqual(gesCal, "");

            gesCal = UnitConvert.GetGesamtCalorie("500", "",  "30");
            Assert.AreEqual(gesCal, "");

            gesCal = UnitConvert.GetGesamtCalorie("",    "",  "30");
            Assert.AreEqual(gesCal, "");


            // 500 g Packung mit 30 kcal/100 g
            gesCal = UnitConvert.GetGesamtCalorie("500", "g", "30");
            Assert.AreEqual(gesCal, "150");

            gesCal = UnitConvert.GetGesamtCalorie("100", "g", "27");
            Assert.AreEqual(gesCal, "27");

            gesCal = UnitConvert.GetGesamtCalorie("200", "g", "27");
            Assert.AreEqual(gesCal, "54");

            gesCal = UnitConvert.GetGesamtCalorie("0.1", "kg", "30");
            Assert.AreEqual(gesCal, "30");

            gesCal = UnitConvert.GetGesamtCalorie("0.2", "kg", "30");
            Assert.AreEqual(gesCal, "60");

            gesCal = UnitConvert.GetGesamtCalorie("0.5", "kg", "30");
            Assert.AreEqual(gesCal, "150");

            // 1 Liter mit 50 kcal/100 ml
            gesCal = UnitConvert.GetGesamtCalorie("0.5", "l", "120");
            Assert.AreEqual(gesCal, "600");

            gesCal = UnitConvert.GetGesamtCalorie("0.1", "l", "120");
            Assert.AreEqual(gesCal, "120");

            gesCal = UnitConvert.GetGesamtCalorie("1", "l", "120");
            Assert.AreEqual(gesCal, "1200");

            gesCal = UnitConvert.GetGesamtCalorie("500", "ml", "120");
            Assert.AreEqual(gesCal, "600");

            gesCal = UnitConvert.GetGesamtCalorie("", "ml", "120");
            Assert.AreEqual(gesCal, "");

            gesCal = UnitConvert.GetGesamtCalorie("500", "ml", "");
            Assert.AreEqual(gesCal, "");
        }

    }
}
