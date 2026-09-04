package de.stryi.vorratsuebersicht.tools

import android.content.Context
import android.util.Log
import java.io.File
import java.lang.StringBuilder
import java.text.SimpleDateFormat
import java.util.*

object Logging {

    var logFileName : String = ""

    private val dateFormatDay = SimpleDateFormat("yyyy.MM.dd", Locale.getDefault())
    private val dateFormatLog = SimpleDateFormat("yyyy-MM-dd HH.mm.ss - ", Locale.getDefault())

    fun InitializeLogFile(context: Context)
    {
        // Beispiel: /storage/emulated/0/Android/data/de.stryi.vorratsuebersicht2/cache/Vue_2026.08.10.log
        logFileName = getCurrentLogFileName(context)

        deleteOldLogFiles(context)
    }

    fun getCurrentLogFileName(context: Context, day: Date? = null): String {

        val usedDay = day ?: Date()

        // "/data/user/0/<package>/files"
        var logFilePath = context.filesDir.absolutePath

        // externe Cache-Verzeichnisse
        val cacheDirs = context.externalCacheDirs
        if (!cacheDirs.isNullOrEmpty()) {
            logFilePath = cacheDirs[0].absolutePath
        }

        val fileName = "Vue_${dateFormatDay.format(usedDay)}.log"
        return File(logFilePath, fileName).absolutePath
    }

    fun logToFile(text: String) {

        // Beispiel: /storage/emulated/0/Android/data/de.stryi.vorratsuebersicht2/cache/Vue_2026.08.10.log
        if (logFileName.isEmpty())
            return

        try {
            File(logFileName).appendText(text)
        }
        catch (e: Exception) {
            Log.e("stryi", e.toString())
        }
    }

    fun createTestLogFiles(context: Context) {

        val calendar = Calendar.getInstance()

        for (index in -1 downTo -11) {

            val cal = calendar.clone() as Calendar
            cal.add(Calendar.DAY_OF_MONTH, index)

            val fileName = getCurrentLogFileName(context, cal.time)
            val file = File(fileName)

            if (!file.exists()) {

                file.appendText(dateFormatLog.format(Date()) + "Protokolleintrag 1\n")
                file.appendText(dateFormatLog.format(Date()) + "Protokolleintrag 2\n")
                file.appendText(dateFormatLog.format(Date()) + "Protokolleintrag 3\n")
            }
        }
    }

    /**
     * Nur die letzten 5 Logdateien behalten
     */
    fun deleteOldLogFiles(context: Context) {

        val fileList = getLogFileList(context)
        if (fileList.size <= 5) return

        for (i in 0 until fileList.size - 5) {
            File(fileList[i]).delete()
        }
    }

    fun getLogFileList(context: Context): Array<String> {

        // Beispiel: /data/user/0/de.stryi.vorratsuebersicht2/files
        var logFilePath = context.filesDir.absolutePath

        val sortedList = mutableListOf<String>()

        val baseDir = File(logFilePath)
        val unsorted = baseDir.listFiles { _, name ->
            name.startsWith("Vue_") && name.endsWith(".log")
        }

        unsorted?.forEach { sortedList.add(it.absolutePath) }

        // Beispiel: "/storage/emulated/0/Android/data/de.stryi.vorratsuebersicht2/cache"
        //           "/storage/0000-0000/Android/data/de.stryi.vorratsuebersicht2/cache"
        // Wenn keine SD Karte, dann nur der erste Eintrag.
        val cacheDirs = context.externalCacheDirs
        if (cacheDirs != null && cacheDirs.size > 0) {

            logFilePath = cacheDirs[0].absolutePath
            val cacheDir = File(logFilePath)

            val moreFiles = cacheDir.listFiles { _, name ->
                name.startsWith("Vue_") && name.endsWith(".log")
            }

            moreFiles?.forEach { sortedList.add(it.absolutePath) }
        }

        return sortedList.sorted().toTypedArray()
    }

    fun getLogFileText(context: Context): String {

        val text = StringBuilder()
        val fileList = getLogFileList(context)

        for (logFileName in fileList) {

            text.appendLine("LogFile: $logFileName")

            val file = File(logFileName)
            if (file.exists()) {
                text.appendLine(file.readText())
            }

            text.appendLine("**** E N D   OF    F I L E ****")
            text.appendLine()
        }

        return text.toString()
    }

    fun deleteAllLogFiles(context: Context) {
        val fileList = getLogFileList(context)

        for (logFileName in fileList)
        {
            File(logFileName).delete()
        }
    }
}