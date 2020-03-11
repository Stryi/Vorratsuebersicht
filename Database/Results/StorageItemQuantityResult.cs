using System;
using System.Diagnostics;

namespace VorratsUebersicht
{
    using SQLite;

    /// <summary>
    /// Lagerbestand mit Anzahl und j�ngstem Ablaufdatum
    /// </summary>
    [DebuggerDisplay("{Name}, Menge: {Quantity}, Warnung: {WarningLevel}, Ablaufdatum: {BestBefore_DebuggerDisplay}")]
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
        public string ArticleStorageName {get; set;}
        public decimal Quantity {get; set;}
        public int Calorie {get; set;}
        public DateTime? BestBefore {get; set;}
        public string StorageName { get; set; }
        public int? MinQuantity { get; set; }
        public int? PrefQuantity { get; set; }
        public decimal? Price { get; set; }

        public decimal? ShoppingListQuantity { get; set; }

        public bool IsChanged;

		/// <summary>
		/// Ablaufdatum �berschritten oder nur Warnung f�r Ablaufdatum
		/// </summary>
        [Ignore]
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
		/// Info f�r Mengen und Ablaufdatum...
		/// </summary>
        [Ignore]
		public string BestBeforeInfoText { get; set; }

		/// <summary>
		/// Warnung f�r Mengen und Ablaufdatum f�r...
		/// </summary>
        [Ignore]
		public string BestBeforeWarningText { get; set; }

		/// <summary>
		/// Menge mit Ablaufdatum �berschritten f�r...
		/// </summary>
        [Ignore]
		public string BestBeforeErrorText { get; set; }

        /// <summary>
        /// Damit beim Debuggen das DateTime? angezeigt wird.
        /// </summary>
        [Ignore]
        public string BestBefore_DebuggerDisplay
        {
            get 
            {
                if (this.BestBefore == null)
                    return string.Empty;

                if (this.BestBefore.Value == null)
                    return string.Empty;

                return this.BestBefore.Value.ToShortDateString();
            }
        }
    }
}