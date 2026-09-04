package de.stryi.vorratsuebersicht.tools

object UnitConvert {

    fun getGesamtCalorie(sizeText : String, unit : String, caloriePerUnitText : String) : String
    {
        var factor = 1.0
        val caloriePerUnit = caloriePerUnitText.toLongOrNull() ?: return ""
        val size = sizeText.toDoubleOrNull() ?: return ""

        if (caloriePerUnit == 0L || size == 0.0)
            return ""

        val unitPerX = getConvertUnit(unit)
        when {
            unit == "kg" && unitPerX ==  "g" -> factor = 10.0
            unit ==  "g" && unitPerX ==  "g" -> factor = 0.01
            unit ==  "l" && unitPerX == "ml" -> factor = 10.0
            unit == "ml" && unitPerX == "ml" -> factor = 0.01
            unitPerX.isEmpty() -> return ""
        }

        return try {
            val calorieGes = (caloriePerUnit * factor * size).toLong()
            calorieGes.toString()
        } catch (_: Exception) {
            ""
        }

    }

    fun getCaloriePerUnit(sizeText: String?, unit: String?, calorieText: String?): String {
        val size = sizeText?.toDoubleOrNull() ?: return "---"
        val calorie = calorieText?.toDoubleOrNull() ?: return "---"
        val u = unit?.lowercase() ?: return "---"

        if (size <= 0.0 || calorie <= 0.0) return "---"

        val factor = when (u) {
            "kg", "l" -> 10.0
            "g", "ml" -> 0.01
            else -> return "---"
        }

        val calPerUnit = (calorie / size / factor).toInt()
        return calPerUnit.toString()
    }

    fun getConvertUnit(unit: String?): String {
        return when (unit?.lowercase()) {
            "kg", "g" -> "g"
            "l", "ml" -> "ml"
            else -> ""
        }
    }
}
