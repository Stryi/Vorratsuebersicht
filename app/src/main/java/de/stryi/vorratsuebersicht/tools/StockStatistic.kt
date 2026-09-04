import android.annotation.SuppressLint
import android.app.Activity
import de.stryi.vorratsuebersicht.R
import de.stryi.vorratsuebersicht.database.Records.ArticleInfo
import de.stryi.vorratsuebersicht.tools.Tools
import java.text.NumberFormat

class StockStatistic {

    internal var count = 0
    internal var quantity = 0.0     // Double statt decimal
    private val sumMenge = mutableMapOf<String, Double>()
    private var sumWarnung = 0.0
    private var sumAbgelaufen = 0.0
    private var sumKcal = 0.0
    private var sumPrice = 0.0

    /*
    fun addWarningLevel1(quantity: Double) {
        sumWarnung += quantity
    }

    fun addWarningLevel2(quantity: Double) {
        sumAbgelaufen += quantity
    }
    */

    private fun addUnitQuantity(unit: String?, size: Double?, quantity: Double) {
        if (size == null || size <= 0.0)
            return

        val key = unit ?: ""
        sumMenge[key] = (sumMenge[key] ?: 0.0) + size * quantity
    }

    private fun addCalorie(quantity: Double, calorie: Int?) {
        if (calorie == null || calorie <= 0)
            return

        sumKcal += quantity * calorie
    }

    fun addStorageItem(item: ArticleInfo) {
        count++
        quantity += item.storageQuantity
        addUnitQuantity(item.unit, item.size, item.storageQuantity)
        addCalorie(item.storageQuantity, item.calorie)
        addCosts(item.storageQuantity, item.price)
    }

    private fun addCosts(quantity: Double, price: Double?) {
        if (price != null) {
            sumPrice += quantity * price
        }
    }

    fun getText(storageItemQuantityList: List<ArticleInfo>, activity: Activity):String {

        for (item in storageItemQuantityList) {

            val itemList = item.storageArticleList
            for (item in itemList)
            {
                if (item.warningLevel == 1)
                {
                    sumWarnung += item.quantity
                }

                if (item.warningLevel == 2)
                {
                    sumAbgelaufen += item.quantity
                }
            }

            addStorageItem(item)
        }

        return getStatistic(activity)
    }

    @SuppressLint("StringFormatMatches", "StringFormatInvalid")
    fun getStatistic(activity: Activity): String {
        val res = activity.resources
        var status: String

        if (count == 1)
            status = String.format(res.getString(R.string.StorageListSummary_Position), count)
        else
            status = String.format(res.getString(R.string.StorageListSummary_Positions), Tools.formatNumber(count))

        val quantityText = res.getString(R.string.StorageListSummary_Quantity)

        status += ", " + String.format(quantityText, Tools.formatNumber(quantity))

        if (sumMenge.isNotEmpty()) {
            convertUnits()

            /*
            val mengeListe = sumMenge
                .filter { it.value > 0 }
                .entries.joinToString(", ") {
                    String.format(Locale.getDefault(), "%,.######f %s", it.value, it.key)
                }

            if (mengeListe.isNotEmpty()) {
                append(", ")
                append(
                    String.format(
                        res.getString(R.string.StorageListSummary_Amount),
                        mengeListe
                    )
                )
            }
            */

            if (sumWarnung > 0)
            {
                status += ", " + String.format(
                    res.getString(R.string.StorageListSummary_Warning),
                    Tools.formatNumber(sumWarnung))
            }

            if (sumAbgelaufen > 0)
            {
                status += ", " + String.format(
                    res.getString(R.string.StorageListSummary_Off),
                    Tools.formatNumber(sumAbgelaufen))
            }
        }

        if (sumKcal > 0)
            status += ", " + String.format(
                res.getString(R.string.StorageListSummary_Calories),
                Tools.formatNumber(sumKcal))

        if (sumPrice > 0)
            status += ", " + String.format(
                    res.getString(R.string.StorageListSummary_Value),
                    Tools.formatNumber(sumPrice),
                    NumberFormat.getCurrencyInstance().currency?.symbol ?: "")

        /*
        if (sumWarnung > 0)
            status = ", " + String.format(
                res.getString(R.string.StorageListSummary_Warning), Tools.formatNumber(sumWarnung))

        if (sumAbgelaufen > 0)
            status = ", " + String.format(
                res.getString(R.string.StorageListSummary_Off), Tools.formatNumber(sumAbgelaufen))
        */

        return status
    }

    private fun convertUnits() {
        if (sumMenge.containsKey("ml") && sumMenge.containsKey("l")) {
            val liter = (sumMenge["l"] ?: 0.0) + (sumMenge["ml"] ?: 0.0) / 1000
            sumMenge.remove("ml")
            sumMenge["l"] = liter
        }

        if (sumMenge.containsKey("cl") && sumMenge.containsKey("l")) {
            val liter = (sumMenge["l"] ?: 0.0) + (sumMenge["cl"] ?: 0.0) / 100
            sumMenge.remove("cl")
            sumMenge["l"] = liter
        }

        if (sumMenge.containsKey("g") && sumMenge.containsKey("kg")) {
            val gewicht = (sumMenge["kg"] ?: 0.0) + (sumMenge["g"] ?: 0.0) / 1000
            sumMenge.remove("g")
            sumMenge["kg"] = gewicht
        }
    }
}
