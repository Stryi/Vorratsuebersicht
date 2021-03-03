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
        public static string GetGesamtCalorie(string sizeText, string unit, string caloriePerUnitText)
        {
            decimal factor     = 1;                     // Umrechnungsfaktor zwischen l und ml oder kg und mg
            Int64 caloriePerUnit;
            decimal size;

            Int64.TryParse(caloriePerUnitText, out caloriePerUnit);
            decimal.TryParse(sizeText, 
                NumberStyles.Number, 
                CultureInfo.InvariantCulture, 
                out size);

            if (string.IsNullOrEmpty(caloriePerUnitText))
                return "";

            if (string.IsNullOrEmpty(sizeText))
                return "";

            if ((size == 0) || (caloriePerUnit == 0))
                return "";

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
                return "";

            Int64 calorieGes;
            try
            {
                calorieGes = (Int64)(caloriePerUnit * factor * size);
            }
            catch
            {
                return "";
            }

            return calorieGes.ToString();
        }

        // Umechnung, z.B. 500 g Packung hat 2000 kcal => 150 kcal/100 g
        public static string GetCaloriePerUnit(string sizeText, string unit, string calorieText)
        {
            decimal factor = 1;                     // Umrechnungsfaktor zwischen l und ml oder kg und mg
            decimal calorie = -1;
            decimal size = -1;

            decimal.TryParse(calorieText, out calorie);

            decimal.TryParse(sizeText, 
                NumberStyles.Number, 
                CultureInfo.InvariantCulture, 
                out size);

            if (string.IsNullOrEmpty(calorieText))
                return "";

            if (string.IsNullOrEmpty(sizeText))
                return "---";

            if ((size == -1) || (calorie == -1))
                return "---";

            unit = unit.ToLower();

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
                return "---";

            Int64 calPerUnit = -1;
            try
            {
                calPerUnit = (Int64)(calorie / size / factor);
            }
            catch { }

            return calPerUnit.ToString();
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