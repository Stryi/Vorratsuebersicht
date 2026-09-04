package de.stryi.vorratsuebersicht.database.Records

import android.database.Cursor
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.database.getDoubleOrNull
import de.stryi.vorratsuebersicht.database.getIntOrNull
import de.stryi.vorratsuebersicht.database.getStringOrNull
import de.stryi.vorratsuebersicht.tools.PricePerUnit
import de.stryi.vorratsuebersicht.tools.Tools
import kotlin.collections.List

class ArticleInfo {
    var articleId : Int = 0
    var name : String? = null
    var manufacturer: String? = null
    var category: String? = null
    var subCategory: String? = null
    var durableInfinity: Boolean = false
    var warnInDays: Int? = null
    var size: Double? = null
    var unit: String? = null
    var calorie: Int? = null
    var storageName: String? = null
    var minQuantity: Int? = null
    var prefQuantity: Int? = null
    var price: Double? = null

    var storageQuantity   : Double = 0.0
    var shoppingQuantity: Double = -1.00

    val heading: String
        get() {
            return this.name ?: ""
        }

    // Detailangaben zum Artikel in der Lagerbestandsliste
    val storageInfo: String
        get() {
            val info = StringBuilder()

            // Anzahl
            val quantityText = Tools.formatNumber(this.storageQuantity)
            info.append("Anzahl: $quantityText")

            // Inhalt/Größe, Menge
            if (this.size != null && this.size != 0.0)
            {
                info.append(", Inhalt/Größe: %.0f %s".format(this.size, this.unit))

                if (this.storageQuantity != 1.0 && this.size != 1.0)
                {
                    info.appendLine()
                    val menge = Tools.formatNumber(this.size!! * this.storageQuantity)

                    info.append("Menge: $menge ${this.unit}")
                }
            }


            if (this.minQuantity != null && this.minQuantity != 0)
            {
                val value = Tools.formatNumber(this.minQuantity)
                info.appendLine()
                info.append("Mindestmenge: $value")
            }

            if (this.prefQuantity != null && this.prefQuantity != 0)
            {
                val value = Tools.formatNumber(this.prefQuantity)
                info.appendLine()
                info.append("Bevorzugte Menge: $value")
            }

            if (!this.storageName.isNullOrEmpty() )
            {
                info.appendLine()
                info.append("Lagerort: $storageName")
            }

            if (this.price != null && this.price != 0.0)
            {
                info.appendLine()
                info.append("Preis: %.2f".format(this.price))
                val pricePerUnit = PricePerUnit.calculate(this.price, this.size, this.unit)
                if (pricePerUnit.isNotBlank())
                {
                    info.append(" ($pricePerUnit)")
                }
                if (this.storageQuantity != 1.0)
                {
                    val menge = this.storageQuantity * this.price!!
                    info.append(" -> Wert: %.2f".format(menge))
                }
            }
            return info.toString().trimEnd()
        }

    val isOnShoppingList: Boolean
        get() {
            return this.shoppingQuantity >= 0.0 // Menge 0 bedeutet: Auf Einkaufszettel, aber ohne Menge.
        }

    val shoppingQuantityText: String
        get() {
            if (!isOnShoppingList)
                return ""

            if (this.shoppingQuantity == 0.00)
                return ""

            return Tools.formatNumber(this.shoppingQuantity)
        }

    var storageArticleListCache: List<StorageItem>? = null

    fun getBestBeforeItemQuantity(storageNameFilter: String?, withoutStorage: Boolean): List<StorageItem>
    {
        if (this.storageArticleListCache != null)
            return this.storageArticleListCache!!

        this.storageArticleListCache = Database.getBestBeforeItemQuantity(
            this.articleId,
            storageNameFilter,
            withoutStorage
        )
        return this.storageArticleListCache!!
    }

    fun shouldBeRemoved(filterExpiryDate: String?): Boolean
    {
        if (this.storageArticleListCache == null)
            return false

        var toRemove = true

        for (item in this.storageArticleListCache!!)
        {
            if (filterExpiryDate == "ExpiryDateOnly" && item.warningLevel == 0)
                continue

            if (filterExpiryDate == "NearExpiryDateOnly" && item.warningLevel != 1)
                continue

            if (filterExpiryDate == "WithExpiryDateOnly" && item.warningLevel != 2)
                continue

            toRemove = false
        }

        return  toRemove
    }

    val storageArticleList: List<StorageItem>
        get()
        {
            if (this.storageArticleListCache == null) {
                this.storageArticleListCache = Database.getBestBeforeItemQuantity(this.articleId) // Gruppiert nach BestBefore!!!
            }

            return this.storageArticleListCache!!
        }

    companion object {
        fun fromCursor(cursor: Cursor): ArticleInfo {
            val storageItem = ArticleInfo()
            storageItem.articleId       = cursor.getIntOrNull("ArticleId") ?: 0
            storageItem.name            = cursor.getStringOrNull("Name")
            storageItem.manufacturer    = cursor.getStringOrNull("Manufacturer")
            storageItem.category        = cursor.getStringOrNull("Category")
            storageItem.subCategory     = cursor.getStringOrNull("SubCategory")
            storageItem.durableInfinity = cursor.getIntOrNull("DurableInfinity") == 1
            storageItem.warnInDays      = cursor.getIntOrNull("WarnInDays")
            storageItem.size            = cursor.getDoubleOrNull("Size")
            storageItem.unit            = cursor.getStringOrNull("Unit")
            storageItem.calorie         = cursor.getIntOrNull("Calorie")
            storageItem.minQuantity     = cursor.getIntOrNull("MinQuantity")
            storageItem.prefQuantity    = cursor.getIntOrNull("PrefQuantity")
            storageItem.price           = cursor.getDoubleOrNull("Price")
            storageItem.storageName     = cursor.getStringOrNull("StorageName")

            // Angaben aus abhängigen Tabellen
            storageItem.storageQuantity  = cursor.getDoubleOrNull("StorageQuantity")  ?: 0.0
            storageItem.shoppingQuantity = cursor.getDoubleOrNull("ShoppingQuantity") ?: -1.00

            return storageItem
        }
    }
}