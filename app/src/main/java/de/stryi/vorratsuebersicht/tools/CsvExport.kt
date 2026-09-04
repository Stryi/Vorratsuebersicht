package de.stryi.vorratsuebersicht.tools

import android.app.Activity
import android.content.Intent
import androidx.core.content.FileProvider
import de.stryi.vorratsuebersicht.database.Database
import java.io.File
import java.time.LocalDate

class CsvExport private constructor(
    private val context: Activity,
    private val separator: String)
{

    companion object {

        fun exportArticles(context: Activity, separator: String)
        {
            val exporter = CsvExport(context, separator)
            val csv = exporter.getArticlesAsCsvString()
            exporter.writeToFileAndSend("Vue-Artikel.csv", csv)
        }

        fun exportStorageItems(context: Activity, separator: String)
        {
            val exporter = CsvExport(context, separator)
            val csv = exporter.getStorageItemsAsCsvString()
            exporter.writeToFileAndSend("Vue-Lagerbestand.csv", csv)
        }
    }

    private fun getArticlesAsCsvString(): StringBuilder {
        val header = StringBuilder()
            .append(
                "ArticleId|EANCode|Name|Manufacturer|Category|SubCategory|" +
                        "DurableInfinity|WarnInDays|Size|Unit|Notes|MinQuantity|" +
                        "PrefQuantity|StorageName|Supermarket|Calorie|Price"
            )
            .appendLine()
            .apply { replace(0, length, toString().replace("|", separator)) }

        val data = StringBuilder()
        val articles = Database.getArticleListToCsvExport()

        for (article in articles) {
            val row = StringBuilder()
            addField(row, article.articleId)
            addField(row, article.eanCode)
            addField(row, article.name)
            addField(row, article.manufacturer)
            addField(row, article.category)
            addField(row, article.subCategory)
            addField(row, article.durableInfinity)
            addField(row, article.warnInDays)
            addField(row, article.size)
            addField(row, article.unit)
            addField(row, article.notes)
            addField(row, article.minQuantity)
            addField(row, article.prefQuantity)
            addField(row, article.storageName)
            addField(row, article.supermarket)
            addField(row, article.calorie)
            addField(row, article.price)

            data.append(row).appendLine()
        }

        header.append(data)
        return header
    }

    private fun getStorageItemsAsCsvString(): StringBuilder {
        val header = StringBuilder()
            .append(
                "StorageItemId|ArticleId|Name|Manufacturer|Category|SubCategory|ArticleStorageName|" +
                        "DurableInfinity|WarnInDays|Quantity|BestBefore|StorageName"
            )
            .appendLine()
            .apply { replace(0, length, toString().replace("|", separator)) }

        val data = StringBuilder()
        val items = Database.getStorageItemQuantityListToCsvExport()
        for (item in items) {
            val row = StringBuilder()
            addField(row, item.storageItemId)
            addField(row, item.articleId)
            addField(row, item.name)
            addField(row, item.manufacturer)
            addField(row, item.category)
            addField(row, item.subcategory)
            addField(row, item.articleStorageName)
            addField(row, item.durableInfinity)
            addField(row, item.warnInDays)
            addField(row, item.quantity)
            addField(row, item.bestBefore)
            addField(row, item.storageName)

            data.append(row).appendLine()
        }

        header.append(data)
        return header
    }

    private fun writeToFileAndSend(fileName: String, content: StringBuilder)
    {
        val dir = context.externalCacheDirs.firstOrNull() ?: context.cacheDir
        val file = File(dir, fileName)

        file.writeText(content.toString())

        val uri = FileProvider.getUriForFile(
            context,
            "de.stryi.vorratsuebersicht.provider",
            file
        )

        val intent = Intent(Intent.ACTION_SEND)
        intent.type = "text/csv"
        intent.putExtra(Intent.EXTRA_SUBJECT, fileName)
        intent.putExtra(Intent.EXTRA_STREAM, uri)
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)

        context.startActivity(
            Intent.createChooser(intent, "CSV Datei senden"))
    }

    /* ---------- Helper ---------- */

    private fun appendSeparator(sb: StringBuilder) {
        if (sb.isNotEmpty()) sb.append(separator)
    }

    private fun addField(sb: StringBuilder, value: Boolean) {
        appendSeparator(sb)
        sb.append(value)
    }

    private fun addField(sb: StringBuilder, value: Int) {
        appendSeparator(sb)
        sb.append(value)
    }

    private fun addField(sb: StringBuilder, value: Int?) {
        appendSeparator(sb)
        sb.append(Tools.formatNumber(value))
    }

    private fun addField(sb: StringBuilder, value: Double?) {
        appendSeparator(sb)
        value ?: return

        var text = Tools.formatNumber(value)
        if (text.contains(separator)) {
            text = "\"$text\""
        }
        sb.append(text)
    }

    private fun addField(sb: StringBuilder, value: LocalDate?) {
        appendSeparator(sb)
        if (value == null) {
            return
        }

        sb.append(Tools.toHumanString(value))
    }

    private fun addField(sb: StringBuilder, text: String?) {
        appendSeparator(sb)
        text ?: return

        if (text.contains(separator)) {
            val escaped = text.replace("\"", "\\\"")
            sb.append("\"").append(escaped).append("\"")
        } else {
            sb.append(text)
        }
    }
}