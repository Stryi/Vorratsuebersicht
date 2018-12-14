using System;
using System.Diagnostics;
using System.Globalization;

namespace VorratsUebersicht
{
    public class UnitConvert
    {
        // https://dotnet-snippets.de/snippet/byte-groessenangaben-als-string-formatieren-kb-mb-gb/1304
        public static string ToFuzzyByteString(long bytes)
        {                    
            double s = bytes; 
            string[] format = new string[]
                  {
                      "{0} bytes", "{0} KB",  
                      "{0} MB", "{0} GB", "{0} TB", "{0} PB", "{0} EB"
                  };

            int i = 0;

            while (i < format.Length && s >= 1024)              
            {                                     
                s = (long) (100*s/1024)/100.0;  
                i++;            
            }                     
            return string.Format(format[i], s);  
        }

        // Umrechnung, z.B. 500 g mit 30 kcal/100g => 150 kcal
        public static int GetGesamtCalorie(string sizeText, string unit, string caloriePerUnitText)
        {
            decimal factor     = 1;                     // Umrechnungsfaktor zwischen l und ml oder kg und mg
            int caloriePerUnit = -1;
            decimal size       = 0;

            int.TryParse(caloriePerUnitText, out caloriePerUnit);
            decimal.TryParse(sizeText, 
                NumberStyles.Number, 
                CultureInfo.InvariantCulture, 
                out size);

            //if (string.IsNullOrEmpty(caloriePerUnitText))
            //    return - 1;

            if ((size == 0) || (caloriePerUnit == -1))
                return - 1;

            string unitPerX = GetConvertUnit(unit);
            if ((unit == "kg") && (unitPerX == "g"))
            {
                factor = 10M;
            }

            if ((unit == "g") && (unitPerX == "g"))
            {
                factor = 0.01M;
            }

            if ((unit == "l") && (unitPerX == "ml"))
            {
                factor = 10M;
            }

            if ((unit == "ml") && (unitPerX == "ml"))
            {
                factor = 0.01M;
            }

            if (string.IsNullOrEmpty(unitPerX))
                return -1;

            int calorieGes = (int)(caloriePerUnit * factor * size);

            return calorieGes;
        }

        // Umechnung, z.B. 500 g Packung hat 2000 kcal => 150 kcal/100 g
        public static Int64 GetCaloriePerUnit(string sizeText, string unit, string calorieText)
        {
            decimal factor = 1;                     // Umrechnungsfaktor zwischen l und ml oder kg und mg
            decimal calorie = -1;
            decimal size = -1;

            decimal.TryParse(calorieText, out calorie);

            decimal.TryParse(sizeText, 
                NumberStyles.Number, 
                CultureInfo.InvariantCulture, 
                out size);

            //if (string.IsNullOrEmpty(calorieText))
            //    return - 1;

            if ((size == -1) || (calorie == -1))
                return - 1;

            string unitPerX = GetConvertUnit(unit);
            if ((unit == "kg") && (unitPerX == "g"))
            {
                factor = 10M;
            }

            if ((unit == "g") && (unitPerX == "g"))
            {
                factor = 0.01M;
            }

            if ((unit == "l") && (unitPerX == "ml"))
            {
                factor = 10M;
            }

            if ((unit == "ml") && (unitPerX == "ml"))
            {
                unitPerX = "ml";
                factor = 0.01M;
            }

            if (string.IsNullOrEmpty(unitPerX))
                return -1;

            Int64 calPerUnit = -1;
            try
            {
                calPerUnit = (Int64)(calorie / size / factor);
            }
            catch { }

            return calPerUnit;
        }

        internal void UnitTest_GetGesamtCalorie()
        {
            int gesCal;

            gesCal = UnitConvert.GetGesamtCalorie("500", "g", "30");
            Debug.Assert(gesCal == 150);


            // 500 g Packung mit 30 kcal/100 g
            gesCal = UnitConvert.GetGesamtCalorie("500", "g", "30");
            Debug.Assert(gesCal == 150);

            gesCal = UnitConvert.GetGesamtCalorie("100", "g", "27");
            Debug.Assert(gesCal == 27);

            gesCal = UnitConvert.GetGesamtCalorie("200", "g", "27");
            Debug.Assert(gesCal == 2*27);

            gesCal = UnitConvert.GetGesamtCalorie("0.1", "kg", "30");
            Debug.Assert(gesCal == 30);

            gesCal = UnitConvert.GetGesamtCalorie("0.2", "kg", "30");
            Debug.Assert(gesCal == 60);

            gesCal = UnitConvert.GetGesamtCalorie("0.5", "kg", "30");
            Debug.Assert(gesCal == 150);

            // 1 Liter mit 50 kcal/100 ml
            gesCal = UnitConvert.GetGesamtCalorie("0.5", "l", "120");
            Debug.Assert(gesCal == 600);

            gesCal = UnitConvert.GetGesamtCalorie("0.1", "l", "120");
            Debug.Assert(gesCal == 120);

            gesCal = UnitConvert.GetGesamtCalorie("1", "l", "120");
            Debug.Assert(gesCal == 1200);

            gesCal = UnitConvert.GetGesamtCalorie("500", "ml", "120");
            Debug.Assert(gesCal == 600);

            gesCal = UnitConvert.GetGesamtCalorie("", "ml", "120");
            Debug.Assert(gesCal == -1);

            gesCal = UnitConvert.GetGesamtCalorie("500", "ml", "");
            Debug.Assert(gesCal == -1);

        }

        internal void UnitTest_GetCaloriePerUnit()
        {
            Int64 calPerUnit;

            // Umechnung, z.B. 500 g Packung hat 500 kcal => 100 kcal/100 g
            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "g", "500");
            Debug.Assert(calPerUnit == 100);

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "g", "250");
            Debug.Assert(calPerUnit == 50);

            calPerUnit = UnitConvert.GetCaloriePerUnit("200", "g", "100");
            Debug.Assert(calPerUnit ==50);

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.1", "kg", "30");
            Debug.Assert(calPerUnit == 30);

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.2", "kg", "30");
            Debug.Assert(calPerUnit == 15);

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.5", "kg", "300");
            Debug.Assert(calPerUnit == 60);

            calPerUnit = UnitConvert.GetCaloriePerUnit("0.1", "l", "120");
            Debug.Assert(calPerUnit == 120);

            // 1 Liter mit 50 kcal/100 ml
            calPerUnit = UnitConvert.GetCaloriePerUnit("0.5", "l", "120");
            Debug.Assert(calPerUnit == 24);

            calPerUnit = UnitConvert.GetCaloriePerUnit("1", "l", "120");
            Debug.Assert(calPerUnit == 12);

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "ml", "120");
            Debug.Assert(calPerUnit == 24);

            calPerUnit = UnitConvert.GetCaloriePerUnit("500", "ml", "0");
            Debug.Assert(calPerUnit == 0);
        }

        internal static string GetConvertUnit(string unit)
        {
            if (unit == "kg")
            {
                return "g";
            }

            if (unit == "g")
            {
                return "g";
            }

            if (unit == "l")
            {
                return "ml";
            }
            if (unit == "ml")
            {
                return "ml";
            }
            return string.Empty;
        }
    }
}