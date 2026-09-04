package de.stryi.vorratsuebersicht.database.Records

import android.database.Cursor
import de.stryi.vorratsuebersicht.database.getDouble
import de.stryi.vorratsuebersicht.database.getIntOrNull
import de.stryi.vorratsuebersicht.database.getLocalDateOrNull
import de.stryi.vorratsuebersicht.database.getStringOrNull
import java.time.LocalDate

class StorageItemCsvExport {
    var storageItemId : Int = 0
    var articleId: Int = 0
    var name: String = ""
    var manufacturer: String = ""
    var category: String = ""
    var subcategory: String = ""
    var articleStorageName: String = ""
    var durableInfinity: Boolean = false
    var warnInDays: Int? = null
    var quantity: Double = 0.00
    var bestBefore: LocalDate? = null
    var storageName: String? = null

    companion object {
        fun fromCursor(cursor: Cursor): StorageItemCsvExport {
            val storageItem = StorageItemCsvExport()
            storageItem.storageItemId   = cursor.getIntOrNull("StorageItemId") ?: 0
            storageItem.articleId       = cursor.getIntOrNull("ArticleId") ?: 0
            storageItem.name            = cursor.getStringOrNull("Name") ?: ""
            storageItem.manufacturer    = cursor.getStringOrNull("Manufacturer") ?: ""
            storageItem.category        = cursor.getStringOrNull("Category") ?: ""
            storageItem.subcategory     = cursor.getStringOrNull("SubCategory") ?: ""
            storageItem.articleStorageName = cursor.getStringOrNull("ArticleStorageName") ?: ""
            storageItem.durableInfinity = cursor.getIntOrNull("DurableInfinity") == 1
            storageItem.warnInDays      = cursor.getIntOrNull("WarnInDays")
            storageItem.quantity        = cursor.getDouble("Quantity", 0.00)
            storageItem.bestBefore      = cursor.getLocalDateOrNull("BestBefore")
            storageItem.storageName     = cursor.getStringOrNull("StorageName")
            return storageItem
        }
    }
}