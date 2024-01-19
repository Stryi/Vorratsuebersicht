using System;
using UnitTestProject;

namespace VueUnitTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            new Tools_UNitTest().Test_01_Date_Today();
            new Tools_UNitTest().Test_01_Date_TodayAt12PM();
            new Tools_UNitTest().Test_01_Date_Yesterday();
            new Tools_UNitTest().Test_01_Date_YesterdayAt12PM();
            new Tools_UNitTest().Test_01_Date_MaiAt12PM();

            new PricePerUnit_UnitTest().Test_01_Liter();
            new PricePerUnit_UnitTest().Test_02_ml();
            new PricePerUnit_UnitTest().Test_03_kg();
            new PricePerUnit_UnitTest().Test_04_g();
            new PricePerUnit_UnitTest().Test_05_dl();
            
            new ShoppingListHelper_UnitTest().Test_01_All();

            new StockStatistic_UnitTest().Test_01_Add_kg_g();
            new StockStatistic_UnitTest().Test_02_Add_ml_cl_l();
            
            new UnitConvert_UnitTest().Test_01_GetCaloriePerUnit();
            new UnitConvert_UnitTest().Test_02_GetGesamtCalorie();
        }
    }
}
