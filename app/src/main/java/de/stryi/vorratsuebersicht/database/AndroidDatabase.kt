package de.stryi.vorratsuebersicht.database

import android.content.Context
import de.stryi.vorratsuebersicht.tools.trimEnd
import java.io.File
import java.io.IOException
import java.io.InputStream
import java.io.OutputStream

object AndroidDatabase {

    const val SQLITE_FILENAME_PROD = "Vorraete.db3"
    const val SQLITE_FILENAME_NEW  = "Vorraete_db0.db3"
    const val SQLITE_FILENAME_DEMO = "Vorraete_Demo.db3"

    /// <summary>
    /// Datenbanken aus den Resourcen erstellen.
    /// Wird beim ersten Start der App aufgerufen.
    /// </summary>
    fun restoreDatabasesFromResourcesOnStartup(context: Context)
    {
        // Productive neue Datenbank erstellen.
        createLocalizedDatabaseFromAsset(context, SQLITE_FILENAME_PROD)

        // Demo database with sample data.
        val created = createLocalizedDatabaseFromAsset(context, SQLITE_FILENAME_DEMO)
        if (created)
        {
            Database.prepareTestDatabase(context)
        }
    }

    /// <summary>
    /// Erstellt Datenbank aus den Resourcen, wenn sie noch nicht da ist.
    /// </summary>
    private fun createLocalizedDatabaseFromAsset(context: Context, fileName: String) : Boolean
    {
        // "/storage/emulated/0/Android/data/de.stryi.vorratsuebersicht/files"
        val dbPath = context.getExternalFilesDir(null)

        val dbFile = File(dbPath, fileName)

        if (dbFile.exists())
            return false

        context.assets.open(fileName).use { inputStream ->
            dbFile.outputStream().use { outputStream ->
                inputStream.copyTo(outputStream)
                return true
            }
        }
    }

    /// <summary>
    /// Erstellt Datenbank aus den Resourcen unter einem speziellen Namen.
    /// </summary>
    fun createLocalizedDatabaseFromAsset(
        context: Context,
        assetsFileName: String,
        databaseFileName: String,
        overrideIfExists: Boolean = false,
        targetDir: File? = null) : Exception?
    {
        // "/storage/emulated/0/Android/data/de.stryi.vorratsuebersicht/files"
        val dbPath = targetDir ?: context.getExternalFilesDir(null)

        val dbFile = File(dbPath, databaseFileName)

        val dbName = databaseFileName.trimEnd(".db3")

        if (dbFile.exists()) {
            if (overrideIfExists) {
                dbFile.delete()
            }
            else {
                return Exception("Die Datenbank '$dbName' existiert bereits.")
            }
        }

        var inputStream: InputStream? = null
        var outputStream: OutputStream? = null

        var result : Exception? = null

        try {
            inputStream = context.assets.open(assetsFileName)
            outputStream = dbFile.outputStream()

            inputStream.copyTo(outputStream)
        } catch (e: IOException) {
            e.printStackTrace()
            result = e
        } finally {
            try {
                inputStream?.close()
            } catch (e: IOException) {
                e.printStackTrace()
            }
            try {
                outputStream?.close()
            } catch (e: IOException) {
                e.printStackTrace()
            }
        }
        return result
    }

    fun loadDatabaseFileListSafe(context: Context):  MutableList<File>
    {
        val fileList = mutableListOf<File>()

        // "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
        // "/storage/0E0E-2316/Android/data/de.stryi.Vorratsuebersicht/files"
        val externalFilesDirs = context.getExternalFilesDirs(null)

        for(extFilesDir in externalFilesDirs)
        {
            if (extFilesDir == null) continue

            for (file in extFilesDir.listFiles()!!)
            {
                if (!file.name.endsWith("db3"))
                    continue

                if (!file.canWrite())
                    continue

                fileList.add(file)
            }
        }

        fileList.sortBy { it.name }

        return fileList
    }

    fun getDemoDatabasePath(context: Context): String
    {
        val dbPath = context.getExternalFilesDir(null)
        val testDbFileName = File(dbPath, SQLITE_FILENAME_DEMO)

        return testDbFileName.absolutePath
    }

    fun renameDatabase(
        oldDatabaseFilePath: String,
        newDatabaseName: String) : Boolean
    {
        val oldFile = File(oldDatabaseFilePath)
        val newFile = File(oldFile.parent, "$newDatabaseName.db3")

        var ok = true

        if (oldFile.exists()) {
            ok = oldFile.renameTo(newFile)
        }

        return ok
    }

    fun isDatabaseExists(context: Context, newDatabaseName: String): Boolean {
        val databaseList = loadDatabaseFileListSafe(context)
        for (databaseFile in databaseList)
        {
            if (databaseFile.nameWithoutExtension.equals(newDatabaseName, ignoreCase = true))
            {
                return true
            }
        }
        return false
    }

    fun isTestDatabase(): Boolean {
        val databaseName = Database.getDatabaseName()

        return databaseName == "Vorraete_Demo"
    }

    fun getDatabaseFullName(context: Context, newDatabaseName: String): String? {
        val databaseList = loadDatabaseFileListSafe(context)
        for (databaseFile in databaseList)
        {
            if (databaseFile.nameWithoutExtension.equals(newDatabaseName, ignoreCase = true))
            {
                return databaseFile.absolutePath
            }
        }
        return null
    }

    fun deleteDatabase(context: Context, databaseName: String) : Exception?
    {
        val dbPath = this.getDatabaseFullName(context, databaseName)
        if (dbPath == null)
        {
            return Exception("Die Datenbank '$databaseName' konnte nicht gefunden werden.")
        }

        try
        {
            File(dbPath).delete()
        }
        catch (e: Exception)
        {
            return e
        }

        return null
    }

    fun isOnSDCard(context: Context, file: File): Boolean {
        val externalFilesDirs = context.getExternalFilesDirs(null)

        // Die erste Position ist meist der interne Speicher,
        // alles danach sind mögliche SD-Karten.
        val sdCardDirs = externalFilesDirs.drop(1).filterNotNull()

        return sdCardDirs.any { sdDir ->
            try {
                file.canonicalPath.startsWith(sdDir.canonicalPath)
            } catch (_: Exception) {
                false
            }
        }
    }

    fun getStorageRoots(context: Context): List<File> {
        return context.getExternalFilesDirs(null).filterNotNull()
    }
}
