using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VorratsUebersicht;

namespace UnitTestProject
{
    [TestClass]
    public class StockStatistic_UnitTest
    {
        [TestMethod]
        public void Test_01_Add_kg_g()
        {
            // --------------------------------------------------------------------------------
            // Zusammenfassung g und kg
            // --------------------------------------------------------------------------------

            StorageItemQuantityResult essen1 = new StorageItemQuantityResult();
            essen1.Unit = "kg";
            essen1.Size = 2;
            essen1.Quantity = 1; // Stück

            StorageItemQuantityResult essen2 = new StorageItemQuantityResult();
            essen2.Unit = "g";
            essen2.Size = 500;
            essen2.Quantity = 3; // Stück

            StockStatistic statisticEssen = new StockStatistic();
            statisticEssen.AddStorageItem(essen1);
            statisticEssen.AddStorageItem(essen2);
            
            statisticEssen.ConvertUnits();

            Assert.AreEqual(statisticEssen.count, 2);       // Positionen
            Assert.AreEqual(statisticEssen.quantity, 4);    // Anzahl
            decimal menge = statisticEssen.sum_menge["kg"];
            Assert.AreEqual(menge, 3.5m);                   // Menge: 3,5 kg
        }

        [TestMethod]
        public void Test_02_Add_ml_cl_l()
        {
            // --------------------------------------------------------------------------------
            // Zusammenfassung ml, cl und l
            // --------------------------------------------------------------------------------

            StockStatistic statisticTrinken = new StockStatistic();
            
            statisticTrinken.AddStorageItem(new StorageItemQuantityResult()
                {
                    Unit = "ml",
                    Size = 50,
                    Quantity = 1 // Stück
                });
            
            statisticTrinken.ConvertUnits();

            Assert.AreEqual(statisticTrinken.count,     1);     // Positionen
            Assert.AreEqual(statisticTrinken.quantity,  1);     // Anzahl
            decimal menge = statisticTrinken.sum_menge["ml"];
            Assert.AreEqual(menge, 50m);                        // Menge: 50 ml


            statisticTrinken.AddStorageItem(new StorageItemQuantityResult()
                {
                    Unit = "l",
                    Size = 0.75m,
                    Quantity = 10 // Stück
                });

            statisticTrinken.ConvertUnits();

            Assert.AreEqual(statisticTrinken.count,     2);     // Positionen
            Assert.AreEqual(statisticTrinken.quantity, 11);     // Anzahl
            decimal liter = statisticTrinken.sum_menge["l"];
            Assert.AreEqual(liter, 7.55m);                      // Menge: 7.55 l

            statisticTrinken.AddStorageItem(new StorageItemQuantityResult()
                {
                    Unit = "cl",
                    Size = 70,
                    Quantity = 1 // Stück
                });

            statisticTrinken.ConvertUnits();

            Assert.AreEqual(statisticTrinken.count,     3);     // Positionen
            Assert.AreEqual(statisticTrinken.quantity, 12);     // Anzahl
            liter = statisticTrinken.sum_menge["l"];
            Assert.AreEqual(liter, 8.25m);                      // Menge: 7.55 l
        }
    }
}
