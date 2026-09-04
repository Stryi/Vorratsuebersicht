package de.stryi.vorratsuebersicht.tools

import android.content.Context
import de.stryi.vorratsuebersicht.MainActivity
import java.text.SimpleDateFormat
import java.util.*
import androidx.core.content.edit
import java.time.LocalDate
import java.time.format.DateTimeFormatter

internal object Settings {

    private const val PREF_NAME = "Vorratsübersicht"
    private const val DATE_PATTERN = "yyyy.MM.dd"
    private val dateFormat = SimpleDateFormat(DATE_PATTERN, Locale.US)

    private fun prefs(): android.content.SharedPreferences {
        return MainActivity.appContext
            .getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE)
    }

    internal fun getString(key: String, defValue: String): String {
        return prefs().getString(key, defValue) ?: defValue
    }

    internal fun getInt(key: String, defValue: Int): Int {
        return prefs().getInt(key, defValue)
    }

    internal fun getBoolean(key: String, defValue: Boolean): Boolean {
        return prefs().getBoolean(key, defValue)
    }

    internal fun getDate(key: String): LocalDate? {
        val dateText = prefs().getString(key, null)
        if (dateText == null)
        {
            return null
        }

        val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd", Locale.getDefault())
        return LocalDate.parse(dateText, formatter)
    }

    internal fun putString(key: String, value: String) {
        prefs().edit(commit = true) {
            putString(key, value)
        }
    }

    internal fun putInt(key: String, value: Int) {
        prefs().edit(commit = true) {
            putInt(key, value)
        }
    }

    internal fun putBoolean(key: String, value: Boolean) {
        prefs().edit(commit = true) {
            putBoolean(key, value)
        }
    }

    internal fun putDate(key: String, value: LocalDate) {
        val dateText = value.format(DateTimeFormatter.ofPattern("yyyy.MM.dd"))

        prefs().edit(commit = true) {
            putString(key, dateText)
        }
    }

    internal fun clear(key: String) {
        prefs().edit(commit = true) {
            remove(key)
        }
    }
}
