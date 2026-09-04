package de.stryi.vorratsuebersicht

import ImageResizer
import android.Manifest
import android.annotation.SuppressLint
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.net.Uri
import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.widget.ArrayAdapter
import android.widget.Toast
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import androidx.core.content.FileProvider
import androidx.core.widget.doOnTextChanged
import com.google.android.material.switchmaterial.SwitchMaterial
import de.stryi.vorratsuebersicht.databinding.ArticleDetailsBinding
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.database.Records.Article
import de.stryi.vorratsuebersicht.database.Records.ArticleImage
import de.stryi.vorratsuebersicht.tools.AddToShoppingListDialog
import de.stryi.vorratsuebersicht.tools.PermissionHelper
import de.stryi.vorratsuebersicht.tools.Settings
import de.stryi.vorratsuebersicht.tools.Tools
import de.stryi.vorratsuebersicht.tools.Tools.TRACE
import de.stryi.vorratsuebersicht.tools.UnitConvert
import java.io.ByteArrayOutputStream
import java.io.File
import java.util.Date


class ArticleDetailsActivity : AppCompatActivity() {

    private lateinit var binding: ArticleDetailsBinding

    companion object {
        var imageLarge: ByteArray? = null
        var imageSmall: ByteArray? = null
    }

    private lateinit var article: Article
    private lateinit var articleImage: ArticleImage
    private var articleId: Int = 0
    private var isChanged: Boolean = false
    private var noStorageQuantity: Boolean = false
    private var noDeleteArticle: Boolean = false
    private var ignoreTextChangeEvent = false

    private var isPhotoSelected: Boolean = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        this.binding = ArticleDetailsBinding.inflate(layoutInflater)
        setContentView(binding.root)

        this.setSupportActionBar(binding.ArticleDetailsAppBar)

        binding.ArticleDetailsAppBar.setNavigationOnClickListener { finish() }
        binding.ArticleDetailsAppBar.overflowIcon?.setTint(getColor(R.color.Application_ActionBar_TextColor))

        this.articleId         = intent.getIntExtra("ArticleId", 0)
        this.noStorageQuantity = intent.getBooleanExtra("NoStorageQuantity", false)
        this.noDeleteArticle   = intent.getBooleanExtra("NoDeleteArticle", false)

        val category    = intent.getStringExtra("Category")
        val subCategory = intent.getStringExtra("SubCategory")

        var article = Database.getArticle(this.articleId)
        if (article == null)
        {
            article = Article()
            article.category = Database.getSettingsString("DEFAULT_CATEGORY")
            if (article.category.isNullOrEmpty())
            {
                article.category = this.resources.getString(R.string.ArticleCatagoryDefault)
            }

            if (!category.isNullOrEmpty())    article.category    = category
            if (!subCategory.isNullOrEmpty()) article.subCategory = subCategory
        }

        this.article = article

        var articleImage = Database.getArticleImage(this.articleId, false)
        if (articleImage == null)
        {
            articleImage = ArticleImage()
        }
        this.articleImage = articleImage

        this.showPictureAndDetails()

        if (binding.ArticleDetailsName.text.isNullOrEmpty()) {
            binding.ArticleDetailsName.requestFocus()
        }

        binding.ArticleDetailsSize.doOnTextChanged { _, _, _, _ ->
            this.berechneCalPerUnit()
        }

        binding.ArticleDetailsUnit.doOnTextChanged { _, _, _, _ ->
            this.berechneCalPerUnit()
        }

        binding.ArticleDetailsCalorie.doOnTextChanged { _, _, _, _ ->
            this.berechneCalPerUnit()
        }

        binding.ArticleDetailsCaloriePerUnit.doOnTextChanged { _, _, _, _ ->
            this.berechneCalGes()
        }

        // Hersteller Eingabe
        val manufacturers = Database.getManufacturerNames()
        val manufactureresAdapter = ArrayAdapter(this, android.R.layout.simple_dropdown_item_1line, manufacturers)
        binding.ArticleDetailsManufacturer.setAdapter(manufactureresAdapter)
        binding.ArticleDetailsManufacturer.threshold = 1

        binding.ArticleDetailsSelectManufacturer.setOnClickListener { this.selectManufacturer() }

        // Fest definierte Kategorien + frei definierten zur Auswahl laden
        val defaultCategories = resources.getStringArray(R.array.ArticleCatagories)
        val userCategorrieList = Database.getSettingsList("USER_CATEGORIES")

        // Einträge zusammenführen und sortieren
        val categories = (defaultCategories + userCategorrieList).toMutableList()

        // Wenn die Kategorie im Artikel inzwischen gelöscht ist, ...
        if (article.category?.isNotEmpty() == true)
        {
            if (!categories.contains(article.category))
            {
                categories.add(article.category)
            }
        }
        categories.sortWith(String.CASE_INSENSITIVE_ORDER)

        val categoryAdapter = ArrayAdapter<String>(this, android.R.layout.simple_dropdown_item_1line, categories)
        binding.ArticleDetailsCategory.adapter = categoryAdapter

        val position = categoryAdapter.getPosition(this.article.category)
        if (position >= 0)
        {
            binding.ArticleDetailsCategory.setSelection(position)
        }

        // Unterkategorie Eingabe
        val subCategories = Database.getSubcategoriesOf()
        val subCategoriesAdapter = ArrayAdapter(this, android.R.layout.simple_dropdown_item_1line, subCategories)
        binding.ArticleDetailsSubCategory.setAdapter(subCategoriesAdapter)
        binding.ArticleDetailsSubCategory.threshold = 1

        binding.ArticleDetailsSelectSubCategory.setOnClickListener { this.selectSubCategory() }

        // Einkaufsmarkt Eingabe
        val supermarkets = Database.getSupermarketNames()
        val supermarketsAdapter = ArrayAdapter(this, android.R.layout.simple_dropdown_item_1line, supermarkets)
        binding.ArticleDetailsSupermarket.setAdapter(supermarketsAdapter)
        binding.ArticleDetailsSupermarket.threshold = 1

        binding.ArticleDetailsSelectSupermarket.setOnClickListener { this.selectSupermarket() }

        // Lagerort Eingabe
        val storages = Database.getStorageNames()
        val storagesAdapter = ArrayAdapter(this, android.R.layout.simple_dropdown_item_1line, storages)
        binding.ArticleDetailsStorage.setAdapter(storagesAdapter)
        binding.ArticleDetailsStorage.threshold = 1

        binding.ArticleDetailsSelectStorage.setOnClickListener { this.selectStorage() }

        binding.ArticleDetailsImage.setOnClickListener { this.takeOrShowPhoto() }
        binding.ArticleDetailsImage2.setOnClickListener { this.selectAPicture() }
        binding.ArticleDetailsImageText.setOnClickListener { this.saveAndGoToStorageItem() }

        if (this.article.articleId == 0)
        {
            val  eanCode = intent.getStringExtra("EANCode")

            if (!eanCode.isNullOrEmpty())
            {
                binding.ArticleDetailsEANCode.setText(eanCode)
                this.searchEanCodeOnInternetDb()
            }
        }
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (menu is MenuBuilder)
        {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.article_details_menu, menu)
        return true
    }

    override fun onPrepareOptionsMenu(menu: Menu): Boolean
    {
        val itemDelete = menu.findItem(R.id.ArticleDetailsMenu_Delete)
        itemDelete.isVisible = this.article.articleId > 0

        val itemShowPicture = menu.findItem(R.id.ArticleDetailsMenu_ShowPicture)
        itemShowPicture.isEnabled = this.isPhotoSelected

        val itemRemovePicture = menu.findItem(R.id.ArticleDetailsMenu_RemovePicture)
        itemRemovePicture.isEnabled = this.isPhotoSelected

        if (MainActivity.IsGooglePlayPreLaunchTestMode)
        {
            val itemEanScan = menu.findItem(R.id.ArticleDetailsMenu_ScanEAN)
            itemEanScan.isEnabled = false
        }

        val eanCode = binding.ArticleDetailsEANCode.text.toString()

        val itemInternetDB = menu.findItem(R.id.ArticleDetailsMenu_InternetDB)
        itemInternetDB.isEnabled = eanCode.isNotEmpty()

        menu.findItem(R.id.ArticleDetailsMenu_ToStorageQuantity).isVisible = !this.noStorageQuantity
        menu.findItem(R.id.ArticleDetailsMenu_Delete).isVisible = !this.noDeleteArticle

        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        when (item.itemId) {
            R.id.ArticleDetailsMenu_Delete -> {
                this.deleteArticle()
                return  true
            }

            R.id.ArticleDetailsMenu_Save -> {
                if (!this.saveArticle())
                {
                    return false
                }
                this.finish()
                return true
            }

            R.id.ArticleDetailsMenu_Cancel -> {
                this.moveTaskToBack(false)
                this.finish()
                return true
            }

            R.id.ArticleDetailsMenu_MakeAPhoto -> {
                this.takeAPhoto()
                return true
            }

            R.id.ArticleDetailsMenu_SelectAPicture -> {
                this.selectAPicture()
                return true
            }

            R.id.ArticleDetailsMenu_ShowPicture -> {
                if (this.isPhotoSelected)
                {
                    val articleImage = Intent(this, ArticleImageActivity::class.java)
                    articleImage.putExtra("ArticleId", this.articleId)
                    articleImage.putExtra("EditMode", true)
                    articleImage.putExtra("Title",  binding.ArticleDetailsName.text.toString())
                    this.startActivity(articleImage)
                }
                return true
            }

            R.id.ArticleDetailsMenu_RemovePicture -> {
                if (this.isPhotoSelected)
                {
                    // Erstelltes oder ausgewähltes Bild entfernen
                    imageLarge = null
                    imageSmall = null

                    this.articleImage.imageLarge = null    // Änderungen verwerfen
                    this.articleImage.imageSmall = null    // Gespeichertes Bild auch löschen

                    binding.ArticleDetailsImage.setImageResource(R.drawable.photo_camera_24px)
                    binding.ArticleDetailsImage2.setImageResource(R.drawable.photo_24px)
                    binding.ArticleDetailsImage2.visibility = View.VISIBLE

                    this.isPhotoSelected = false
                    this.isChanged = true
                }
            }

            R.id.ArticleDetailsMenu_ScanEAN -> {
                PermissionHelper().requestPermission(
                    this,
                    Manifest.permission.CAMERA,
                    cameraRequestCode)
                {
                    openEanCodeScanner()
                }
            }

            R.id.ArticleDetailsMenu_InternetDB -> {
                this.searchEanCodeOnInternetDb()
            }

            R.id.ArticleDetailsMenu_ToShoppingList -> {
                this.saveAndAddToShoppingList()
            }

            R.id.ArticleDetailsMenu_ToStorageQuantity -> {
                this.saveAndGoToStorageItem()
            }
        }

        return false
    }

    private fun selectManufacturer()
    {
        val manufacturers = Database.getManufacturerNames()

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            manufacturers)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.ArticleDetails_Manufacturer)
        builder.setAdapter(adapter) { _, which ->
            binding.ArticleDetailsManufacturer.setText( manufacturers[which])
        }
        builder.show()
    }

    private fun selectSubCategory()
    {
        val category = binding.ArticleDetailsCategory.selectedItem.toString()

        val subCategories = Database.getSubcategoriesOf(category)

        if (!subCategories.isEmpty())
        {
            subCategories.add("")
        }

        for (subCategory in Database.getSubcategoriesOf())
        {
            if (!subCategories.contains(subCategory))
            {
                subCategories.add(subCategory)
            }
        }

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            subCategories)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.ArticleDetails_SubCategory)
        builder.setAdapter(adapter) { _, which ->
            binding.ArticleDetailsSubCategory.setText(subCategories[which])
        }
        builder.show()
    }

    private fun selectSupermarket()
    {
        val supermarkets = Database.getSupermarketNames()

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            supermarkets)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.ArticleDetails_SupermarketLabel)
        builder.setAdapter(adapter) { _, which ->
            binding.ArticleDetailsSupermarket.setText(supermarkets[which])
        }
        builder.show()
    }

    private fun selectStorage()
    {
        val storageNames = Database.getStorageNames()

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            storageNames)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.ArticleDetails_StorageLabel)
        builder.setAdapter(adapter) { _, which ->
            binding.ArticleDetailsStorage.setText(storageNames[which])
        }
        builder.show()
    }

    fun berechneCalPerUnit()
    {
        if (this.ignoreTextChangeEvent)
            return

        val unitPerX = UnitConvert.getConvertUnit(binding.ArticleDetailsUnit.text.toString())

        val calPerUnit = UnitConvert.getCaloriePerUnit(
            binding.ArticleDetailsSize.text.toString(),
            binding.ArticleDetailsUnit.text.toString(),
            binding.ArticleDetailsCalorie.text.toString())

        this.ignoreTextChangeEvent = true
        if (calPerUnit != "---")
        {
            binding.ArticleDetailsCaloriePerUnit.setText(calPerUnit)
            binding.ArticleDetailsCaloriePerUnit.isEnabled = true
        }
        else
        {
            binding.ArticleDetailsCaloriePerUnit.setText("---")
            binding.ArticleDetailsCaloriePerUnit.isEnabled = false
        }

        this.ignoreTextChangeEvent = false

        // Text auf "Kalorien pro 100 ??" setzen.
        var perUnitText = resources.getString(R.string.ArticleDetails_CaloriesPerUnit)
        perUnitText = String.format(perUnitText, unitPerX)
        binding.ArticleDetailsCaloriePerUnitLabel.text = perUnitText
    }

    fun berechneCalGes()
    {
        if (this.ignoreTextChangeEvent)
            return

        val calorieGes = UnitConvert.getGesamtCalorie(
            binding.ArticleDetailsSize.text.toString(),
            binding.ArticleDetailsUnit.text.toString(),
            binding.ArticleDetailsCaloriePerUnit.text.toString())

        if (calorieGes == "")
            return

        // Hat sich nichts geändert?
        if (binding.ArticleDetailsCalorie.text.toString() == calorieGes)
            return

        this.ignoreTextChangeEvent = true

        binding.ArticleDetailsCalorie.setText(calorieGes)
        this.ignoreTextChangeEvent = false
    }

    private fun takeOrShowPhoto()
    {
        if (!this.isPhotoSelected) {
            this.takeAPhoto()
            return
        }

        val articleImage = Intent(this, ArticleImageActivity::class.java)
        articleImage.putExtra("ArticleId", this.articleId)
        articleImage.putExtra("EditMode", true)
        articleImage.putExtra("Title",  binding.ArticleDetailsName.text.toString())
        articleImageLauncher.launch(articleImage)
    }

    // Launcher für Bild Betrachter
    private val articleImageLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult())
    { result ->
        if (result.resultCode == RESULT_OK)
        {
            val largeBitmap = BitmapFactory.decodeByteArray(imageLarge, 0, imageLarge!!.size)

            binding.ArticleDetailsImage.setImageBitmap(largeBitmap)
            this.isChanged = true
        }
    }

    private fun openEanCodeScanner() {
        val eanScanFragment = EanCodeScan()
        eanScanFragment.onResult = { eanCode ->
            searchEANCode(eanCode)
        }
        eanScanFragment.show(supportFragmentManager, "EanCodeScan")
    }

    fun searchEANCode(eanCode: String?)
    {
        if (eanCode.isNullOrEmpty())
            return

        TRACE("Scanned Barcode: %s", eanCode)

        if (binding.ArticleDetailsEANCode.text.isNullOrEmpty())
        {
            binding.ArticleDetailsEANCode.setText(eanCode)
            return
        }

        val message = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        message.setIcon(R.drawable.ic_launcher)

        if (binding.ArticleDetailsEANCode.text.contains(eanCode))
        {
            message.setMessage(R.string.ArticleDetails_EanCodeAlreadyContains)
            message.setPositiveButton(R.string.App_Ok) { _, _ -> }
            message.create().show()
            return
        }

        message.setMessage(R.string.ArticleDetails_EanCodeReplaceOrInsert)

        message.setPositiveButton(R.string.App_Replace) { _, _ ->
            binding.ArticleDetailsEANCode.setText(eanCode)
        }

        message.setNegativeButton(R.string.App_Insert) { _, _ ->
            binding.ArticleDetailsEANCode.append(", ")
            binding.ArticleDetailsEANCode.append(eanCode)
        }

        message.setNeutralButton(R.string.App_Cancel, null)

        message.create().show()
    }

    private fun searchEanCodeOnInternetDb() {

        val eanCode = binding.ArticleDetailsEANCode.text.toString()
        if (eanCode.isEmpty())
            return

        val showCostMessage = Settings.getBoolean("ShowOpenFoodFactsInternetCostsMessage", true)
        if (!showCostMessage)
        {
            val internetDB = Intent(this, InternetDatabaseSearchActivity::class.java)
            internetDB.putExtra("EANCode", eanCode)
            internetDatabaseSearchLauncher.launch(internetDB)
            return
        }

        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setTitle(getString(R.string.ArticleDetails_SearchEANScan))
        dialog.setMessage(getString(R.string.ArticleDetails_SearchOnOpenFoodFacts))
        dialog.setIcon(R.drawable.ic_launcher)

        val checkBox = SwitchMaterial(this)
        checkBox.text = getString(R.string.ArticleDetails_StopShowingWarning)
        checkBox.textSize = 14f
        checkBox.setPadding(50, 50, 20, 20)

        dialog.setView(checkBox)

        dialog.setPositiveButton(getString(R.string.App_Yes)) { _, _ ->
            val internetDbIntent = Intent(this, InternetDatabaseSearchActivity::class.java)
            internetDbIntent.putExtra("EANCode", eanCode)
            internetDatabaseSearchLauncher.launch(internetDbIntent)

            if (checkBox.isChecked) {
                Settings.putBoolean("ShowOpenFoodFactsInternetCostsMessage", false)
            }
        }

        dialog.setNegativeButton(getString(R.string.App_No), null)

        dialog.create().show()
    }

    private val internetDatabaseSearchLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult())
    { result ->
        if ((result.resultCode == RESULT_OK) && (result.data != null))
        {
            val name       = result.data!!.getStringExtra("Name")
            val hersteller = result.data!!.getStringExtra("Hersteller")
            val quantity   = result.data!!.getDoubleExtra("Quantity", -1.00)
            val unit       = result.data!!.getStringExtra("Unit")
            val kcalPer100 = result.data!!.getDoubleExtra("KCalPer100", -1.00)

            if (!name.isNullOrEmpty())
                binding.ArticleDetailsName.setText(name)

            if (!hersteller.isNullOrEmpty())
                binding.ArticleDetailsManufacturer.setText(hersteller)

            if (quantity > 0)
                binding.ArticleDetailsSize.setText(Tools.formatNumber(quantity))

            if (!unit.isNullOrEmpty())
                binding.ArticleDetailsUnit.setText(unit)

            if (kcalPer100 > 0)
            {
                binding.ArticleDetailsCalorie.setText(Tools.formatNumber(kcalPer100))
                this.berechneCalGes()
            }

            if (InternetDatabaseSearchActivity.picture != null)
            {
                this.resizeBitmap(InternetDatabaseSearchActivity.picture!!)
            }
        }
    }

    private fun saveAndAddToShoppingList() {

        if (this.articleId != 0)
        {
            this.saveArticle()
            this.addToShoppingListManually()
            return
        }

        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setMessage(getString(R.string.ArticleDetails_SaveToAddToShippingList))
        dialog.setIcon(R.drawable.ic_launcher)
        dialog.setNegativeButton(getString(R.string.App_Cancel)) { _, _ -> }
        dialog.setPositiveButton(getString(R.string.App_Ok)) { _, _ ->
            saveArticle()
            if (articleId != 0) {   // Speichern erfolgreich (articleId gesetzt?)
                addToShoppingListManually()
                return@setPositiveButton
            }
        }
        dialog.create().show()
    }

    private fun saveAndGoToStorageItem() {

        if (this.noStorageQuantity)
            return

        if (this.articleId != 0)
        {
            this.saveArticle()
            this.goToStorageItem(this.articleId)
            return
        }

        val message = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        message.setMessage(getString(R.string.ArticleDetails_SaveToAddToStorage))
        message.setIcon(R.drawable.ic_launcher)
        message.setNegativeButton(getString(R.string.App_Cancel)) { _, _ -> }
        message.setPositiveButton(getString(R.string.App_Ok)) { _, _ ->
            this.saveArticle()
            if (this.articleId != 0)
            {
                this.goToStorageItem(this.articleId)
                return@setPositiveButton
            }
        }
        message.create().show()
    }

    private fun goToStorageItem(articleId: Int) {

        if (this.noStorageQuantity)
            return

        val storageDetails = Intent(this, StorageItemInventoryActivity::class.java)
        storageDetails.putExtra("ArticleId", articleId)
        storageDetails.putExtra("NoArticleDetails", true)
        storageDetailsLauncher.launch(storageDetails)
    }

    private val storageDetailsLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult())
    { result ->
        if (result.resultCode == RESULT_OK)
        {
            this.showStoreQuantityInfo()
        }
    }

    override fun finish() {
        if (isChanged) {
            val returnIntent = Intent()
            setResult(RESULT_OK, returnIntent)
        }

        //this.article = null
        imageLarge = null
        imageSmall = null
        InternetDatabaseSearchActivity.picture = null

        super.finish()
    }

    // Launcher für das Bild-Auswahl-Intent
    private val pickImageLauncher = registerForActivityResult(ActivityResultContracts.GetContent())
    { uri: Uri? ->
        if (uri == null)
            return@registerForActivityResult

        this.loadAndResizeBitmap(uri)
    }

    private fun selectAPicture() {
        pickImageLauncher.launch("image/*")
    }

    val cameraRequestCode = 101
    private lateinit var photoUri: Uri

    private fun createImageUri(): Uri
    {
        val dir = externalCacheDirs.firstOrNull() ?: cacheDir

        val file = File(dir, "temp_photo.jpg")

        return FileProvider.getUriForFile(this, "de.stryi.vorratsuebersicht.provider", file)
    }

    // Launcher für die Kamera-Foto-Aufnahme
    private val takePictureLauncher = registerForActivityResult(ActivityResultContracts.TakePicture())
    { success ->
        if (!success) return@registerForActivityResult

        val bitmap = BitmapFactory.decodeStream(contentResolver.openInputStream(photoUri))
        resizeBitmap(bitmap)
    }

    private fun takeAPhoto() {
        if (MainActivity.IsGooglePlayPreLaunchTestMode)
        {
            return
        }
        PermissionHelper().requestPermission(
            this,
            Manifest.permission.CAMERA,
            cameraRequestCode)
        {
            photoUri = createImageUri()
            takePictureLauncher.launch(photoUri)
        }
    }

    private fun saveArticle() : Boolean {
        try {
            this.article.name = binding.ArticleDetailsName.text.toString()
            this.article.manufacturer = binding.ArticleDetailsManufacturer.text.toString()
            this.article.category    = binding.ArticleDetailsCategory.selectedItem.toString()
            this.article.subCategory = binding.ArticleDetailsSubCategory.text.toString()
            this.article.supermarket = binding.ArticleDetailsSupermarket.text.toString()
            this.article.storageName = binding.ArticleDetailsStorage.text.toString()
            this.article.durableInfinity = binding.ArticleDetailsDurableInfinity.isChecked

            this.article.warnInDays = getIntegerFromEditText(binding.ArticleDetailsWarnInDays.text.toString())
            this.article.price      = getDoubleFromText(binding.ArticleDetailsPrice.text.toString())

            this.article.size       = getDoubleFromText(binding.ArticleDetailsSize.text.toString())
            this.article.unit       = binding.ArticleDetailsUnit.text.toString()
            this.article.calorie    = getIntegerFromEditText(binding.ArticleDetailsCalorie.text.toString())

            this.article.minQuantity  = getIntegerFromEditText(binding.ArticleDetailsMinQuantity.text.toString())
            this.article.prefQuantity = getIntegerFromEditText(binding.ArticleDetailsPrefQuantity.text.toString())

            this.article.eanCode = binding.ArticleDetailsEANCode.text.toString()
            this.article.notes = binding.ArticleDetailsNotes.text.toString()

            this.article.name = this.article.name?.trim()
            this.article.manufacturer = this.article.manufacturer?.trim()
            this.article.subCategory = this.article.subCategory?.trim()
            this.article.supermarket = this.article.supermarket?.trim()
            this.article.storageName = this.article.storageName?.trim()
            this.article.eanCode = this.article.eanCode?.trim()
            this.article.notes = this.article.notes?.trim()

            if (this.article.articleId > 0)
            {
                Database.updateArticle(this.article)
            }
            else
            {
                val newId = Database.insertArtice(this.article)
                this.articleId = newId.toInt()
            }

            if (imageLarge != null)
                this.articleImage.imageLarge = imageLarge

            if (imageSmall != null)
                this.articleImage.imageSmall = imageSmall

            if (this.articleImage.imageLarge != null)   // Ein neues Bild wurde ausgewählt oder vorhandenes geändert.
            {
                if (this.articleImage.imageId > 0)
                {
                    Database.updateArticleImage(this.articleImage)
                }
                else
                {
                    this.articleImage.articleId = this.articleId
                    this.articleImage.type = 0
                    this.articleImage.createdAt = Date()
                    Database.insertArticleImage(this.articleImage)
                }
            }

            if ((this.articleImage.imageSmall == null) && (this.articleImage.imageId > 0))  // Vorhandenes Bild gelöscht?
            {
                Database.deleteArticleImage(this.articleImage)
            }
        }
        catch (ex: Exception)
        {
            Toast.makeText(this, ex.message, Toast.LENGTH_LONG).show()
            return false
        }

        isChanged = true

        return true
    }

    private fun deleteArticle() {
        val anzahl = Database.getArticleQuantityInStorage(this.articleId)
        if (anzahl > 0)
        {
            var message = resources.getString(R.string.ArticleDetails_CanNotDeleteBecauseStorage)
            message = message.format(Tools.formatNumber(anzahl))

            val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
            dialog.setMessage(message)
            dialog.setPositiveButton(R.string.App_Ok, null)
            dialog.show()
            return
        }

        var msg = resources.getString(R.string.ArticleDetails_DeleteArticlerReally)
        val isInShoppingList = Database.isArticleInShoppingList(this.articleId)
        if (isInShoppingList)
        {
            msg += "\n\n"
            msg += resources.getString(R.string.ArticleDetails_ArticleOnShoppingList)
        }

        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setMessage(msg)
        dialog.setNegativeButton(R.string.App_Cancel, null)
        dialog.setPositiveButton(R.string.App_Ok) { _, _ ->
            Database.deleteArticle(this.articleId)
            this.setResult(RESULT_OK)
            finish()
        }
        dialog.show()
    }

    private fun addToShoppingListManually() {

        val minQuantity = this.article.minQuantity
        val prefQuantity = this.article.prefQuantity

        AddToShoppingListDialog.showDialog(this, this.articleId, minQuantity, prefQuantity)
    }

    private fun showPictureAndDetails() {

        if (this.articleImage.imageSmall != null)
        {
            val smallBitmap: Bitmap? = BitmapFactory.decodeByteArray(
                articleImage.imageSmall,
                0,
                articleImage.imageSmall!!.size)

            binding.ArticleDetailsImage.setImageBitmap(smallBitmap)
            binding.ArticleDetailsImage2.visibility = View.GONE
            isPhotoSelected = true
        }

        binding.ArticleDetailsName.setText(this.article.name)
        binding.ArticleDetailsManufacturer.setText(this.article.manufacturer)
        binding.ArticleDetailsSubCategory.setText(this.article.subCategory)
        binding.ArticleDetailsSupermarket.setText(this.article.supermarket)
        binding.ArticleDetailsStorage.setText(this.article.storageName)
        binding.ArticleDetailsDurableInfinity.isChecked = this.article.durableInfinity
        binding.ArticleDetailsWarnInDays.setText(Tools.formatNumber(this.article.warnInDays))
        binding.ArticleDetailsPrice.setText(Tools.formatNumber(this.article.price))
        binding.ArticleDetailsSize.setText(Tools.formatNumber(this.article.size))
        binding.ArticleDetailsUnit.setText(this.article.unit)
        binding.ArticleDetailsCalorie.setText(Tools.formatNumber(this.article.calorie))
        binding.ArticleDetailsMinQuantity.setText(Tools.formatNumber(this.article.minQuantity))
        binding.ArticleDetailsPrefQuantity.setText(Tools.formatNumber(this.article.prefQuantity))
        binding.ArticleDetailsEANCode.setText(this.article.eanCode)
        binding.ArticleDetailsNotes.setText(this.article.notes)
        binding.ArticleDetailsArticleId.text = Tools.formatResource(this, R.string.ArticleDetails_ArticleId, this.article.articleId)

        this.berechneCalPerUnit()

        this.showStoreQuantityInfo()
    }

    @SuppressLint("StringFormatInvalid")
    private fun showStoreQuantityInfo() {
        val storageItemBestList = Database.getBestBeforeItemQuantity(article.articleId)

        var bestand = 0.00
        var vorDemAblauf = 0.00
        var mitWarnung = 0.00
        var abgelaufen = 0.00

        for(result in storageItemBestList)
        {
            bestand += result.quantity
            if (result.warningLevel == 0)
            {
                vorDemAblauf += result.quantity
            }
            if (result.warningLevel == 1)
            {
                mitWarnung += result.quantity
            }
            if (result.warningLevel == 2)
            {
                abgelaufen += result.quantity
            }
        }

        var info = String.format(resources.getString(R.string.StorageItem_InventoryInPieces), Tools.formatNumber(bestand))

        if (vorDemAblauf > 0)
        {
            if (info.isNotEmpty()) info += "\r\n"
            info += String.format(resources.getString(R.string.ArticleDetails_WithExpiryDate), Tools.formatNumber(vorDemAblauf))
        }

        if (mitWarnung > 0)
        {
            if (info.isNotEmpty()) info += "\r\n"
            info += String.format(resources.getString(R.string.ArticleDetails_WithWarnings), Tools.formatNumber(mitWarnung))
        }

        if (abgelaufen > 0)
        {
            if (info.isNotEmpty()) info += "\r\n"
            info += String.format(resources.getString(R.string.ArticleDetails_AfterExpiryDate), Tools.formatNumber(abgelaufen))
        }

        binding.ArticleDetailsImageText.text = info
    }

    private fun resizeBitmap(newBitmap: Bitmap)
    {
        var widthLarge = 854
        var heightLarge = 854

        var largeBitmap : Bitmap
        var resizedImage: ByteArray

        val compressMode = Settings.getInt("CompressPicturesMode", 2)
        if (compressMode == 2)
        {
            widthLarge  = 1_024
            heightLarge = 1_024
        }

        if (compressMode == 3)
        {
            widthLarge  = 1_280
            heightLarge = 1_280
        }

        if (compressMode == 4)
        {
            widthLarge  = 1_536
            heightLarge = 1_536
        }

        widthLarge  = Math.min(newBitmap.width,  widthLarge)
        heightLarge = Math.min(newBitmap.height, heightLarge)

        resizedImage= ImageResizer.resizeImageAndroid(newBitmap, widthLarge.toFloat(), heightLarge.toFloat())
        largeBitmap = BitmapFactory.decodeByteArray(resizedImage, 0, resizedImage.size)

        var stream = ByteArrayOutputStream()
        largeBitmap.compress(Bitmap.CompressFormat.PNG, 100, stream)
        imageLarge = stream.toByteArray()

        // --------------------------------------------------------------------------------
        // Miniaturansicht erstellen
        // --------------------------------------------------------------------------------

        resizedImage= ImageResizer.resizeImageAndroid(newBitmap, (48*2).toFloat(), (85*2).toFloat())
        val smallBitmap = BitmapFactory.decodeByteArray(resizedImage, 0, resizedImage.size)

        stream = ByteArrayOutputStream()
        smallBitmap.compress(Bitmap.CompressFormat.PNG, 100, stream)
        imageSmall = stream.toByteArray()

        runOnUiThread {
            binding.ArticleDetailsImage.setImageBitmap(smallBitmap)
            binding.ArticleDetailsImage2.visibility = View.GONE
            invalidateOptionsMenu() // To update the menu items
        }

        isPhotoSelected = true
        isChanged = true

        TRACE("-------------------------------------")
        TRACE(String.format("Org.: %s x %s (%s)", Tools.formatNumber(newBitmap.width),   Tools.formatNumber(newBitmap.height),   Tools.toFuzzyByteString(newBitmap.byteCount.toLong())))
        TRACE(String.format("Bild: %s x %s (%s)", Tools.formatNumber(largeBitmap.width), Tools.formatNumber(largeBitmap.height), Tools.toFuzzyByteString(largeBitmap.byteCount.toLong())))
        TRACE(String.format("Thn.: %s x %s (%s)", Tools.formatNumber(smallBitmap.width), Tools.formatNumber(smallBitmap.height), Tools.toFuzzyByteString(smallBitmap.byteCount.toLong())))
        TRACE("-------------------------------------")

    }

    private fun loadAndResizeBitmap(uri: Uri) {
        this.createProgressBar()

        Thread {
            Thread.sleep(1000)
            val inputStream = contentResolver.openInputStream(uri)
            val bitmap = BitmapFactory.decodeStream(inputStream)
            inputStream?.close()

            this.resizeBitmap(bitmap)

            runOnUiThread {
                this.hideProgressBar()
            }
        }.start()
    }

    fun getIntegerFromEditText(valueText: String): Int?
    {
        if (valueText.isEmpty())
            return null

        try
        {
            return valueText.toInt()
        }
        catch (ex: Exception) {
            TRACE("Fehler beim Konvertieren von %s in ein Integer.", valueText)
            TRACE(ex.message)
        }
        return null
    }

    fun getDoubleFromText(valueText: String): Double?
    {
        if (valueText.isEmpty())
            return null

        try
        {
            return valueText.toDouble()
        }
        catch (ex: Exception)
        {
            TRACE("Fehler beim Konvertieren von %s in ein Double.", valueText)
            TRACE(ex.message)
        }

        return null
    }

    private fun createProgressBar() {
        binding.ProgressBar.visibility = View.VISIBLE
    }

    private fun hideProgressBar() {
        binding.ProgressBar.visibility = View.INVISIBLE
    }
}
