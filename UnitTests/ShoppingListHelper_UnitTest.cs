using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VorratsUebersicht;

namespace UnitTestProject
{
    [TestClass]
    public class ShoppingListHelper_UnitTest
    {
        [TestMethod]
        public void Test_01_All()
        {
            
            //                                               MinQuantity  PrefQuantity IsQuantity ToBuy
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           0,           4),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           0,           3),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           0,           2),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           0,           1),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           0,           0),         0);

            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           0,           6),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           0,           5),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           0,           4),         1);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           0,           3),         2);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           0,           2),         3);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           0,           1),         4);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           0,           0),         5);

            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           9),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           8),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           7),         1);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           6),         2);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           5),         3);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           4),         4);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           3),         5);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           2),         6);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           1),         7);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(0,           8,           0),         8);

            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           9),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           8),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           7),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           6),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           5),         0);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           4),         4);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           3),         5);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           2),         6);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           1),         7);
            Assert.AreEqual(ShoppingListHelper.GetToBuyQuantity(5,           8,           0),         8);
        }
    }
}
