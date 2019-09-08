using System;
using System.Diagnostics;

namespace VorratsUebersicht
{
    /// <summary>
    /// Lagerbestand mit Anzahl
    /// </summary>
    [DebuggerDisplay("{Name} {WarningLevel}, {Quantity}")]
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
        public int Quantity {get; set;}
        public int Calorie {get; set;}
        public DateTime? BestBefore {get; set;}
        public string StorageName { get; set; }
        public int? MinQuantity { get; set; }
        public int? PrefQuantity { get; set; }
        public decimal Price { get; set; }

        public int QuantityDiff;

		/// <summary>
		/// Ablaufdatum �berschritten oder nur Warnung f�r Ablaufdatum
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
		/// Warnung f�r Ablaufdatum f�r...
		/// </summary>
		public string BestBeforeInfoText { get; set; }

		/// <summary>
		/// Ablaufdatum �berschritten f�r...
		/// </summary>
		public string BestBeforeWarningText { get; set; }
    }
}