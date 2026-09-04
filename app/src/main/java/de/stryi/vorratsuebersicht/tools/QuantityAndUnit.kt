package de.stryi.vorratsuebersicht.tools

import java.util.Locale

class QuantityAndUnit(var quantity: Double, var unit: String) {
    //var quantity: Double = 0.00
    //var unit: String = ""

    fun quantityAndUnit(quantity: Double, unit: String) {
        this.quantity = quantity
        this.unit = unit
    }

    companion object {

        fun parse(valueText: String?) : QuantityAndUnit? {

            if (valueText.isNullOrEmpty())
                return null

            val quantityAndUnit = valueText.trim()

            var value : QuantityAndUnit?

            value = parse(quantityAndUnit, " kg"); if (value != null) return value
            value = parse(quantityAndUnit, " g");  if (value != null) return value
            value = parse(quantityAndUnit, " gr"); if (value != null) return value
            value = parse(quantityAndUnit, " l");  if (value != null) return value
            value = parse(quantityAndUnit, " ml"); if (value != null) return value

            value = parse(quantityAndUnit, "kg"); if (value != null) return value
            value = parse(quantityAndUnit, "g");  if (value != null) return value
            value = parse(quantityAndUnit, "gr"); if (value != null) return value
            value = parse(quantityAndUnit, "l");  if (value != null) return value
            value = parse(quantityAndUnit, "ml"); if (value != null) return value

            return null
        }

        fun parse(quantityAndUnit: String, unit: String): QuantityAndUnit?
        {
            // Prüfen, ob String mit der Einheit endet (case-insensitive)
            if (!quantityAndUnit.endsWith(unit, ignoreCase = true)) {
                return null
            }

            // Zahl extrahieren
            var quantityText = quantityAndUnit.substring(0, quantityAndUnit.length - unit.length)
            val normalizedUnit = unit.trim().lowercase(Locale.getDefault())

            // Komma gegen Punkt tauschen
            quantityText = quantityText.replace(",", ".")

            // Versuch, in Double umzuwandeln
            val quantity = quantityText.toDoubleOrNull() ?: return null

            return QuantityAndUnit(quantity, normalizedUnit)
        }
    }
}