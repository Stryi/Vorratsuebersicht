package de.stryi.vorratsuebersicht

import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.view.View
import android.widget.ArrayAdapter
import android.widget.Toast
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.core.content.FileProvider
import androidx.core.widget.doOnTextChanged
import androidx.lifecycle.lifecycleScope
import de.stryi.vorratsuebersicht.database.AndroidDatabase
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.databinding.SettingsActivityBinding
import de.stryi.vorratsuebersicht.tools.CsvExport
import de.stryi.vorratsuebersicht.tools.Logging
import de.stryi.vorratsuebersicht.tools.Settings
import de.stryi.vorratsuebersicht.tools.Tools
import de.stryi.vorratsuebersicht.tools.TwoLineAdapter
import kotlinx.coroutines.launch
import java.io.File
import java.io.FileOutputStream
import java.nio.file.Paths
import java.text.SimpleDateFormat
import java.time.LocalDateTime
import java.util.Date
import java.util.Locale

class SettingsActivity : AppCompatActivity() {

    var userCategoriesChanged = false

    private lateinit var binding: SettingsActivityBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = SettingsActivityBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setSupportActionBar(binding.SettingsAppBar)

        binding.SettingsAppBar.setNavigationOnClickListener { finish() }

        // -------------------------------------------
        // Artikelangaben (zusätzliche Kategorien)
        // -------------------------------------------

        // Frei definierte Kategorien zum Bearbeiten laden.
        val userCategories = Database.getSettingsString("USER_CATEGORIES")

        binding.SettingsCategories.setText(userCategories)
        binding.SettingsCategories.doOnTextChanged { _, _, _, _ ->
            userCategoriesChanged = true
        }
        binding.SettingsCategories.setOnFocusChangeListener { _, hasFocus ->
            if (!hasFocus) {
                saveUserDefinedCategories()
            }
        }

        var defaultCategory = Database.getSettingsString("DEFAULT_CATEGORY")
        if (defaultCategory.isNullOrEmpty())
        {
            defaultCategory = this.resources.getString(R.string.ArticleCatagoryDefault)
        }

        binding.SettingsDefaultCategory.text = defaultCategory
        binding.SettingsButtonSelectDefaultCategory.setOnClickListener { this.selectDefaultCategory() }

        // Addition Categories
        // Standard Kategorie (in der Datenbank ablegen, da abhängig der Datenbank)

        // -------------------------------------------
        // Aktuelle Datenbank
        // -------------------------------------------

        this.showDatabaseInfo()
        binding.SettingsButtonDatabaseShare.setOnClickListener     { this.onBackupShareClick() }

        // -------------------------------------------
        // Backup und Restore
        // -------------------------------------------

        binding.SettingsButtonBackupToFile.setOnClickListener    { this.onBackupExportClick() }
        binding.SettingsButtonRestoreFromFile.setOnClickListener {  this.onRestoreBackupClick()}


        this.showLastBackupDay()

        this.showDatabaseChangeCounter()

        binding.SettingsButtonAskForBackup.isChecked = Settings.getBoolean("AskForBackup", true)
        binding.SettingsButtonAskForBackup.setOnCheckedChangeListener { _, _ -> this.onAskAskForBackupClick() }

        // -------------------------------------------
        // Lagerverwaltung
        // -------------------------------------------

        binding.SettingsButtonDeleteStorageItems.setOnClickListener { this.deleteStorageItemsClick() }
        binding.SettingsButtonRenameeStorageMame.setOnClickListener { this.renameStorageNameClick() }

        // -------------------------------------------
        // Datenbank verwalten
        // -------------------------------------------

        binding.SettingsButtonCompress.setOnClickListener { this.compressDatabase()  }
        binding.SettingsButtonRepair.setOnClickListener   { this.repairDatabase()    }

        binding.SettingsButtonDatabaseNew.setOnClickListener    { this.buttonNewDbClick()}
        binding.SettingsButtonDatabaseImport.setOnClickListener { this.buttonImportDbClick() }
        binding.SettingsButtonDatabaseRename.setOnClickListener { this.buttonRenameDBClick() }
        binding.SettingsButtonMove.setOnClickListener           { this.moveDatabase()      }
        binding.SettingsButtonDatabaseDelete.setOnClickListener { this.buttonDeleteDbClick() }

        // EAN Scann (Benutze Frontkamera)
        binding.SettingsButtonEANScanFrontCamera.isChecked = Settings.getBoolean("UseFrontCameraForEANScan", false)
        binding.SettingsButtonEANScanFrontCamera.setOnCheckedChangeListener { _, _ -> this.onEANScanFrontCameraChecked() }

        // -------------------------------------------
        // Testdatenbank
        // -------------------------------------------

        binding.SettingsButtonRestoreSampleDb.setOnClickListener { this.restoreSampleDatabase() }
        binding.SettingsButtonRestoreDb0.setOnClickListener      { this.restoreEmptySampleDatabase() }

        // -------------------------------------------
        // CSV Export
        // -------------------------------------------

        binding.SettingsButtonCsvExportArticles.setOnClickListener     { this.buttonCsvExportArticlesClick() }
        binding.SettingsButtonCsvExportStorageItems.setOnClickListener { this.buttonCsvExportStorageItemsClick() }

        val csvSeparatorType = Settings.getInt("CsvExportSeparator", 1)

        binding.SettingsCSVSeparatorComma.isChecked     = csvSeparatorType == 1
        binding.SettingsCSVSeparatorSemicolon.isChecked = csvSeparatorType == 2
        binding.SettingsCSVSeparatorTab.isChecked       = csvSeparatorType == 3

        binding.SettingsCSVSeparatorComma.setOnClickListener     { this.csvSeparatorTypeClick() }
        binding.SettingsCSVSeparatorSemicolon.setOnClickListener { this.csvSeparatorTypeClick() }
        binding.SettingsCSVSeparatorTab.setOnClickListener       { this.csvSeparatorTypeClick() }

        // -------------------------------------------
        // Sonstiges
        // -------------------------------------------

        // OpenFoodFacts
        val showCostsMessage = Settings.getBoolean("ShowOpenFoodFactsInternetCostsMessage", true)
        binding.SettingsButtonShowOFFCostMessage.isChecked = showCostsMessage
        binding.SettingsButtonShowOFFCostMessage.setOnCheckedChangeListener { _, checked ->
            this.switchCostMessageClick(checked) }

        // Bilder Komprimieren
        // Müssenimmer komprimiert werden, da ansonsten sie nicht aus der Datenbank
        // wieder geladen werden können (Fehler: Row too big to fit into CursorWindow)
        val compressMode = Settings.getInt("CompressPicturesMode", 2)

        binding.SettingsButtonCompressPicturesSmall.isChecked = compressMode == 1
        binding.SettingsButtonCompressPicturesSmall.setOnClickListener { this.CompressModeClick(1) }

        binding.SettingsButtonCompressPicturesMiddle.isChecked = compressMode == 2
        binding.SettingsButtonCompressPicturesMiddle.setOnClickListener { this.CompressModeClick(2) }

        binding.SettingsButtonCompressPicturesBig.isChecked = compressMode == 3
        binding.SettingsButtonCompressPicturesBig.setOnClickListener { this.CompressModeClick(3) }

        binding.SettingsButtonCompressPicturesHuge.isChecked = compressMode == 4
        binding.SettingsButtonCompressPicturesHuge.setOnClickListener { this.CompressModeClick(4) }


        // -------------------------------------------
        // Support
        // -------------------------------------------

        // Version
        binding.SettingsButtonVersion.text = this.getApplicationVersion(this)

        // LOG Datei
        binding.SettingsLogFile.text = Tools.getLogFileName()
        binding.SettingsLogFile.setOnClickListener { this.testException() }

        // LOG Datei anzeigen / Senden
        binding.SettingsButtonShowLogFile.setOnClickListener { this.buttonShowLogFileClick() }

        // Lizenz
        binding.SettingsButtonLicenses.setOnClickListener { this.buttonLicenseClick() }

        val createBackup = intent.getBooleanExtra("CreateBackup", false)
        if (createBackup)
        {
            this.onBackupExportClick()
        }
    }

    private fun CompressModeClick(i: Int)
    {
        Settings.putInt("CompressPicturesMode", i)
    }

    private fun switchCostMessageClick(checked: Boolean)
    {
        Settings.putBoolean("ShowOpenFoodFactsInternetCostsMessage", checked)
    }

    private fun onAskAskForBackupClick() {
        val askForBackup = binding.SettingsButtonAskForBackup.isChecked
        Settings.putBoolean("AskForBackup", askForBackup)
    }

    fun showDatabaseChangeCounter()
    {
        val changesSinceLastBackup = Database.getChangeCounter()
        var changesText = resources.getString(R.string.Settings_ChangesSinceLastBackup)
        changesText = String.format(changesText, Tools.formatNumber(changesSinceLastBackup))

        binding.SettingsChangesSinceLastBackup.text = changesText
    }

    private fun showLastBackupDay()
    {
        val lastBackupDay = Database.getSettingsDateTime("LAST_BACKUP_TIME")
        val text = resources.getString(R.string.Settings_LastBackupOn)

        binding.SettingsLastBackupDay.text = String.format(text, Tools.toHumanText(lastBackupDay))
    }

    override fun finish() {
        saveUserDefinedCategories()
        super.finish()
    }

    private fun selectDefaultCategory()
    {
        // Ggf. die Änderungen der Kategorien abspeichern
        saveUserDefinedCategories()

        // Fest definierte Kategorien + frei definierten zur Auswahl laden
        val defaultCategories = resources.getStringArray(R.array.ArticleCatagories)
        val userCategorrieList = Database.getSettingsList("USER_CATEGORIES")

        // Einträge zusammenführen und sortieren
        val categories = (defaultCategories + userCategorrieList).sortedWith(String.CASE_INSENSITIVE_ORDER)

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            categories)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Settings_DefaultCategory)
        builder.setNegativeButton(R.string.App_Cancel) { _, _ -> }
        builder.setAdapter(adapter) { _, which ->
            val defaultCategory = categories[which]
            binding.SettingsDefaultCategory.text = defaultCategory
            Database.setSettings("DEFAULT_CATEGORY", defaultCategory)
        }
        builder.show()
    }

    fun saveUserDefinedCategories()
    {
        if (!userCategoriesChanged)
            return

        val categories = binding.SettingsCategories.text.toString()
        Database.setSettings("USER_CATEGORIES", categories.trim())
        userCategoriesChanged = false
    }

    // -------------------------------------------
    // Backup und Restore
    // -------------------------------------------

    private var pendingExportDbName: String? = null

    fun onBackupExportClick() {
        this.saveUserDefinedCategories()
        pendingExportDbName = Database.getDatabasePath()
        val currentDateAsString = getCurrentDateAsString()
        val currentDatabaseName = Database.getDatabaseName()
        val exportName = currentDatabaseName + "_" + currentDateAsString + ".VueBak"
        createBackupFileLauncher.launch(exportName)
    }

    val createBackupFileLauncher =
        registerForActivityResult(ActivityResultContracts.CreateDocument("application/octet-stream")) { uri ->
            if (uri != null) {
                val dbPath = File(pendingExportDbName!!)

                Database.setSettingsDateTime("LAST_BACKUP_TIME", LocalDateTime.now())
                Database.setSettingsDate    ("LAST_BACKUP",      LocalDateTime.now())
                Database.resetChangeCounter()

                contentResolver.openOutputStream(uri)?.use { output ->
                    dbPath.inputStream().use { input ->
                        input.copyTo(output)
                    }
                }
                Toast.makeText(this, "Backup der Datenbank gespeichert!", Toast.LENGTH_LONG).show()
                this.showLastBackupDay()
                this.showDatabaseChangeCounter()
            }
        }

    private fun getCurrentDateAsString(): String {
        val formatter = SimpleDateFormat("yyyy-MM-dd HH.mm.ss", Locale.getDefault())
        return formatter.format(Date())
    }

    private fun onBackupShareClick()
    {
        this.saveUserDefinedCategories()

        val databasePath = Database.getDatabasePath()
        if (databasePath == null)
            return

        val file = File(databasePath)
        val uri = FileProvider.getUriForFile(this, "de.stryi.vorratsuebersicht.provider", file)

        val shareIntent = Intent().apply {
            action = Intent.ACTION_SEND
            putExtra(Intent.EXTRA_STREAM, uri)
            type = "application/octet-stream"
            addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
        }
        startActivity(Intent.createChooser(shareIntent, "Datenbank teilen"))
    }

    private fun onRestoreBackupClick()
    {
        // Datei *.vuebak auswählen
        backupPickerLauncher.launch(arrayOf("*/*"))
    }

    private val backupPickerLauncher =
        registerForActivityResult(ActivityResultContracts.OpenDocument()) { uri ->
            if (uri != null) {
                restoreDatabaseFromFile(uri)
            }
        }

    // -------------------------------------------
    // Lagerverwaltung
    // -------------------------------------------

    private fun deleteStorageItemsClick()
    {
        val storageNames = Database.getStorageNames(true)
        val checkedItems = BooleanArray(storageNames.size)

        if (storageNames.isEmpty())
        {
            Toast.makeText(this, "Keine Lagernamen vorhanden.", Toast.LENGTH_SHORT).show()
            return
        }

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Settings_DeleteStorageItemsSelection)

        builder.setMultiChoiceItems(storageNames.toTypedArray(), checkedItems) { _, which, isChecked ->
            checkedItems[which] = isChecked
        }

        builder.setNegativeButton(R.string.App_Cancel) { _, _ -> }

        builder.setPositiveButton(R.string.App_DeleteBig) { dialog, _ ->

            val selectedStorages = storageNames.filterIndexed { index, _ ->
                checkedItems[index]
            }

            for (storage in selectedStorages) {
                Database.deleteStorageItems(storage)
            }

            if (selectedStorages.isNotEmpty()) {
                Tools.showMessage(
                    this,
                    "Alle Artikel aus ${selectedStorages.size} Lager(n) wurden gelöscht."
                )
            }

            dialog.dismiss()
        }

        builder.show()
    }

    private fun renameStorageNameClick()
    {
        val storageNames = Database.getStorageNames()

        if (storageNames.isEmpty())
        {
            Toast.makeText(this, "Keine Lagernamen vorhanden.", Toast.LENGTH_SHORT).show()
            return
        }

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            storageNames)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Settings_RenameStorageName)
        builder.setAdapter(adapter) { _, which ->
            renameStorageName(storageNames[which])
        }
        builder.show()
    }

    private fun renameStorageName(oldStorageName: String)
    {
        lifecycleScope.launch {

            val newStorageName = Tools.askForText(
                this@SettingsActivity,
                resources.getString(R.string.Settings_RenameStorageName),
                resources.getString(R.string.Settings_EnterNewStorageName),
                oldStorageName
            )

            if (newStorageName.isNullOrEmpty()) {
                return@launch
            }

            if (newStorageName == oldStorageName)
                return@launch

            val storageNames = Database.getStorageNames()
            if (storageNames.any { it.equals(newStorageName, ignoreCase = true) }) {

                val message =
                    "Die Lagername '$newStorageName' existiert bereits. " +
                    "Sollen die Lagerbestände '$oldStorageName' und '$newStorageName' zusammengefhrt werden?"

                val question = AlertDialog.Builder(this@SettingsActivity, R.style.MyAlertDialogTheme)
                question.setMessage(message)
                question.setNegativeButton(R.string.App_No) { _, _ -> }
                question.setPositiveButton(R.string.App_Yes) { _, _ ->
                    Database.renameStorageName(oldStorageName, newStorageName)
                }
                question.show()

                return@launch
            }

            Database.renameStorageName(oldStorageName, newStorageName)
        }
    }

    // -------------------------------------------
    // Aktuelle Datenbank
    // -------------------------------------------

    private fun showDatabaseInfo()
    {
        binding.SettingsButtonDatabasePath.text = Database.getDatabasePath()

        val databaseFileSize = Database.getDatabaseSize()
        val fileInformation = resources.getString(R.string.Settings_Datenbank)
        binding.SettingsButtonDatabasePath.text = String.format(
            fileInformation,
            Database.getDatabasePath(),
            Tools.toFuzzyByteString(databaseFileSize),
            Tools.formatNumber(databaseFileSize.toInt()))
    }

    // -------------------------------------------
    // Datenbank verwalten
    // -------------------------------------------

    fun compressDatabase()
    {
        this.createCompressProgressBar()

        Thread {
            try {
                Database.compressDatabase()
            }
            catch (e: Exception) {
                runOnUiThread {
                    Tools.showMessage(this, e.message!!)
                    this.hideCompressProgressBar()
                    this.showDatabaseInfo()
                }
            }
            runOnUiThread {
                this.hideCompressProgressBar()
                this.showDatabaseInfo()
                Toast.makeText(this, "Datenbank komprimiert!", Toast.LENGTH_LONG).show()
            }
        }.start()
    }

    fun repairDatabase() {
        this.createCompressProgressBar()

        var checkResult : String? = null

        Thread {
            try {
                checkResult =  Database.repairDatabase()
            }
            catch (e: Exception) {
                runOnUiThread {
                    Tools.showMessage(this, e.message!!)
                    this.hideCompressProgressBar()
                }
            }
            runOnUiThread {
                this.hideCompressProgressBar()
                this.showDatabaseInfo()
                Tools.showMessage(this, checkResult ?: "Keine Information")
            }
        }.start()
    }

    fun moveDatabase() {
        val databases = AndroidDatabase.loadDatabaseFileListSafe(this)

        if (databases.isEmpty()) {
            Toast.makeText(this, "Keine Datenbanken gefunden.", Toast.LENGTH_SHORT).show()
            return
        }

        val items = databases.map { file ->
            val size = Tools.toFuzzyByteString(file.length())
            val name = file.nameWithoutExtension
            var info = if (AndroidDatabase.isOnSDCard(this, file)) resources.getString(R.string.Settings_SdCard) else resources.getString(R.string.Settings_InternalStorage)
            info += ", $size"
            name to info
        }

        val adapter = TwoLineAdapter(this, items)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Settings_DatabaseMove)
        builder.setAdapter(adapter) { _, which ->
            confirmMoveDatabase(databases[which])
        }
        builder.show()
    }

    private fun confirmMoveDatabase(currentFile: File) {
        val storageRoots = AndroidDatabase.getStorageRoots(this)

        if (storageRoots.size < 2) {
            Tools.showWarning(this, "Keine SD-Karte gefunden oder nur ein Speicher verfügbar.")
            return
        }

        // Find which root we are currently on
        val currentRootIndex = storageRoots.indexOfFirst { root ->
            try {
                currentFile.canonicalPath.startsWith(root.canonicalPath)
            } catch (_: Exception) {
                false
            }
        }

        if (currentRootIndex == -1) {
            Tools.showWarning(this, "Speicherort der Datenbank konnte nicht ermittelt werden.")
            return
        }

        // Toggle between roots (assuming 0 is internal and 1 is SD)
        val targetRootIndex = if (currentRootIndex == 0) 1 else 0
        val targetRoot = storageRoots[targetRootIndex]

        val fromText = resources.getString(if (currentRootIndex == 0) R.string.Settings_InternalStorage else R.string.Settings_SdCard)
        val toText = resources.getString(if (targetRootIndex == 0) R.string.Settings_InternalStorage else R.string.Settings_SdCard)

        val message = resources.getString(R.string.Settings_DatabaseMove_ConfirmMessage, currentFile.nameWithoutExtension, fromText, toText)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Settings_DatabaseMove)
        builder.setMessage(message)
        builder.setNegativeButton(R.string.App_Cancel) { _, _ -> }
        builder.setPositiveButton(R.string.App_Ok) { _, _ ->
            performMoveDatabase(currentFile, targetRoot)
        }
        builder.show()
    }

    private fun performMoveDatabase(currentFile: File, targetDir: File) {
        val targetFile = File(targetDir, currentFile.name)

        if (targetFile.exists()) {
            Tools.showWarning(this, "Die Datei '${currentFile.name}' existiert bereits am Zielort.")
            return
        }

        val activeDatabasePath = Database.getDatabasePath()
        val isActiveDatabase = activeDatabasePath != null && File(activeDatabasePath).canonicalPath == currentFile.canonicalPath

        try {
            if (isActiveDatabase) {
                Database.closeDatabase()
            }

            currentFile.inputStream().use { input ->
                targetFile.outputStream().use { output ->
                    input.copyTo(output)
                }
            }

            if (targetFile.exists() && targetFile.length() == currentFile.length()) {
                currentFile.delete()
                
                if (isActiveDatabase) {
                    val error = Database.init(targetFile.absolutePath)
                    if (error == null) {
                        showDatabaseInfo()
                        Tools.showMessage(this, "Datenbank wurde verschoben.")
                    } else {
                        Tools.showWarning(this, "Fehler beim Öffnen der verschobenen Datenbank: $error")
                    }
                } else {
                    Tools.showMessage(this, "Datenbank '${currentFile.nameWithoutExtension}' wurde verschoben.")
                }
            } else {
                if (isActiveDatabase) {
                    Database.init(currentFile.absolutePath)
                }
                Tools.showWarning(this, "Fehler beim Kopieren der Datenbank.")
            }
        } catch (e: Exception) {
            if (isActiveDatabase && activeDatabasePath != null) {
                Database.init(activeDatabasePath)
            }
            Tools.showException(this, e, null, "Fehler beim Verschieben der Datenbank.")
        }
    }

    fun buttonNewDbClick()
    {
        lifecycleScope.launch {
            val newDatabaseName = Tools.askForText(this@SettingsActivity,
                resources.getString(R.string.Settings_DatabaseNewDialogTitle),
                resources.getString(R.string.Settings_DatabaseNewDialogMessage),
                "")

            if (newDatabaseName.isNullOrEmpty()) {
                return@launch
            }

            if (AndroidDatabase.isDatabaseExists(this@SettingsActivity, newDatabaseName))
            {
                Tools.showWarning(this@SettingsActivity, "Die Datenbank '$newDatabaseName' existiert bereits.")
                return@launch
            }

            val storageRoots = AndroidDatabase.getStorageRoots(this@SettingsActivity)
            if (storageRoots.size > 1) {
                val items = storageRoots.map { file ->
                    val isSD = AndroidDatabase.isOnSDCard(this@SettingsActivity, file)
                    val title = if (isSD) "SD-Karte" else "Interner Speicher"
                    val subtitle = file.absolutePath
                    title to subtitle
                }

                val adapter = TwoLineAdapter(this@SettingsActivity, items)

                val builder = AlertDialog.Builder(this@SettingsActivity, R.style.MyAlertDialogTheme)
                builder.setTitle("Speicherort auswählen")
                builder.setAdapter(adapter) { _, which ->
                    performCreateDatabase(newDatabaseName, storageRoots[which])
                }
                builder.show()
            } else {
                performCreateDatabase(newDatabaseName, storageRoots.firstOrNull())
            }
        }
    }

    private fun performCreateDatabase(newDatabaseName: String, targetDir: File?) {
        val exception = AndroidDatabase.createLocalizedDatabaseFromAsset(this@SettingsActivity,
            AndroidDatabase.SQLITE_FILENAME_NEW,
            "$newDatabaseName.db3",
            false,
            targetDir)

        Tools.showException(this@SettingsActivity,
            exception,
            "Die Datenbank '$newDatabaseName' wurde erstellt.",
            "Die Datenbank '$newDatabaseName' konnte nicht erstellt werden.")
    }

    fun buttonRenameDBClick()
    {
        val databaseList = AndroidDatabase.loadDatabaseFileListSafe(this)
        val currentDatabaseName = Database.getDatabaseName()
        val fileListArray = mutableListOf<String>()

        for (databaseName in databaseList)
        {
            // Aktuelle Datenbank kann nicht ausgewählt werden.
            if (databaseName.nameWithoutExtension == currentDatabaseName)
            {
                continue
            }
            fileListArray.add(databaseName.nameWithoutExtension)
        }

        if (fileListArray.isEmpty()) {
            Toast.makeText(this, "Keine Datenbanken vorhanden.", Toast.LENGTH_SHORT).show()
            return
        }

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            fileListArray)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Settings_DatabaseRenameDialogTitle)
        builder.setAdapter(adapter) { _, which ->
            renameDatabase(fileListArray[which])
        }
        builder.show()
    }

    fun renameDatabase(oldDatabaseName: String)
    {
        lifecycleScope.launch {

            val newDatabaseName = Tools.askForText(
                this@SettingsActivity,
                resources.getString(R.string.Settings_DatabaseRenameDialogTitle),
                resources.getString(R.string.Settings_DatabaseRenameDialogMessage),
                oldDatabaseName
            )

            if (newDatabaseName.isNullOrEmpty()) {
                return@launch
            }

            val oldDatabaseFileName = AndroidDatabase.getDatabaseFullName(this@SettingsActivity, oldDatabaseName)
            if (oldDatabaseFileName == null)
            {
                Tools.showWarning(this@SettingsActivity, "Die Datenbank '$oldDatabaseName' konnte nicht gefunden werden.")
                return@launch
            }

            if (AndroidDatabase.isDatabaseExists(this@SettingsActivity, newDatabaseName))
            {
                Tools.showWarning(this@SettingsActivity, "Die Datenbank '$newDatabaseName' existiert bereits.")
                return@launch
            }

            val result = AndroidDatabase.renameDatabase(oldDatabaseFileName, newDatabaseName)
            if (!result)
            {
                Tools.showWarning(this@SettingsActivity, "Die Datenbank konnte nicht umbenannt werden.")
                return@launch
            }
        }
    }

    fun buttonImportDbClick()
    {
        filePickerLauncher.launch(arrayOf("*/*"))
    }

    private val filePickerLauncher =
        registerForActivityResult(ActivityResultContracts.OpenDocument()) { uri ->
            if (uri != null) {
                importDatabaseFromFile(uri)
            }
        }

    fun importDatabaseFromFile(uri: Uri) {
        val fileName = Tools.getFileNameFromUri(this, uri)

        val databaseName = Tools.getBackupDatabaseName(fileName)

        // "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
        val dbPath = this.getExternalFilesDir(null)

        lifecycleScope.launch {
            var newDatabaseName = Tools.askForText(this@SettingsActivity,
                resources.getString(R.string.Settings_DatabaseImport),
                resources.getString(R.string.Settings_DatabaseImportName),
                databaseName)

            if (newDatabaseName.isNullOrEmpty()) {
                return@launch
            }

            if (AndroidDatabase.isDatabaseExists(this@SettingsActivity, newDatabaseName))
            {
                Tools.showWarning(this@SettingsActivity, "Die Datenbank '$newDatabaseName' existiert bereits.")
                return@launch
            }

            val inputStream = contentResolver.openInputStream(uri)

            newDatabaseName = newDatabaseName.trimEnd()
            newDatabaseName += ".db3"

            // Hole den Pfad zum App-Datenbankordner
            val dbFile = File(dbPath, newDatabaseName) // legt Datei direkt im DB-Ordner an

            inputStream.use { input ->
                FileOutputStream(dbFile).use { output ->
                    input!!.copyTo(output)
                }
            }
        }
    }
    fun restoreDatabaseFromFile(uri: Uri) {
        val fileName = Tools.getFileNameFromUri(this, uri)

        val databaseName = Tools.getBackupDatabaseName(fileName)

        val currentDatabase = Database.getDatabasePath()
        if (currentDatabase == null)
            return

        lifecycleScope.launch {
            var newDatabaseName = Tools.askForText(this@SettingsActivity,
                resources.getString(R.string.Settings_DatabaseRestore),
                resources.getString(R.string.Settings_DatabaseRestoreName),
                databaseName)

            if (newDatabaseName.isNullOrEmpty()) {
                return@launch
            }

            newDatabaseName = newDatabaseName.trimEnd()
            newDatabaseName += ".db3"

            // Im Verzeichnis der aktuellen Datenbank wiederherstellen
            var dbFile = File(currentDatabase).parent
            dbFile = Paths.get(dbFile, newDatabaseName).toString() // legt Datei direkt im DB-Ordner an

            val inputStream = contentResolver.openInputStream(uri)

            inputStream.use { input ->
                FileOutputStream(dbFile).use { output ->
                    input!!.copyTo(output)
                }
            }
        }
    }


    private fun onEANScanFrontCameraChecked()
    {
        Settings.putBoolean("UseFrontCameraForEANScan", binding.SettingsButtonEANScanFrontCamera.isChecked)
    }

    private fun buttonDeleteDbClick() {

        val databaseList = AndroidDatabase.loadDatabaseFileListSafe(this)
        val currentDatabaseName = Database.getDatabaseName()

        databaseList.removeIf { it.nameWithoutExtension == currentDatabaseName }

        val fileListArray = databaseList.map { it.nameWithoutExtension }

        if (fileListArray.isEmpty())
        {
            Toast.makeText(this, "Keine Datenbanken zum Löschen vorhanden.", Toast.LENGTH_LONG).show()
            return
        }

        var selectedIndex = -1

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Settings_DeleteDatabase)
        builder.setNegativeButton(R.string.App_Cancel) { _, _ -> }
        builder.setSingleChoiceItems(fileListArray.toTypedArray(), -1) { _, which ->
            selectedIndex = which
        }

        builder.setPositiveButton(R.string.App_DeleteBig) { dialog, _ ->
            if (selectedIndex >= 0) {
                val databaseToDelete = fileListArray[selectedIndex]
                val exception = AndroidDatabase.deleteDatabase(this, databaseToDelete)
                Tools.showException(this,
                    exception,
                    "Die Datenbank '$databaseToDelete' wurde gelöscht.",
                    "Die Datenbank '$databaseToDelete' konnte nicht gelöscht werden.")
            }
            dialog.dismiss()
        }

        builder.show()
    }


    // -------------------------------------------
    // Testdatenbank
    // -------------------------------------------

    fun restoreSampleDatabase()
    {
        val currentDatabase = Database.getDatabasePath()
        if (currentDatabase == null)
            return

        Database.closeDatabase()

        val exception = AndroidDatabase.createLocalizedDatabaseFromAsset(this,
            AndroidDatabase.SQLITE_FILENAME_DEMO,
            AndroidDatabase.SQLITE_FILENAME_DEMO,
            true)

        Database.init(currentDatabase)

        if (exception == null)
        {
            Database.prepareTestDatabase(this)
        }

        Tools.showException(this,
            exception,
            "Die Demo Datenbank wurde zurückgesetzt.",
            "Die Demo Datenbank konnte nicht zurückgesetzt werden.")
    }

    fun restoreEmptySampleDatabase()
    {
        val currentDatabase = Database.getDatabasePath()
        if (currentDatabase == null)
            return

        Database.closeDatabase()

        val exception = AndroidDatabase.createLocalizedDatabaseFromAsset(this,
            AndroidDatabase.SQLITE_FILENAME_NEW,
            AndroidDatabase.SQLITE_FILENAME_DEMO,
            true)

        Database.init(currentDatabase)

        Tools.showException(this,
            exception,
            "Eine leere Demo Datenbank wurde erstellt.",
            "Eine leere Demo Datenbank konnte nicht erstellt werden.")
    }


    private fun csvSeparatorTypeClick()
    {
        if (binding.SettingsCSVSeparatorComma.isChecked)
            Settings.putInt("CsvExportSeparator", 1)

        if (binding.SettingsCSVSeparatorSemicolon.isChecked)
            Settings.putInt("CsvExportSeparator", 2)

        if (binding.SettingsCSVSeparatorTab.isChecked)
            Settings.putInt("CsvExportSeparator", 3)
    }

    private fun buttonCsvExportStorageItemsClick()
    {
        try {
            CsvExport.exportStorageItems(this, this.getCsvSeparator())
        }
        catch (e: Exception) {
            Tools.showException(this, e, null, "Fehler beim Export der Lagerbestände.")
        }
    }

    private fun buttonCsvExportArticlesClick()
    {
        try {
            CsvExport.exportArticles(this, this.getCsvSeparator())
        }
        catch (e: Exception) {
            Tools.showException(this, e, null, "Fehler beim Export der Artikel.")
        }
    }

    fun getCsvSeparator(): String
    {
        val csvSeparatorType = Settings.getInt("CsvExportSeparator", 1)
        when (csvSeparatorType)
        {
            1 -> return ","
            2 -> return ";"
            3 -> return "\t"
            else -> return ","
        }
    }

    fun getApplicationVersion(context: Context): String
    {
        var versionInfo = ""

        try {
            val packageInfo = context.packageManager.getPackageInfo(context.packageName, 0)
            val versionName = packageInfo.versionName
            val versionCode = packageInfo.longVersionCode

            versionInfo += "Version $versionName"
            versionInfo += " (Code Version $versionCode)"
        } catch (e: Exception) {
            Tools.TRACE(e.message)
            Tools.TRACE(e.stackTraceToString())
        }

        try {
            val abis = Build.SUPPORTED_ABIS
            if (abis.isNotEmpty()) {
                val prozessoren = abis.joinToString(",")
                versionInfo += "\nProzessor: $prozessoren"
            }
        } catch (e: Exception) {
            Tools.TRACE(e.message)
            Tools.TRACE(e.stackTraceToString())
        }

        return versionInfo
    }

    private fun buttonShowLogFileClick()
    {
        val intent = Intent(this, LogViewerActivity::class.java)
        startActivity(intent)
    }

    private fun buttonSendLogFileClick()
    {
        val message = resources.getString(R.string.Settings_SendLogFileMessage)

        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setMessage(message)

        dialog.setPositiveButton(resources.getString(R.string.App_Yes)) { _, _ ->

            val context: Context = applicationContext
            val packageInfo = context.packageManager.getPackageInfo(context.packageName, 0)
            val versionName = packageInfo.versionName
            val versionCode = packageInfo.longVersionCode

            val text = StringBuilder()
            text.append("Version $versionName (Code Version ${versionCode})\n")
            text.append("Current Database: ${Database.getDatabasePath()}\n")
            text.append("Android Version: ${Build.VERSION.RELEASE}\n")
            text.append("Android SDK: ${Build.VERSION.SDK_INT}\n")
            text.append("Manufacturer: ${Build.MANUFACTURER}\n")
            text.append("Modell: ${Build.MODEL}\n")
            text.append("CurrentCulture: ${Locale.getDefault().displayName}\n")
            text.append("CurrentUICulture: ${Locale.getDefault().displayName}\n")

            text.appendLine()
            text.appendLine(Logging.getLogFileText(this))

            android.util.Log.d("TRACE", text.toString())

            val subject = "Vue_LOG_" +
                    SimpleDateFormat("yyyy-MM-dd_HH.mm.ss", Locale.getDefault()).format(Date())

            val emailIntent = Intent(Intent.ACTION_SEND).apply {
                putExtra(Intent.EXTRA_EMAIL, arrayOf("cstryi@freenet.de"))
                putExtra(Intent.EXTRA_SUBJECT, subject)
                putExtra(Intent.EXTRA_TEXT, text.toString())
                type = "text/plain"
            }

            startActivity(
                Intent.createChooser(
                    emailIntent,
                    resources.getString(R.string.Settings_SendLogFile)
                )
            )
        }

        dialog.setNegativeButton(resources.getString(R.string.App_No)) { _, _ -> }
        dialog.create().show()
    }

    private fun testException()
    {
        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setTitle("Absturz Test")
        dialog.setMessage("Dieser Testabsturz dienst nur für interne Tests des Entwicklers und wird im Normalfall nicht gebraucht (bitte auf 'Abbrechen' klicken).")
        dialog.setPositiveButton(R.string.App_Cancel) { _, _ -> }
        dialog.setNegativeButton("ABSTURZ") { _, _ -> throw Exception("Das ist ein Testabsturz.")}
        dialog.setNeutralButton("LOG Dateien löschen") { _, _ -> Logging.deleteAllLogFiles(this) }
        dialog.show()
    }

    private fun buttonLicenseClick()
    {
        val license = Intent(this, LicenseActivity::class.java)
        startActivity(license)
    }

    fun createCompressProgressBar()
    {
        binding.ProgressBarCompress.visibility = View.VISIBLE
    }

    fun hideCompressProgressBar()
    {
        binding.ProgressBarCompress.visibility = View.INVISIBLE
    }
}
