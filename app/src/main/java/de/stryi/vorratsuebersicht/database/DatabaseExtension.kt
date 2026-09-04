package de.stryi.vorratsuebersicht.database

import android.database.Cursor
import android.util.Log
import java.text.SimpleDateFormat
import java.time.LocalDate
import java.time.format.DateTimeFormatter
import java.util.Date
import java.util.Locale

fun Cursor.getStringOrNull(columnName: String): String? =
    getColumnIndexOrNull(columnName)?.takeIf { !isNull(it) }?.let { getString(it) }

fun Cursor.getIntOrNull(columnName: String): Int? =
    getColumnIndexOrNull(columnName)?.takeIf { !isNull(it) }?.let { getInt(it) }

fun Cursor.getDoubleOrNull(columnName: String): Double? {
    val columnIndex = getColumnIndexOrNull(columnName)
    if (columnIndex == null) {
        return null
    }

    if (isNull(columnIndex)) {
        return null
    }

    return getDouble(columnIndex)
}

fun Cursor.getDouble(columnName: String, defaultValue: Double): Double {
    val columnIndex = getColumnIndexOrNull(columnName)
    if (columnIndex == null) {
        return defaultValue
    }

    if (isNull(columnIndex)) {
        return defaultValue
    }

    return getDouble(columnIndex)
}

fun Cursor.getDateOrNull(columnName: String): Date?
    {
        val index = getColumnIndexOrNull(columnName) ?: return null
        if (isNull(index)) return null

        val dateStr = getString(index)
        if (dateStr.isBlank())
        {
            return null
        }
        val sdf = SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault())
        try {
            return sdf.parse(dateStr)
        } catch (e: Exception) {
            Log.e("getDateOrNull", e.message!!)
        }
        return null
    }

private val ISO_DATE = DateTimeFormatter.ISO_LOCAL_DATE

fun Cursor.getLocalDateOrNull(columnName: String): LocalDate? {
    val index = getColumnIndexOrNull(columnName) ?: return null
    if (isNull(index)) return null

    val dateStr = getString(index)?.trim() ?: return null
    if (dateStr.length < 10) return null

    return try {
        LocalDate.parse(dateStr.take(10), ISO_DATE)
    } catch (e: Exception) {
        Log.e("getLocalDateOrNull", "Invalid date: $dateStr", e)
        null
    }
}

fun Cursor.getBlobOrNull(columnName: String): ByteArray?
{
    val index = this.getColumnIndex(columnName)
    if (index < 0) { return null }
    return this.getBlob(index)
}

fun Cursor.getColumnIndexOrNull(columnName: String): Int? =
    try {
        getColumnIndexOrThrow(columnName)
    } catch (_: IllegalArgumentException) {
        null
    }
