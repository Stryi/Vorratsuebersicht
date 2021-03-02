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
            
            string istGewicht  = statisticEssen.GetStatistic();
            string sollGewicht = "2 Positionen, Anzahl: 4 Stück, Menge: 3,5 kg";

            Assert.AreEqual(istGewicht, sollGewicht);
        }

        [TestMethod]
        public void Test_02_Add_ml_cl_l()
        {
            // --------------------------------------------------------------------------------
            // Zusammenfassung ml, cl und l
            // --------------------------------------------------------------------------------

            StorageItemQuantityResult trinken1 = new StorageItemQuantityResult()
            {
                Unit = "ml",
                Size = 50,
                Quantity = 1 // Stück
            };

            StorageItemQuantityResult trinken2 = new StorageItemQuantityResult()
            {
                Unit = "l",
                Size = 0.75m,
                Quantity = 10 // Stück
            };

            StorageItemQuantityResult trinken3 = new StorageItemQuantityResult()
            {
                Unit = "cl",
                Size = 70,
                Quantity = 1 // Stück
            };

            StockStatistic statisticTrinken = new StockStatistic();
            statisticTrinken.AddStorageItem(trinken1);

            string trinkenIst  = statisticTrinken.GetStatistic();
            string trinkenSoll = "1 Position, Anzahl: 1 Stück, Menge: 50 ml";
            Assert.AreEqual(trinkenSoll, trinkenIst);

            statisticTrinken.AddStorageItem(trinken2);

            trinkenIst  = statisticTrinken.GetStatistic();
            trinkenSoll = "2 Positionen, Anzahl: 11 Stück, Menge: 7,55 l";
            Assert.AreEqual(trinkenSoll, trinkenIst);

            statisticTrinken.AddStorageItem(trinken3);

            trinkenIst = statisticTrinken.GetStatistic();
            trinkenSoll = "3 Positionen, Anzahl: 12 Stück, Menge: 8,25 l";
            Assert.AreEqual(trinkenSoll, trinkenIst);
        }
    }
}
