package de.stryi.vorratsuebersicht.database

import android.content.Context
import android.net.Uri
import android.os.Environment
import android.provider.DocumentsContract
import de.stryi.vorratsuebersicht.R
import de.stryi.vorratsuebersicht.tools.Settings
import de.stryi.vorratsuebersicht.tools.Tools
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

        val sharedPath = Settings.getString("SharedDatabasePath", "")
        val sharedUriStr = Settings.getString("SharedDatabaseUri", "")

        val isSharedTarget = sharedUriStr.isNotEmpty() && targetDir != null &&
            try {
                val dirCanonical = targetDir.canonicalPath
                val sharedCanonical = File(sharedPath).canonicalPath
                dirCanonical == sharedCanonical || dirCanonical.startsWith(sharedCanonical)
            } catch (_: Exception) { false }

        var inputStream: InputStream? = null
        var outputStream: OutputStream? = null
        var result : Exception? = null

        try {
            inputStream = context.assets.open(assetsFileName)

            if (isSharedTarget) {
                try {
                    val treeUri = Uri.parse(sharedUriStr)
                    val docId = DocumentsContract.getTreeDocumentId(treeUri)
                    val docUri = DocumentsContract.buildDocumentUriUsingTree(treeUri, docId)

                    // Prüfen, ob Datei bereits existiert
                    val childrenUri = DocumentsContract.buildChildDocumentsUriUsingTree(treeUri, docId)
                    context.contentResolver.query(
                        childrenUri,
                        arrayOf(DocumentsContract.Document.COLUMN_DOCUMENT_ID, DocumentsContract.Document.COLUMN_DISPLAY_NAME),
                        null, null, null
                    )?.use { cursor ->
                        val nameIdx = cursor.getColumnIndex(DocumentsContract.Document.COLUMN_DISPLAY_NAME)
                        val idIdx = cursor.getColumnIndex(DocumentsContract.Document.COLUMN_DOCUMENT_ID)
                        while (cursor.moveToNext()) {
                            val name = cursor.getString(nameIdx)
                            if (name.equals(databaseFileName, ignoreCase = true)) {
                                if (overrideIfExists) {
                                    val existingId = cursor.getString(idIdx)
                                    val existingUri = DocumentsContract.buildDocumentUriUsingTree(treeUri, existingId)
                                    DocumentsContract.deleteDocument(context.contentResolver, existingUri)
                                } else {
                                    return Exception("Die Datenbank '$dbName' existiert bereits.")
                                }
                                break
                            }
                        }
                    }

                    val newDocUri = DocumentsContract.createDocument(
                        context.contentResolver,
                        docUri,
                        "application/octet-stream",
                        databaseFileName
                    )

                    if (newDocUri != null) {
                        outputStream = context.contentResolver.openOutputStream(newDocUri)
                    }
                } catch (e: Exception) {
                    Tools.TRACE("SAF creation failed: ${e.message}")
                }
            }

            if (outputStream == null) {
                if (dbFile.exists()) {
                    if (overrideIfExists) {
                        dbFile.delete()
                    } else {
                        return Exception("Die Datenbank '$dbName' existiert bereits.")
                    }
                }
                outputStream = dbFile.outputStream()
            }

            inputStream.copyTo(outputStream)
        } catch (e: Exception) {
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

    fun loadDatabaseFileListSafe(context: Context): MutableList<File>
    {
        val fileList = mutableListOf<File>()
        val roots = getStorageRoots(context)

        for (dir in roots)
        {
            if (!dir.exists() || !dir.isDirectory) continue

            val files = dir.listFiles() ?: continue
            for (file in files)
            {
                if (!file.name.endsWith("db3"))
                    continue

                if (!file.isFile)
                    continue

                val alreadyAdded = fileList.any {
                    try { it.canonicalPath == file.canonicalPath } catch (_: Exception) { false }
                }
                if (!alreadyAdded) {
                    fileList.add(file)
                }
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
        val roots = mutableListOf<File>()

        // "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
        // "/storage/0E0E-2316/Android/data/de.stryi.Vorratsuebersicht/files"
        // Internal and SD card dirs from system
        val externalDirs = context.getExternalFilesDirs(null).filterNotNull()
        roots.addAll(externalDirs)

        // Shared directory if configured
        val sharedPath = Settings.getString("SharedDatabasePath", "")
        if (sharedPath.isNotEmpty()) {
            val sharedDir = File(sharedPath)
            if (sharedDir.exists() && sharedDir.isDirectory) {
                val alreadyContains = roots.any {
                    try { it.canonicalPath == sharedDir.canonicalPath } catch (_: Exception) { false }
                }
                if (!alreadyContains) {
                    roots.add(sharedDir)
                }
            }
        }

        return roots
    }

    fun getStorageName(context: Context, file: File): String {
        val sharedPath = Settings.getString("SharedDatabasePath", "")
        if (sharedPath.isNotEmpty()) {
            try {
                val sharedDir = File(sharedPath)
                if (file.canonicalPath.startsWith(sharedDir.canonicalPath) ||
                    file.parentFile?.canonicalPath == sharedDir.canonicalPath) {
                    return context.getString(R.string.Settings_SharedDirectory)
                }
            } catch (_: Exception) {}
        }

        if (isOnSDCard(context, file)) {
            return context.getString(R.string.Settings_SdCard)
        }

        return context.getString(R.string.Settings_InternalStorage)
    }

    fun resolveTreeUriToFile(context: Context, uri: Uri): File? {
        try {
            if (uri.scheme == "file") {
                uri.path?.let { return File(it) }
            }

            if (uri.scheme == "content") {
                val docId = try {
                    DocumentsContract.getTreeDocumentId(uri)
                } catch (_: Exception) {
                    try {
                        DocumentsContract.getDocumentId(uri)
                    } catch (_: Exception) {
                        null
                    }
                }

                if (docId != null && docId.contains(":")) {
                    val split = docId.split(":", limit = 2)
                    val type = split[0]
                    val subPath = if (split.size > 1) split[1] else ""

                    if (type.equals("primary", ignoreCase = true)) {
                        val rootDir = Environment.getExternalStorageDirectory()
                        return if (subPath.isNotEmpty()) File(rootDir, subPath) else rootDir
                    } else {
                        val targetPath = if (subPath.isNotEmpty()) "/storage/$type/$subPath" else "/storage/$type"
                        val file = File(targetPath)
                        if (file.exists()) {
                            return file
                        }
                        val externalFilesDirs = context.getExternalFilesDirs(null).filterNotNull()
                        for (extDir in externalFilesDirs) {
                            if (extDir.absolutePath.contains(type)) {
                                val basePath = extDir.absolutePath.substringBefore("/Android/data")
                                val candidate = if (subPath.isNotEmpty()) File(basePath, subPath) else File(basePath)
                                if (candidate.exists()) return candidate
                            }
                        }
                        return file
                    }
                }

                val path = uri.path
                if (path != null) {
                    if (path.contains("/document/primary:")) {
                        val subPath = path.substringAfter("/document/primary:")
                        return File(Environment.getExternalStorageDirectory(), subPath)
                    }
                    if (path.contains("/tree/primary:")) {
                        val subPath = path.substringAfter("/tree/primary:")
                        return File(Environment.getExternalStorageDirectory(), subPath)
                    }
                }
            }
        } catch (e: Exception) {
            Tools.TRACE("Error resolving tree URI: ${e.message}")
        }
        return null
    }
}
