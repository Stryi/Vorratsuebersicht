package de.stryi.vorratsuebersicht

import android.Manifest
import android.annotation.SuppressLint
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Color
import android.os.Build
import android.os.Bundle
import android.util.Log
import android.view.Menu
import android.view.MenuItem
import android.widget.ArrayAdapter
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import de.stryi.vorratsuebersicht.databinding.MainActivityBinding
import de.stryi.vorratsuebersicht.database.AndroidDatabase
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.tools.AddToShoppingListDialog
import de.stryi.vorratsuebersicht.tools.Logging
import de.stryi.vorratsuebersicht.tools.PermissionHelper
import de.stryi.vorratsuebersicht.tools.Settings
import de.stryi.vorratsuebersicht.tools.Tools
import de.stryi.vorratsuebersicht.tools.Tools.TRACE
import de.stryi.vorratsuebersicht.tools.TwoLineAdapter
import java.io.File
import java.time.LocalDateTime
import java.util.Locale


class MainActivity : AppCompatActivity() {

    val cameraRequestCode = 101

    companion object {

        var IsGooglePlayPreLaunchTestMode: Boolean = false
        lateinit var appContext: Context
            private set
    }

    private lateinit var binding: MainActivityBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = MainActivityBinding.inflate(layoutInflater)
        setContentView(binding.root)
        setSupportActionBar(binding.MainAppBar)

        appContext = applicationContext

        Logging.InitializeLogFile(this)
        this.protocolAppInfo(this)

        // Globale unhandled Exceptions abfangen und in die LOG Datei protokollieren.
        val defaultHandler = Thread.getDefaultUncaughtExceptionHandler()

        Thread.setDefaultUncaughtExceptionHandler { thread, throwable ->
            TRACE("***** CRASH ${System.currentTimeMillis()}")
            TRACE(Log.getStackTraceString(throwable))

            // danach normalen Crash weiter auslösen
            defaultHandler?.uncaughtException(thread, throwable)
        }

        val firstRun = Settings.getBoolean("FirstRun", true)
        if (firstRun) {
            AndroidDatabase.restoreDatabasesFromResourcesOnStartup(this)
        }

        var lastSelectedDatabase = Settings.getString(
            "LastSelectedDatabase",
            AndroidDatabase.SQLITE_FILENAME_PROD
        )

        val databases = AndroidDatabase.loadDatabaseFileListSafe(this)

        if (databases.isNotEmpty()) {
            // Prüfen, ob der gespeicherte Pfad in der Liste vorkommt
            val exists = databases.any { it.absolutePath == lastSelectedDatabase }

            if (!exists) {
                lastSelectedDatabase = databases.first().absolutePath
            }
        }

        val exception = Database.init(lastSelectedDatabase)

        this.showDatabaseName(lastSelectedDatabase)
        this.showStorageInfoText(exception)

        if (false)
        {
            val settings = Intent(this, SettingsActivity::class.java)
            startActivity(settings)

            /*
            val internetDB = Intent(this, InternetDatabaseSearchActivity::class.java)
            internetDB.putExtra("EANCode", "8076800195057")
            startActivity(internetDB)

            val storageItem = Intent(this, StorageItemInventoryActivity::class.java)
            storageItem.putExtra("ArticleId", 4)
            startActivity(storageItem)
            finish()

             */
        }

        ShoppingItemListActivity.orderBy             = Settings.getInt("ShoppingListOrder", 1)
        ShoppingItemViewAdapter.sparseView           = Settings.getInt("ShoppingListViewType", 0)

        binding.contentLayout.MainText. setOnClickListener { this.articlesNearExpiryDate("ExpiryDateOnly") }
        binding.contentLayout.MainText1.setOnClickListener { this.articlesNearExpiryDate("WithExpiryDateOnly") }
        binding.contentLayout.MainText2.setOnClickListener { this.articlesNearExpiryDate("NearExpiryDateOnly") }

        // Auswahl nach Kategorien
        binding.contentLayout.MainButtonKategorie.setOnClickListener {
            this.showCategoriesSelection()
        }

        // Lagerbestand
        binding.contentLayout.MainButtonLagerbestand.setOnClickListener {
            val intent = Intent(this, StorageItemListActivity::class.java)
            startActivity(intent)
        }

        // Artikeldaten
        binding.contentLayout.MainButtonArtikeldaten.setOnClickListener {
            val intent = Intent(this, ArticleListActivity::class.java)
            startActivity(intent)
        }

        // Einkaufsliste
        binding.contentLayout.MainButtonShoppingList.setOnClickListener {
            val intent = Intent(this, ShoppingItemListActivity::class.java)
            startActivity(intent)
        }

        // Barcode scannen
        binding.contentLayout.MainButtonBarcode.setOnClickListener {
            PermissionHelper().requestPermission(
                this,
                Manifest.permission.CAMERA,
                cameraRequestCode)
            {
                openEanCodeScanner()
            }
        }

        this.ShowInfoAufTestdatenbank()

        Settings.putBoolean("FirstRun", false)

        this.askForCreateBackup()
    }

    override fun onResume() {
        super.onResume()
        showStorageInfoText()
    }

    private fun articlesNearExpiryDate(tag: String) {

        val storageitemList = Intent(this, StorageItemListActivity::class.java)
        storageitemList.putExtra("OrderByToConsumeDate", true)
        storageitemList.putExtra("FilterExpiryDate",    tag)
        startActivity(storageitemList)
    }

    private fun showCategoriesSelection() {
        val categories = Database.getCategoriesInUse()
        if (categories.isEmpty())
        {
            Toast.makeText(this, R.string.NoArticleCatagories, Toast.LENGTH_SHORT).show()
            return
        }

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            categories)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.ArticleCatagoriesSelect)
        builder.setAdapter(adapter) { _, which ->
            val intent = Intent(this, SubCategoryActivity::class.java)
            intent.putExtra("Category", categories[which])
            startActivity(intent)
        }
         builder.show()
     }

    fun askForCreateBackup()
    {
        /*
        // Einstellungen löschen
        */
        //Database.clearSettings("LAST_BACKUP")
        //Database.clearSettings("LAST_BACKUP_TIME")
        //Database.setSettingsDate    ("LAST_BACKUP",      LocalDateTime.of(2019,1,1, 12,55,13))
        //Database.setSettingsDateTime("LAST_BACKUP_TIME", LocalDateTime.of(2019,1,1, 12,55,13))
        //Database.setChangeCounter(10)
        //Settings.clear("BACKUP_NOT_TODAY")
        //Settings.putDate("BACKUP_NOT_TODAY", LocalDateTime.now().plusDays(-1).toLocalDate())

        if (AndroidDatabase.isTestDatabase())
            return

        val askForBackup = Settings.getBoolean("AskForBackup", true)
        if (!askForBackup)
            return

        val articleCount = Database.getArticleCount()
        if (articleCount < 5)
            return

        val changesCount = Database.getChangeCounter()
        if (changesCount == 0)
        {
            // Keine Änderungen seit dem letzten Backup.
            return
        }

        val lastBackupDay = Database.getSettingsDate("LAST_BACKUP_TIME")

        // Activate to test the Backup Message
        //lastBackupDay = new DateTime(2000, 02, 20);

        // Backup nur alle 7 Tage vorschlagen
        if ((lastBackupDay != null) && (lastBackupDay.plusDays(7) >= LocalDateTime.now().toLocalDate()))
            return

        // Heute nicht mehr fragen?
        val notToday = Settings.getDate("BACKUP_NOT_TODAY")
        if ((notToday != null) && (notToday == LocalDateTime.now().toLocalDate()))
            return

        var messageText = this.resources.getString(R.string.Main_CreateBackupNow)

        messageText += "\r\n\r\n"
        messageText +=
        String.format(this.resources.getString(R.string.Settings_LastBackupOn),
            Tools.toHumanString(lastBackupDay))

        messageText += "\r\n"
        messageText +=
            String.format(this.resources.getString(R.string.Settings_ChangesSinceLastBackup),
                Tools.formatNumber(changesCount))

        val message = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        message.setIcon(R.drawable.ic_launcher)
        message.setMessage(messageText)
        message.setPositiveButton(R.string.App_Yes) { _, _ ->
            val settingsActivity = Intent(this, SettingsActivity::class.java)
            settingsActivity.putExtra("CreateBackup", true)
            this.startActivity(settingsActivity)
        }
        message.setNegativeButton(R.string.App_Leter) { _, _ -> }
        message.setNeutralButton(R.string.App_NotToday) { _, _ ->
            Settings.putDate("BACKUP_NOT_TODAY", LocalDateTime.now().toLocalDate())
        }
        message.show()

    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.menu_main, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        when (item.itemId) {
            R.id.Main_Menu_Options -> {
                this.startActivity(Intent(this, SettingsActivity::class.java))
                return true
            }
            R.id.Main_Menu_SelectDatabase -> {
                this.switchDatabase()
                return true
            }
            else -> return false
        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String?>,
        grantResults: IntArray)
    {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)

        if (requestCode == cameraRequestCode){
            if (grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED){
                openEanCodeScanner()
            }
            else {
                Toast.makeText(this, "Kamera-Berechtigung nicht erteilt.", Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun openEanCodeScanner() {
        val eanScanFragment = EanCodeScan()
        eanScanFragment.onResult = { eanCode ->
            searchEANCode(eanCode)
        }
        eanScanFragment.show(supportFragmentManager, "EanCodeScan")
    }

    fun searchEANCode(eanCode: String)
    {
        TRACE("Scanned Barcode: %s", eanCode)

        val result = Database.getArticlesByEanCode(eanCode)
        if (result.isEmpty())
        {
            // Neuanlage Artikel
            val articleDetails = Intent(this, ArticleDetailsActivity::class.java)
            articleDetails.putExtra("EANCode", eanCode)
            startActivity(articleDetails)
            return
        }

        val selectDialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        selectDialog.setTitle(this.resources.getString(R.string.App_ChooseAction))

        if (result.count() == 1)          // Artikel eindeutig gefunden
        {
            val articleId = result[0].articleId

            var zusatzInfo = ""

            val quantityInStorage = Database.getArticleQuantityInStorage(articleId)
            if (quantityInStorage > 0.00)
            {
                zusatzInfo += "- Bestand:  %s".format(Tools.formatNumber(quantityInStorage))
            }

            val shoppingListQuantiy = Database.getShoppingListQuantiy(articleId, -1.00)
            if (shoppingListQuantiy != null && shoppingListQuantiy >= 0.00)
            {
                if (zusatzInfo.isNotEmpty()) zusatzInfo += "\n"
                zusatzInfo += "- Auf Einkaufsliste: %s".format(Tools.formatNumber(shoppingListQuantiy))
            }

            val actions = mutableListOf(
                this.resources.getString(R.string.Main_Button_Lagerbestand),
                this.resources.getString(R.string.Main_Button_Artikelangaben),
                this.resources.getString(R.string.Main_Button_AufEinkaufsliste))

            if (shoppingListQuantiy != null && shoppingListQuantiy >= 0.00)
            {
                actions.add(this.resources.getString(R.string.Main_Button_InLagerbestand))
            }

            if (zusatzInfo.isNotEmpty())
            {
                val info = TextView(this)
                info.setTextColor(Color.GRAY)
                info.setPadding(25,0,0,0)
                info.text = zusatzInfo

                selectDialog.setView(info)
            }

            val adapter = ArrayAdapter(
                this,
                R.layout.dialog_item,
                R.id.DialogItem_Text1,
                actions)

            selectDialog.setAdapter(adapter) { _, which ->
                when (which) {
                    0 -> { // Lagerbestand bearbeiten
                        val storageItem = Intent(this, StorageItemInventoryActivity::class.java)
                        storageItem.putExtra("ArticleId", articleId)
                        startActivity(storageItem)

                    }
                    1 -> {
                        // Artikelstamm bearbeiten
                        val articleDetails = Intent(this, ArticleDetailsActivity::class.java)
                        articleDetails.putExtra("ArticleId", articleId)
                        startActivity(articleDetails)
                    }
                    2 -> {
                        // Auf die Einkaufsliste
                        AddToShoppingListDialog.showDialog(this, articleId)
                    }
                    3 -> {
                        // Aus Einkaufsliste ins Lager
                        val storageInventory = Intent(this, StorageItemInventoryActivity ::class.java)
                        storageInventory.putExtra("ArticleId", articleId)
                        storageInventory.putExtra("EditMode", true)
                        storageInventory.putExtra("Quantity", shoppingListQuantiy)

                        startActivity(storageInventory)
                    }
                }
            }
            selectDialog.show()
            return
        }

        val actions = mutableListOf(
            this.resources.getString(R.string.Main_Button_LagerbestandListe),
            this.resources.getString(R.string.Main_Button_ArtikelListe))

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            actions)

        selectDialog.setAdapter(adapter) { _, which ->
            when (which) {
                0 -> { // // Lagerbestand Liste
                    val storageItemList = Intent(this, StorageItemListActivity::class.java)
                    storageItemList.putExtra("EANCode", eanCode)
                    startActivity(storageItemList)

                }
                1 -> {
                    // // Artikel Liste
                    val articleList = Intent(this, ArticleListActivity::class.java)
                    articleList.putExtra("EANCode", eanCode)
                    startActivity(articleList)
                }
            }
        }
        selectDialog.show()
        return
    }

    /// <summary>
    /// Information über abgelaufene Lagerpositionen und die Positionen,
    /// bei denen das Ablaufdatum innerhalb vom Warnungsdatum liegt.
    /// </summary>
    fun showStorageInfoText(exception: String? = null)
    {
        if (exception != null)
        {
            binding.contentLayout.MainTextInfo.text = exception
            binding.contentLayout.MainTextInfo.visibility = TextView.VISIBLE
            return
        }

        binding.contentLayout.MainText1.visibility = TextView.GONE
        binding.contentLayout.MainText2.visibility = TextView.GONE
        binding.contentLayout.MainText.visibility = TextView.GONE
        binding.contentLayout.MainText1Counter.visibility = TextView.INVISIBLE
        binding.contentLayout.MainText2Counter.visibility = TextView.INVISIBLE
        binding.contentLayout.MainShoppingListCounter.visibility = TextView.INVISIBLE

        val abgelaufen = Database.getArticleCountAbgelaufen()
        if (abgelaufen > 0)
        {
            val text = this.resources.getString(R.string.Main_ArticlesWithExpiryDate)

            binding.contentLayout.MainText.visibility = TextView.VISIBLE
            binding.contentLayout.MainText1.text = text.format(Tools.formatNumber(abgelaufen))
            binding.contentLayout.MainText1.visibility = TextView.VISIBLE

            binding.contentLayout.MainText1Counter.text = Tools.formatNumber(abgelaufen)
            binding.contentLayout.MainText1Counter.visibility = TextView.VISIBLE
        }

        val kurzDavor = Database.getArticleCountBaldZuVerbrauchen()
        if (kurzDavor > 0)
        {
            val text = this.resources.getString(R.string.Main_ArticlesNearExpiryDate)

            binding.contentLayout.MainText.visibility = TextView.VISIBLE
            binding.contentLayout.MainText2.text = text.format(Tools.formatNumber(kurzDavor))
            binding.contentLayout.MainText2.visibility = TextView.VISIBLE

            binding.contentLayout.MainText2Counter.text = Tools.formatNumber(kurzDavor)
            binding.contentLayout.MainText2Counter.visibility = TextView.VISIBLE
        }

        val shoppingCount = Database.getShoppingItemCount()
        if (shoppingCount > 0)
        {
            binding.contentLayout.MainShoppingListCounter.text = Tools.formatNumber(shoppingCount)
            binding.contentLayout.MainShoppingListCounter.visibility = TextView.VISIBLE
        }
    }

    private fun switchDatabase() {
        val databases = AndroidDatabase.loadDatabaseFileListSafe(this)

        selectDatabase(databases) { selectedDatabase ->
            var errorMessage = Database.init(selectedDatabase)
            if (errorMessage != null)
            {
                Tools.showMessage(this, errorMessage)
                return@selectDatabase
            }

            Settings.putString("LastSelectedDatabase", selectedDatabase)
            this.showStorageInfoText()
            this.showDatabaseName(selectedDatabase)
        }
    }

    private fun showDatabaseName(databaseName: String) {
        val databaseFile = File(databaseName)

        var databaseInfo = databaseFile.nameWithoutExtension

        if (isOnSDCard(this, databaseFile))
            databaseInfo += " (SD-Karte)"
        else
            databaseInfo += " (Interner Speicher)"

        binding.MainAppBar.subtitle =
            resources.getString(R.string.Main_Database) + " : " + databaseInfo

        if (databaseFile.nameWithoutExtension == "Vorraete_Demo") {
            binding.contentLayout.MainTextInfo.visibility = TextView.VISIBLE
            binding.contentLayout.MainTextInfo.setText(R.string.Main_TestDatabase)
        }
        else {
            binding.contentLayout.MainTextInfo.visibility = TextView.GONE
        }
    }

    fun selectDatabase(databases: List<File>, onSelected: (String) -> Unit) {

        val items = databases.map { file ->
            val size = Tools.toFuzzyByteString(file.length())
            val name = file.nameWithoutExtension
            var info = if (isOnSDCard(this, file)) "SD-Karte" else "Interner Speicher"
            info += ", ${size}"
            name to info
        }

        val adapter = TwoLineAdapter(this, items)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.Main_OpenDatabase)
        builder.setAdapter(adapter) { _, which  ->
            onSelected(databases[which].absolutePath)
        }

        builder.show()
    }

    private fun isOnSDCard(context: Context, file: File): Boolean {
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

    private fun ShowInfoAufTestdatenbank()
    {
        val firstRun = Settings.getBoolean("FirstRun", true)
        if (!firstRun)
            return

        val message = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        message.setIcon(R.drawable.ic_launcher)
        message.setTitle(R.string.App_Name)
        message.setMessage(R.string.Start_TestDbQuestion)
        message.setPositiveButton(R.string.App_Yes) { _, _ ->
            val testDatabasePath = AndroidDatabase.getDemoDatabasePath(this)
            val exception = Database.init(testDatabasePath)

            this.showDatabaseName(testDatabasePath)
            this.showStorageInfoText(exception)

        }
        message.setNegativeButton(R.string.App_No) { _, _ -> }
        message.show()
    }

    private fun protocolAppInfo(context: Context)
    {
        val packageInfo = context.packageManager.getPackageInfo(context.packageName, 0)
        val versionName = packageInfo.versionName
        val versionCode = packageInfo.longVersionCode

        TRACE("--- Application Start ---")
        TRACE("Version $versionName (Code Version ${versionCode})")
        TRACE("Android Version: ${Build.VERSION.RELEASE}")
        TRACE("Android SDK: ${Build.VERSION.SDK_INT}")
        TRACE("CurrentCulture: ${Locale.getDefault().displayName}")
        TRACE("CurrentUICulture: ${Locale.getDefault().displayName}")
    }
}