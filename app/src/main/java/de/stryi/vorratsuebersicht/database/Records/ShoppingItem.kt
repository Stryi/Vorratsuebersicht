package de.stryi.vorratsuebersicht.database.Records

import android.database.Cursor
import de.stryi.vorratsuebersicht.ShoppingItemViewAdapter
import de.stryi.vorratsuebersicht.database.getDoubleOrNull
import de.stryi.vorratsuebersicht.database.getIntOrNull
import de.stryi.vorratsuebersicht.database.getStringOrNull
import de.stryi.vorratsuebersicht.tools.PricePerUnit
import de.stryi.vorratsuebersicht.tools.Tools

class ShoppingItem {

    var shoppingListId : Int = 0
    var articleId : Int = 0
    var name : String = ""
    var manufacturer : String? = null
    var supermarket: String? = null
    var category: String? = null
    var subCategory: String? = null
    var size: Double? = null
    var unit: String? = null
    var calorie: Int? = null
    var quantity: Double? = null
    var notes: String? = null
    var price: Double? = null
    var bought: Boolean? = null

    val heading: String
        get() {
            return this.name
        }

    val shoppingInfo: String
        get() {

            val info = StringBuilder()
            if (!this.manufacturer.isNullOrEmpty())
            {
                if (info.isNotEmpty()) info.appendLine()

                info.append("Hersteller: %s".format(this.manufacturer))
            }
            if (this.size != null)
            {
                if (info.isNotEmpty()) info.appendLine()

                info.append("Inhalt/Größe: %.0f %s".format(this.size, this.unit))
            }
            if (!this.supermarket.isNullOrEmpty())
            {
                if (info.isNotEmpty()) info.appendLine()

                info.append("Einkaufsmarkt: %s".format(this.supermarket))
            }
            if (this.price != null && this.price != 0.0)
            {
                if (info.isNotEmpty()) info.appendLine()

                info.append("Preis: %.2f".format(this.price))
                val pricePerUnit = PricePerUnit.calculate(this.price, this.size, this.unit)
                if (pricePerUnit.isNotBlank())
                {
                    info.append(" ($pricePerUnit)")
                }
            }

            if (ShoppingItemViewAdapter.sparseView < 1)
            {
                // Kategorie / Unterkategorie
                var categoryText = ""
                if (!this.category.isNullOrEmpty())
                {
                    categoryText = this.category.toString()
                }
                if (!this.subCategory.isNullOrEmpty())
                {
                    if (!categoryText.isEmpty())
                    {
                        categoryText += " / "
                    }
                    categoryText += this.subCategory
                }

                if (categoryText.isNotBlank()) {
                    info.appendLine()
                    info.append("Kategorie: $categoryText")
                }

                if (!this.notes.isNullOrEmpty())
                {
                    info.appendLine()
                    info.append("Notizen: %s".format(this.notes))
                }
            }

            return info.toString()
        }

    val quantityText: String
        get ()
        {
            return "Anzahl: %s".format(Tools.formatNumber(this.quantity))
        }

    companion object {
        fun fromCursor(cursor: Cursor): ShoppingItem {
            val shoppingItem = ShoppingItem()

            shoppingItem.shoppingListId = cursor.getIntOrNull("ShoppingListId") ?: 0
            shoppingItem.articleId    = cursor.getIntOrNull("ArticleId") ?: 0
            shoppingItem.name         = cursor.getStringOrNull("Name") ?: ""
            shoppingItem.manufacturer = cursor.getStringOrNull("Manufacturer")
            shoppingItem.supermarket  = cursor.getStringOrNull("Supermarket")
            shoppingItem.category     = cursor.getStringOrNull("Category")
            shoppingItem.subCategory  = cursor.getStringOrNull("SubCategory")
            shoppingItem.size         = cursor.getDoubleOrNull("Size")
            shoppingItem.unit         = cursor.getStringOrNull("Unit")
            shoppingItem.calorie      = cursor.getIntOrNull("Calorie")
            shoppingItem.quantity     = cursor.getDoubleOrNull("Quantity")
            shoppingItem.notes        = cursor.getStringOrNull("Notes")
            shoppingItem.price        = cursor.getDoubleOrNull("Price")
            shoppingItem.bought       = cursor.getIntOrNull("Bought") == 1

            return  shoppingItem
        }
    }
}