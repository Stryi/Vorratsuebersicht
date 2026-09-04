package de.stryi.vorratsuebersicht.tools

import java.math.BigDecimal
import java.util.Locale

object PricePerUnit {

    fun calculate(priceText: String?, sizeText: String?, unitText: String?): String {
        if (unitText.isNullOrBlank()) return "---"

        val price = priceText?.toDoubleOrNull() ?: return "---"
        val size = sizeText?.toDoubleOrNull() ?: return "---"

        if (price == 0.toDouble() || size == 0.toDouble()) return "---"

        return calculate(price, size, unitText)
    }

    fun calculate(price: Double?, size: Double?, unitText: String?): String {
        if (price == null || size == null || unitText.isNullOrBlank() || size == 0.toDouble())
            return ""

        var calculatedSize = -1.toDouble()
        var calculatedUnit = ""
        var pricePerSize: Double = 0.toDouble()

        when (unitText.lowercase()) {
            "l" -> {
                if (size <= 0.25) {
                    calculatedSize = 100.toDouble()
                    calculatedUnit = "ml"
                    pricePerSize = (price / size) / 10.toDouble()
                } else {
                    calculatedSize = 1.toDouble()
                    calculatedUnit = "Liter"
                    pricePerSize = price / size
                }
            }

            "cl" -> {
                if (size <= 25.toDouble()) {
                    calculatedSize = 100.toDouble()
                    calculatedUnit = "ml"
                    pricePerSize = (price / size) * 10.toDouble()
                } else {
                    calculatedSize = 1.toDouble()
                    calculatedUnit = "Liter"
                    pricePerSize = (price / size) * 100.toDouble()
                }
            }

            "ml" -> {
                if (size == 100.toDouble()) return ""
                if (size <= 250.toDouble()) {
                    calculatedSize = 100.toDouble()
                    calculatedUnit = "ml"
                    pricePerSize = (price / size) * 100.toDouble()
                } else {
                    calculatedSize = 1.toDouble()
                    calculatedUnit = "Liter"
                    pricePerSize = (price / size) * 1000.toDouble()
                }
            }

            "kg" -> {
                if (size == 1.toDouble()) return ""
                if (size <= 0.250) {
                    calculatedSize = 100.toDouble()
                    calculatedUnit = "g"
                    pricePerSize = (price / size) / 10.toDouble()
                } else {
                    calculatedSize = 1.toDouble()
                    calculatedUnit = "kg"
                    pricePerSize = price / size
                }
            }

            "g" -> {
                if (size == 100.toDouble()) return ""
                if (size <= 250.toDouble()) {
                    calculatedSize = 100.toDouble()
                    calculatedUnit = "g"
                    pricePerSize = (price / size) * 100.toDouble()
                } else {
                    calculatedSize = 1.toDouble()
                    calculatedUnit = "kg"
                    pricePerSize = (price / size) * 1000.toDouble()
                }
            }
        }

        return if (calculatedSize > 0.toDouble()) {

            val formattedSize = BigDecimal.valueOf(calculatedSize)
                .stripTrailingZeros()
                .toPlainString()

            String.format(Locale.getDefault(), "%s %s = %.2f", formattedSize, calculatedUnit, pricePerSize)
        } else {
            ""
        }
    }
}
