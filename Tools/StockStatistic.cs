using System;
using System.Globalization;
using System.Collections.Generic;

namespace VorratsUebersicht
{
    public class StockStatistic
    {
        int count = 0;
        decimal quantity = 0;
        Dictionary<string, decimal> sum_menge = new Dictionary<string, decimal>();
        decimal sum_warnung = 0;
        decimal sum_abgelaufen = 0;
        decimal sum_kcal = 0;
        decimal sum_price = 0;

        internal void AddWarningLevel1(decimal quantity)
        {
            sum_warnung += quantity;
        }

        internal void AddWarningLevel2(decimal quantity)
        {
            sum_abgelaufen += quantity;
        }
        
        private void AddUnitQuantity(string unit, decimal size, decimal quantity)
        {
            if (string.IsNullOrEmpty(unit))
                unit = string.Empty;

            if (!this.sum_menge.ContainsKey(unit))
            {
                this.sum_menge.Add(unit, size * quantity);
            }
            else
            {
                this.sum_menge[unit] += size * quantity;
            }
        }

        private void AddCalorie(decimal quantity, int calorie)
        {
            sum_kcal += quantity * calorie;
        }

        internal void AddStorageItem(StorageItemQuantityResult storegeItem)
        {
            this.count ++;
            this.quantity += storegeItem.Quantity;
            this.AddUnitQuantity(storegeItem.Unit, storegeItem.Size, storegeItem.Quantity);
            this.AddCalorie(storegeItem.Quantity, storegeItem.Calorie);
            this.AddCosts(storegeItem.Quantity, storegeItem.Price);
        }

        private void AddCosts(decimal quantity, decimal? price)
        {
            if (!price.HasValue)
                return;

            this.sum_price += quantity * price.Value;
        }

        internal string GetStatistic()
        {
            string status;
            if (this.count == 1)
                status = string.Format("{0:n0} Position", this.count);
            else
                status = string.Format("{0:n0} Positionen", this.count);

            status += string.Format(CultureInfo.CurrentUICulture, ", Anzahl: {0:#,0.######} Stück", this.quantity);

            if (this.sum_menge.Count > 0)
            {
                string mengeListe = string.Empty;

                if (this.sum_menge.ContainsKey("ml") && this.sum_menge.ContainsKey("l"))
                {
                    decimal ml = this.sum_menge["ml"];
                    decimal l  = this.sum_menge["l"];

                    decimal liter = l + ml / 1000;

                    this.sum_menge.Remove("ml");
                    this.sum_menge["l"] = liter;
                }

                if (this.sum_menge.ContainsKey("cl") && this.sum_menge.ContainsKey("l"))
                {
                    decimal cl = this.sum_menge["cl"];
                    decimal l  = this.sum_menge["l"];

                    decimal liter = l + cl / 100;

                    this.sum_menge.Remove("cl");
                    this.sum_menge["l"] = liter;
                }

                if (this.sum_menge.ContainsKey("g") && this.sum_menge.ContainsKey("kg"))
                {
                    decimal g   = this.sum_menge["g"];
                    decimal kg  = this.sum_menge["kg"];

                    decimal gewicht = kg + g / 1000;

                    this.sum_menge.Remove("g");
                    this.sum_menge["kg"] = gewicht;
                }

                foreach(var menge in this.sum_menge)
                {
                    if (menge.Value == 0)
                        continue;

                    if (!string.IsNullOrEmpty(mengeListe))
                        mengeListe += ", ";
                    mengeListe += string.Format(CultureInfo.CurrentUICulture, "{0:#,0.######} {1}", menge.Value, menge.Key);
                }

                if (!string.IsNullOrEmpty(mengeListe))
                {
                    status += string.Format(", Menge: {0:n0}", mengeListe);
                }
            }

            if (sum_kcal        > 0) status += string.Format(CultureInfo.CurrentUICulture, ", Kalorien: {0:n0}",   sum_kcal);
            if (sum_price       > 0) status += string.Format(CultureInfo.CurrentUICulture, ", Wert: {0:n2} {1}",   sum_price, CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
            if (sum_warnung     > 0) status += string.Format(CultureInfo.CurrentUICulture, ", {0:#,0.######} Warnung(en)", sum_warnung);
            if (sum_abgelaufen  > 0) status += string.Format(CultureInfo.CurrentUICulture, ", {0:#,0.######} Abgelaufen",  sum_abgelaufen);
            
            return status;
        }
    }
}