using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VueUnitTestProject
{
    using VorratsUebersicht;
    
    internal class Tools_UNitTest
    {
        public void Test_01_Date_Today()
        {
            var date = DateTime.Today;

            var text = Tools.ToHumanText(date);
            Assert.AreEqual(text, "Heute");
        }

        public void Test_01_Date_TodayAt12PM()
        {
            var date = DateTime.Today;
            date = date.AddHours(12);
            var text = Tools.ToHumanText(date);
            Assert.AreEqual(text, "Heute um 12:00");
        }

        public void Test_01_Date_Yesterday()
        {
            var date = DateTime.Today.AddDays(-1);

            var text = Tools.ToHumanText(date);
            Assert.AreEqual(text, "Gestern");
        }

        public void Test_01_Date_YesterdayAt12PM()
        {
            var date = DateTime.Today.AddDays(-1);
            date = date.AddHours(12);
            var text = Tools.ToHumanText(date);
            Assert.AreEqual(text, "Gestern um 12:00");
        }

        public void Test_01_Date_MaiAt12PM()
        {
            var date = new DateTime(2024, 5, 2);
            date = date.AddHours(12);
            var text = Tools.ToHumanText(date);
            Assert.AreEqual(text, "02.05.2024 um 12:00");
        }

    }
}
