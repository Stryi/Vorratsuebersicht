using System;
using System.Globalization;

namespace VorratsUebersicht
{
    internal class PricePerUnit
    {
        public static string Calculate(string priceText, string sizeText, string unitText)
        {
            decimal price;
            decimal size;

            if (string.IsNullOrEmpty(unitText))
                return "---";

            Decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out price);
            Decimal.TryParse(sizeText,  NumberStyles.Any, CultureInfo.InvariantCulture, out size);

            if (price == 0)
                return "---";

            if (size == 0)
                return "---";

            if (string.IsNullOrEmpty(unitText))
                return "---";

            return PricePerUnit.Calculate(price, size, unitText);
        }

        public static string Calculate(decimal? price, decimal? size, string unitText)
        {
            if ((price == null) || (size == null))
                return string.Empty;

            if (size.Value == 0)
                return string.Empty;

            string  pricePerUnit   = string.Empty;
            decimal calculatedSize = -1;
            string  calculatedUnit = string.Empty;
            decimal pricePerSize   = 0;

            if (unitText == "l")
            {
                if (size.Value == 1)
                    return string.Empty;

                if (size.Value <= 0.25m)
                {
                    // Umrechnen in 100 ml
                    calculatedSize = 100;
                    calculatedUnit = "ml";

                    pricePerSize = (price.Value / size.Value) / 10;
                }
                else
                {
                    // Umrechnen in 1 Liter
                    calculatedSize = 1;
                    calculatedUnit = "Liter";

                    pricePerSize = (price.Value / size.Value) ;
                }
            }

            if (unitText == "cl")
            {
                if (size.Value <= 25m)
                {
                    calculatedSize = 100;
                    calculatedUnit = "ml";

                    pricePerSize = (price.Value / size.Value) * 10;
                }
                else
                {
                    calculatedSize = 1;
                    calculatedUnit = "Liter";

                    pricePerSize = (price.Value / size.Value) * 100;
                }
            }

            if (unitText == "ml")
            {
                if (size == 100)
                    return string.Empty;

                if (size.Value <= 250m)
                {
                    calculatedSize = 100;
                    calculatedUnit = "ml";

                    pricePerSize = (price.Value / size.Value) * 100;
                }
                else
                {
                    calculatedSize = 1;
                    calculatedUnit = "Liter";

                    pricePerSize = (price.Value / size.Value) * 1000;
                }
            }


            if (unitText == "kg")
            {
                if (size == 1)
                    return string.Empty;

                if (size.Value <= 0.250m)
                {
                    calculatedSize = 100;
                    calculatedUnit = "g";

                    pricePerSize = (price.Value / size.Value) / 10;
                }
                else
                {
                    calculatedSize = 1;
                    calculatedUnit = "kg";

                    pricePerSize = (price.Value / size.Value) / 1;
                }
            }

            if (unitText == "g")
            {
                if (size == 100)
                    return string.Empty;

                if (size.Value <= 250m)
                {
                    calculatedSize = 100;
                    calculatedUnit = "g";

                    pricePerSize = (price.Value / size.Value) * 100;
                }
                else
                {
                    calculatedSize = 1;
                    calculatedUnit = "kg";

                    pricePerSize = (price.Value / size.Value) * 1000;
                }
            }


            if (calculatedSize != -1)
            {
                pricePerUnit = string.Format(CultureInfo.CurrentUICulture,
                    "{0} {1} = {2:N2}",
                    calculatedSize,
                    calculatedUnit,
                    pricePerSize);
            }

            return pricePerUnit;
        }

        /*
        public void UnitTest()
        {
            Trace.Assert(PricePerUnit.Calculate("2.99", "1",   "l") == "0.30 pro 100 ml");
            Trace.Assert(PricePerUnit.Calculate("2.99", "0.7", "l") == "0.43 pro 100 ml");

            Trace.Assert(PricePerUnit.Calculate("2", "100",   "ml") == "2.00 pro 100 ml");
            Trace.Assert(PricePerUnit.Calculate("2", "500",   "ml") == "0.40 pro 100 ml");

            Trace.Assert(PricePerUnit.Calculate("5.99", "1",   "kg") == "0.60 pro 100 g");
            Trace.Assert(PricePerUnit.Calculate("5.99", "0.5", "kg") == "1.20 pro 100 g");

            Trace.Assert(PricePerUnit.Calculate("2.99", "100", "g") == "2.99 pro 100 g");
            Trace.Assert(PricePerUnit.Calculate("2.99", "200", "g") == "1.50 pro 100 g");

        }
        */
    }
}