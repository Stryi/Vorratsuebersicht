package de.stryi.vorratsuebersicht.database.Records

import android.database.Cursor
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.database.getDoubleOrNull
import de.stryi.vorratsuebersicht.database.getIntOrNull
import de.stryi.vorratsuebersicht.database.getStringOrNull
import de.stryi.vorratsuebersicht.tools.PricePerUnit
import de.stryi.vorratsuebersicht.tools.Tools
import de.stryi.vorratsuebersicht.tools.UnitConvert

class Article {
    var articleId: Int = 0
    var name: String? = null
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
    var supermarket: String? = null
    var eanCode: String? = null
    var notes: String? = null

    val heading: String
        get() {
            return this.name ?: ""
        }

    val articleInfo: String
        get() {
            val info = StringBuilder()

            // Hersteller
            this.manufacturer?.takeIf { it.isNotBlank() }?.let {
                info.appendLine("Hersteller: $it")
            }

            // Kategorie / Unterkategorie
            val categoryText = buildString{
                if (!category.isNullOrBlank()) append(category)
                if (!subCategory.isNullOrBlank()) {
                    if (isNotEmpty()) append(" / ")
                    append(subCategory)
                }
            }
            if (categoryText.isNotBlank()) {
                info.appendLine("Kategorie: $categoryText")
            }

            // Supermarkt
            supermarket?.takeIf { it.isNotBlank() }?.let {
                info.appendLine("Einkaufsmarkt: $it")
            }

            // Lagerort
            storageName?.takeIf { it.isNotBlank() }?.let {
                info.appendLine("Standard Lagerort: $it")
            }

            // Warnung in Tagen
            if (!durableInfinity && warnInDays != null) {
                info.appendLine("Warnen in Tagen vor Ablauf: $warnInDays")
            }

            // Preis
            info.append("Preis: ")
            if (price != null) {
                var priceText = "%.2f".format(price)

                val pricePerUnit = PricePerUnit.calculate(price, size, unit)
                if (pricePerUnit.isNotBlank()) {
                    priceText += " ($pricePerUnit)"
                }
                info.appendLine(priceText)
            }
            else
            {
                info.appendLine("-")
            }

            // Größe
            if (size != null) {
                val sizeText = "%.0f".format(size)
                info.appendLine("Inhalt/Größe: $sizeText ${unit ?: ""}")
            }

            // Kalorien
            info.append("Kalorien: ")
            if (calorie != null) {
                var calorieText = "%.0f".format(calorie?.toDouble() ?: 0.0)

                if (calorie != 0) {
                    val unitPerX = UnitConvert.getConvertUnit(unit)
                    val calPerUnit = UnitConvert.getCaloriePerUnit(
                        size?.toString(),
                        unit,
                        calorie?.toString()
                    )
                    if (calPerUnit != "---") {
                        calorieText += " (100 $unitPerX = $calPerUnit)"
                    }
                }
                info.appendLine(calorieText)
            }
            else
            {
                info.appendLine("-")
            }

            return info.toString().trimEnd()
        }

    val notesText: String
        get() {
            val info = StringBuilder()

            // Hersteller
            this.notes?.takeIf { it.isNotBlank() }?.let {
                info.appendLine("Notizen: $it")
            }

            return info.toString().trimEnd()
        }

    var shoppingQuantityCache: Double? = null

    val isOnShoppingList: Boolean
        get() {
            if (shoppingQuantityCache == null)
            {
                shoppingQuantityCache = Database.getShoppingListQuantiy(
                    this.articleId,
                    (-1).toDouble()
                )
            }
            return shoppingQuantityCache!! >= 0.0  // Menge 0 bedeutet: Auf EInkaufszettel, aber ohne Menge.
        }

    val shoppingQuantityText: String
        get() {
            if (!isOnShoppingList)
                return ""

            if (shoppingQuantityCache == 0.00)
                return ""

            return Tools.formatNumber(this.shoppingQuantityCache)
        }

    var storageQuantityCache: Double? = null

    val isInStorage: Boolean
        get() {
            if (storageQuantityCache == null)
            {
                storageQuantityCache = Database.getArticleQuantityInStorage(this.articleId)
            }
            return storageQuantityCache!! > 0.0
        }

    val storageQuantityText: String
        get() {
            if (!isInStorage)
                return "0"

            return Tools.formatNumber(storageQuantityCache)
        }

    companion object {
        fun fromCursor(cursor: Cursor): Article {
            val article = Article()
            article.articleId       = cursor.getIntOrNull("ArticleId") ?: 0
            article.name            = cursor.getStringOrNull("Name")
            article.manufacturer    = cursor.getStringOrNull("Manufacturer")
            article.category        = cursor.getStringOrNull("Category")
            article.subCategory     = cursor.getStringOrNull("SubCategory")
            article.durableInfinity = cursor.getIntOrNull("DurableInfinity") == 1
            article.warnInDays      = cursor.getIntOrNull("WarnInDays")
            article.size            = cursor.getDoubleOrNull("Size")
            article.unit            = cursor.getStringOrNull("Unit")
            article.calorie         = cursor.getIntOrNull("Calorie")
            article.minQuantity     = cursor.getIntOrNull("MinQuantity")
            article.prefQuantity    = cursor.getIntOrNull("PrefQuantity")
            article.price           = cursor.getDoubleOrNull("Price")
            article.storageName     = cursor.getStringOrNull("StorageName")
            article.supermarket     = cursor.getStringOrNull("Supermarket")
            article.eanCode         = cursor.getStringOrNull("EANCode")
            article.notes           = cursor.getStringOrNull("Notes")

            return article
        }
    }
}