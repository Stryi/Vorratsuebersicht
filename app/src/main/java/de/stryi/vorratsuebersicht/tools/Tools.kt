package de.stryi.vorratsuebersicht.tools

import android.app.Activity
import android.content.Context
import android.net.Uri
import android.provider.OpenableColumns
import android.util.Log
import android.view.View
import android.view.ViewGroup
import android.widget.EditText
import android.widget.ImageView
import android.widget.Toast
import androidx.appcompat.app.AlertDialog
import de.stryi.vorratsuebersicht.R
import kotlinx.coroutines.suspendCancellableCoroutine
import java.io.File
import java.text.DecimalFormat
import java.text.DecimalFormatSymbols
import java.text.SimpleDateFormat
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter
import java.time.format.DateTimeParseException
import java.util.Date
import java.util.Locale
import kotlin.coroutines.resume

object Tools {

    fun toFuzzyByteString(bytes: Long): String {

        var s = bytes.toDouble()
        val formats = arrayOf(
            "%,.0f bytes", "%,.2f KB",
            "%,.2f MB", "%,.2f GB", "%,.2f TB",
            "%,.2f PB", "%,.2f EB"
        )

        var i = 0
        while (i < formats.size - 1 && s >= 1024) {
            s = (100 * s / 1024).toInt() / 100.0  // Rundung auf 2 Nachkommastellen
            i++
        }

        return String.format(Locale.getDefault(), formats[i], s)
    }

    fun toHumanText(dateTime: LocalDateTime?): String
    {
        if (dateTime == null) return ""

        var datumText = dateTime.format(DateTimeFormatter.ofPattern("dd.MM.yyyy"))

        val now = LocalDateTime.now()

        if (dateTime.dayOfYear == now.dayOfYear)
        {
            datumText = "Heute"
        }

        if (dateTime.dayOfYear == now.minusDays(1).dayOfYear)
        {
            datumText = "Gester"
        }

        // Keine Uhrzeit?
        if (dateTime.hour == 0 && dateTime.minute == 0 && dateTime.second == 0) {
            return datumText
        }

        datumText = datumText + " um " + dateTime.format(DateTimeFormatter.ofPattern("HH:mm"))

        return datumText
    }

    fun formatNumber(number: Double?): String {
        if (number == null)
            return ""

        val locale = Locale.getDefault()
        val symbols = DecimalFormatSymbols(locale)
        val formatter = DecimalFormat("###,###,###,###.####", symbols)
        return formatter.format(number)
    }

    fun formatNumber(number: Int?): String {
        if (number == null)
            return ""

        val locale = Locale.getDefault()
        val symbols = DecimalFormatSymbols(locale)
        val formatter = DecimalFormat("###,###,###,###", symbols)
        return formatter.format(number)
    }

    fun toString(date: LocalDate?) : String? {
        if (date == null)
            return ""

        val formatter  = DateTimeFormatter.ofPattern("yyyy-MM-dd", Locale.getDefault())
        return date.format(formatter)
    }

    fun toHumanString(date: LocalDate?) : String? {
        if (date == null)
            return ""

        val formatter  = DateTimeFormatter.ofPattern("yyyy.MM.dd", Locale.getDefault())
        return date.format(formatter)
    }

    fun formatResource(
        activity: Activity,
        resourceId: Int,
        vararg args: Any
    ): String {
        // Argumente durchgehen und bei Bedarf formatieren
        val formattedArgs = args.map { arg ->
            when (arg) {
                is Double -> formatNumber(arg)
                is Int    -> formatNumber(arg.toDouble())
                else      -> arg          // Strings, andere Typen unverändert
            }
        }.toTypedArray()

        val template = activity.resources.getString(resourceId)
        return template.format(*formattedArgs)
    }

    fun stringToDate(dateString: String?): LocalDate?
    {
        if (dateString == null)
            return null

        if (dateString.length == 10) {
            val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd", Locale.getDefault())
            return LocalDate.parse(dateString, formatter)
        }

        val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd HH:mm:ss", Locale.getDefault())
        return LocalDate.parse(dateString, formatter)
    }

    fun stringToDateTime(dateString: String?): LocalDateTime?
    {
        if (dateString == null)
            return null

        if (dateString.length == 10) {
            val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd", Locale.getDefault())
            return LocalDateTime.parse(dateString, formatter)
        }

        val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd HH:mm:ss", Locale.getDefault())
        return LocalDateTime.parse(dateString, formatter)
    }

    fun dateToString(date: LocalDate?): String
    {
        if (date == null)
            return ""

        var dateText: String

        try {
            val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd", Locale.getDefault())
            dateText = date.format(formatter)
        }
        catch (e: Exception) {
            dateText = e.message.toString()
        }
        
        return dateText
    }


    fun getFileNameFromUri(context: Context, uri: Uri): String {
        var name = ""
        context.contentResolver.query(uri, null, null, null, null)?.use { cursor ->
            val nameIndex = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME)
            if (nameIndex >= 0 && cursor.moveToFirst()) {
                name = cursor.getString(nameIndex)
            }
        }
        return name
    }

    fun getBackupDatabaseName(backupFilePath: String): String {
        var databaseName = File(backupFilePath).nameWithoutExtension

        if (databaseName.length < 19) {
            return databaseName
        }

        val dateTimeText = databaseName.takeLast(19)
        val formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH.mm.ss", Locale.US)

        val isDateTime = try {
            LocalDateTime.parse(dateTimeText, formatter)
            true
        } catch (_: DateTimeParseException) {
            false
        }

        if (!isDateTime) {
            return databaseName
        }

        // Datumsteil entfernen
        databaseName = databaseName.dropLast(19).trimEnd('_')

        if (databaseName == "Vue") {
            databaseName = "Vorraete"
        }

        return databaseName
    }

    suspend fun askForText(
        context: Context,
        title: String,
        message: String,
        name: String): String? =
        suspendCancellableCoroutine { continuation ->

            val input = EditText(context)
            input.inputType = android.text.InputType.TYPE_TEXT_FLAG_AUTO_COMPLETE
            input.setText(name)

            // Setze den Abstand (Padding) für den EditText
            val marginInDp = 20
            val scale: Float = context.resources.displayMetrics.density
            val marginInPixels = (marginInDp * scale + 0.5f).toInt()
            input.setPadding(marginInPixels, 0, marginInPixels, marginInPixels)
            input.requestFocus()

            val builder = AlertDialog.Builder(context, R.style.MyAlertDialogTheme)
            builder.setTitle(title)
            builder.setMessage(message)
            builder.setView(input)
            builder.setPositiveButton(context.getString(R.string.App_Ok)) { _, _ ->
                continuation.resume(input.text.toString().trim())
            }
            builder.setNegativeButton(context.getString(R.string.App_Cancel)) { _, _ ->
                continuation.resume(null)
            }
            builder.setOnCancelListener {
                continuation.resume(null)
            }

            builder.show()
        }


    fun showMessage(activity: Activity, message: String) {
        val messageBox = AlertDialog.Builder(activity, R.style.MyAlertDialogTheme)
        messageBox.setMessage(message)
        messageBox.setPositiveButton(activity.resources.getString(R.string.App_Ok)) { _, _ -> }
        messageBox.show()
    }

    fun showWarning(activity: Activity, message: String) {
        val messageBox = AlertDialog.Builder(activity, R.style.MyAlertDialogTheme)
        messageBox.setIcon(android.R.drawable.ic_dialog_alert)
        messageBox.setTitle("ACHTUNG")
        messageBox.setMessage(message)
        messageBox.setPositiveButton(activity.resources.getString(R.string.App_Ok)) { _, _ -> }
        messageBox.show()
    }

    fun showException(
        activity: Activity,
        exception: Exception?,
        positivMessage: String?,
        exceptionMessage: String)
    {
        if (exception == null)
        {
            if (positivMessage == null)
                return

            Toast.makeText(activity, positivMessage, Toast.LENGTH_LONG).show()
            return
        }

        TRACE(exceptionMessage)
        TRACE(exception.message)
        TRACE(exception.stackTraceToString())

        this.showWarning(activity, exceptionMessage + "\n\n" + exception)
    }

    fun findFirstEditText(view: View): EditText? {
        if (view is EditText) return view
        if (view !is ViewGroup) return null
        for (i in 0 until view.childCount) {
            val child = view.getChildAt(i)
            val found = findFirstEditText(child)
            if (found != null) return found
        }
        return null
    }

    fun findFirstImageView(view: View): ImageView? {
        if (view is ImageView) return view
        if (view !is ViewGroup) return null
        for (i in 0 until view.childCount) {
            val child = view.getChildAt(i)
            val found = findFirstImageView(child)
            if (found != null) return found
        }
        return null
    }

    /// Statische Funktion TRACE(strin)
    fun TRACE(format: String?, vararg args: Any)
    {
        if (format == null)
            return

        val text = String.format(Locale.getDefault(), format, *args)

        TRACE(text)
    }

    fun TRACE(text: String?) {
        if (text == null)
            return

        Log.d("stryi", text)
        logToFile(text)
    }

    var start = System.currentTimeMillis()

    private fun DURATION_START()
    {
        this.start = System.currentTimeMillis()
    }

    private fun DURATION_TRACE(text: String)
    {
        val end = System.currentTimeMillis()
        val duration = end - this.start
        val minutes = duration / 60000
        val seconds = (duration % 60000) / 1000
        val millis = duration % 1000
        TRACE("$text => Dauer: %02d:%02d.%03d".format(minutes, seconds, millis))
    }


    private val timeFormat = SimpleDateFormat("HH.mm.ss - ", Locale.getDefault())

    fun logToFile(text: String) {

        try {

            val lines = text.split("\n")

            for (line in lines) {

                if (line.isEmpty()) continue

                val lineText = timeFormat.format(Date()) + line + "\r\n"
                Logging.logToFile(lineText)
            }

        } catch (e: Exception) {
            Log.e("stryi", e.toString())
        }
    }

    fun getLogFileName() : String
    {
        return Logging.logFileName
    }
}
