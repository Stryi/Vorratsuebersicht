package de.stryi.vorratsuebersicht.database.Records

import android.database.Cursor
import de.stryi.vorratsuebersicht.database.getDouble
import de.stryi.vorratsuebersicht.database.getIntOrNull
import de.stryi.vorratsuebersicht.database.getLocalDateOrNull
import de.stryi.vorratsuebersicht.database.getStringOrNull
import java.time.LocalDate

class StorageItem {

    var storageItemId : Int = 0
    var articleId: Int = 0
    var bestBefore : LocalDate? = null
    var quantity : Double = 0.00
    var warnInDays : Int? = null
    var durableInfinity : Boolean = false
    var storageName : String? = null

    var isChanged = false

    val warningLevel : Int
        get() {
            if (this.bestBefore == null)
                return 0

            if (this.bestBefore!!.isBefore(LocalDate.now()))
                return 2

            if (warnInDays == null)
                return 0

            if (this.bestBefore!!.minusDays(warnInDays!!.toLong()).isBefore(LocalDate.now()))
                return 1

            return 0
        }

    companion object {
        fun fromCursor(cursor: Cursor): StorageItem {
            val storageItem = StorageItem()
            storageItem.storageItemId   = cursor.getIntOrNull("StorageItemId") ?: 0
            storageItem.articleId       = cursor.getIntOrNull("ArticleId") ?: 0
            storageItem.bestBefore      = cursor.getLocalDateOrNull("BestBefore")
            storageItem.quantity        = cursor.getDouble("Quantity", 0.00)
            storageItem.warnInDays      = cursor.getIntOrNull("WarnInDays")
            storageItem.durableInfinity = cursor.getIntOrNull("DurableInfinity") == 1
            storageItem.storageName     = cursor.getStringOrNull("StorageName")
            return storageItem
        }
    }
}