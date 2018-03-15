using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    /// <summary>
    /// Lagerbestand mit Anzahl
    /// </summary>
    public class StorageItemQuantityResult
    {
        public int ArticleId {get; set;}
        public int StorageItemId {get; set;}
        public string EANCode {get; set;}
        public string Name {get; set;}
        public string Manufacturer {get; set;}
        public string Category {get; set;}
        public bool DurableInfinity {get; set;}
        public int WarnInDays {get; set;}
        public decimal Size {get; set; }
        public string Unit {get; set;}
        public byte[] Image {get; set;}
        public byte[] ImageLarge {get; set;}
        public int Quantity {get; set;}
        public DateTime? BestBefore {get; set;}

        public int QuantityDiff;

		/// <summary>
		/// Ablaufdatum überschritten oder nur Warnung für Ablaufdatum
		/// </summary>
        public int WarningLevel
        {
            get
            {
				if (this.DurableInfinity)		// Haltbar unendlich
					return 0;

                if (this.BestBefore == null)
                    return 0;

                if (!this.BestBefore.HasValue)
                    return 0;

                if (this.BestBefore == DateTime.MinValue)
                    return 0;

                if (this.BestBefore < DateTime.Today)
                    return 2;

                if (this.BestBefore.Value.AddDays(-this.WarnInDays) < DateTime.Today)
                    return 1;

                return 0;
            }
        }

		/// <summary>
		/// Warnung für Ablaufdatum für...
		/// </summary>
		public string BestBeforeInfoText { get; set; }

		/// <summary>
		/// Ablaufdatum überschritten für...
		/// </summary>
		public string BestBeforeWarningText { get; set; }
    }
}