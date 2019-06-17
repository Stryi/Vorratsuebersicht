using System;
using System.Globalization;
using System.Linq;

namespace VorratsUebersicht
{
    public class QuantityAndUnit
    {
        public decimal Quantity;
        public string Unit;

        public QuantityAndUnit(decimal quantity, string unit)
        {
            this.Quantity = quantity;
            this.Unit     = unit;
        }

        public static QuantityAndUnit Parse(string quantityAndUnit)
        {
            if (string.IsNullOrEmpty(quantityAndUnit))
                return null;

            QuantityAndUnit value;
            quantityAndUnit = quantityAndUnit.Trim();

            value = QuantityAndUnit.Parse(quantityAndUnit, " kg"); if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, " g");  if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, " gr"); if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, " l");  if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, " ml"); if (value != null) return value;

            value = QuantityAndUnit.Parse(quantityAndUnit, "kg"); if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, "g");  if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, "gr"); if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, "l");  if (value != null) return value;
            value = QuantityAndUnit.Parse(quantityAndUnit, "ml"); if (value != null) return value;

            return null;
        }

        public static QuantityAndUnit Parse(string quantityAndUnit, string unit)
        {
            // kg, g, l, ml
            if (!quantityAndUnit.EndsWith(unit, StringComparison.InvariantCultureIgnoreCase))
                return null;

            string quantityText = quantityAndUnit.Substring(0, quantityAndUnit.Length - unit.Length);
            unit = unit.Trim().ToLower();

            quantityText = quantityText.Replace(",", ".");
            decimal quantity;

            if (!decimal.TryParse(quantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out quantity))
                return null;

            return new QuantityAndUnit(quantity, unit);
        }
    }
}