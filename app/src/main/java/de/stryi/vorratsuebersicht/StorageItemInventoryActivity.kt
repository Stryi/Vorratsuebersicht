package de.stryi.vorratsuebersicht

import android.annotation.SuppressLint
import androidx.appcompat.app.AlertDialog
import android.content.Intent
import android.graphics.BitmapFactory
import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.view.WindowManager
import android.widget.ArrayAdapter
import android.widget.EditText
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.DividerItemDecoration
import androidx.recyclerview.widget.LinearLayoutManager
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.database.Records.Article
import de.stryi.vorratsuebersicht.database.Records.StorageItem
import de.stryi.vorratsuebersicht.databinding.StorageItemInventoryBinding
import de.stryi.vorratsuebersicht.tools.AddToShoppingListDialog
import de.stryi.vorratsuebersicht.tools.Tools
import kotlinx.coroutines.launch
import java.time.LocalDate


class StorageItemInventoryActivity : AppCompatActivity() {

    private lateinit var binding: StorageItemInventoryBinding

    private lateinit var article: Article
    private var articleId: Int = 0
    private var durableInfinity = false
    private var isChanged = false
    private var isEditMode = false
    private var isAddedToShoppingList = false
    private var noArticleDetails = false
    private var quantity = -1.00
    private var isImage = false
    private var stepValue = 1.0
    private var storages = listOf<String>()

    private val articleDetailsLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {
                this.showPictureAndDetails(this.article.articleId)
            }
        }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = StorageItemInventoryBinding.inflate(layoutInflater)
        setContentView(binding.root)

        this.setSupportActionBar(binding.StorageItemInventoryAppBar)

        binding.StorageItemInventoryAppBar.setNavigationOnClickListener { finish() }
        binding.StorageItemInventoryAppBar.overflowIcon?.setTint(getColor(R.color.Application_ActionBar_TextColor))

        // Trennlinie in der Liste
        binding.StorageItemQuantityView.isClickable = true
        val divider = DividerItemDecoration(this, DividerItemDecoration.VERTICAL)
        ContextCompat.getDrawable(this, R.drawable.divider)?.let { divider.setDrawable(it) }
        binding.StorageItemQuantityView.addItemDecoration(divider)

        this.articleId        = intent.getIntExtra    ("ArticleId", 0)
        val editMode          = intent.getBooleanExtra("EditMode", false)
        this.noArticleDetails = intent.getBooleanExtra("NoArticleDetails", false)
        this.quantity         = intent.getDoubleExtra ("Quantity", -1.0)

        this.showPictureAndDetails(this.articleId)
        this.showStorageListForArticle(this.articleId)

        // Lagerort Eingabe
        this.storages = Database.getStorageNames()

        val storageAdapter = ArrayAdapter(this, android.R.layout.simple_dropdown_item_1line, this.storages)
        binding.StorageItemQuantityStorageText.setAdapter(storageAdapter)
        binding.StorageItemQuantityStorageText.threshold = 1
        binding.StorageItemQuantityStorageText.setText(this.article.storageName?.trimEnd())

        binding.StorageItemQuantityAddArticle.setOnClickListener { this.addArticle() }

        binding.StorageItemQuantitySelectStorage.setOnClickListener { this.selectStorageForCreation() }
        binding.StorageItemQuantityStepButton.setOnClickListener    { this.setQuantityStep() }

        binding.StorageItemQuantityImage.setOnClickListener         { this.goToPicture()}
        binding.StorageItemQuantityArticleDetail.setOnClickListener { this.goToArticleDetails() }

        if (editMode)
        {
            this.setEditMode(true)
        }

        if (this.quantity > 0.00)
        {
            this.addArticle()
        }
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.storage_item_inventory_manu, menu)
        return true
    }

    override fun onPrepareOptionsMenu(menu: Menu): Boolean {
        menu.findItem(R.id.StorageItemQuantity_Menu_Edit).isVisible   = !this.isEditMode
        menu.findItem(R.id.StorageItemQuantity_Menu_Cancel).isVisible = this.isEditMode
        menu.findItem(R.id.StorageItemQuantity_Menu_Save).isVisible   = this.isEditMode

        menu.findItem((R.id.StorageItemQuantity_Menu_EditPicture)).isVisible    = this.isImage
        menu.findItem((R.id.StorageItemQuantity_Menu_ArticleDetails)).isVisible = !this.noArticleDetails

        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        when (item.itemId) {
            R.id.StorageItemQuantity_Menu_Edit ->
            {
                this.setEditMode(true)
            }

            R.id.StorageItemQuantity_Menu_Save ->
            {
                this.isChanged = true
                val ok = this.saveChanges()
                if (ok == false) {
                    return false
                }

                this.setEditMode(false)
                this.quantity = -1.00
                this.showStorageListForArticle(this.articleId)
                this.addToShoppingList()
            }

            R.id.StorageItemQuantity_Menu_Cancel -> {
                this.showStorageListForArticle(this.articleId)
                this.setEditMode(false)
                this.quantity = -1.00
            }

            R.id.StorageItemQuantity_Menu_ToShoppingList -> {
                this.addToShoppingListManually()
            }

            R.id.StorageItemQuantity_Menu_ArticleDetails -> {
                this.goToArticleDetails()
            }

            R.id.StorageItemQuantity_Menu_EditPicture -> {
                this.goToPicture()
            }
        }
        return false
    }

    override fun finish()
    {
        if (!this.isAddedToShoppingList)
        {
            val dialogIsShowing = this.addToShoppingList()
            if (dialogIsShowing)
                return
        }

        if (this.isChanged)
        {
            val resultIntent = Intent()
            resultIntent.putExtra("ArticleId", this.articleId)
            this.setResult(RESULT_OK, resultIntent)
        }
        super.finish()
    }

    private fun saveChanges() : Boolean {

        val adapter = binding.StorageItemQuantityView.adapter as StorageItemInventoryViewAdapter

        try {
            val storageItemList = adapter.getChangedStorageItems()
            for (storageItem in storageItemList)
            {
                Database.updateStorageItemQuantity(storageItem)
                storageItem.isChanged = false
            }
            return true
        }
        catch (e: Exception)
        {
            val messageBox = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
            messageBox.setTitle(R.string.App_ErrorOccurred)
            messageBox.setMessage(e.message)
            messageBox.setPositiveButton(resources.getString(R.string.App_Ok), null)
            messageBox.show()
        }
        return false
    }

    @SuppressLint("NotifyDataSetChanged")
    private fun setEditMode(editMode: Boolean) {
        this.isEditMode = editMode

        this.invalidateOptionsMenu()
        val adapter = binding.StorageItemQuantityView.adapter as StorageItemInventoryViewAdapter

        if (editMode)
        {
            binding.StorageItemQuantityStorage.visibility = View.VISIBLE
            binding.StorageItemQuantityStep.visibility    = View.VISIBLE

            adapter.activateButtons()
            this.window.setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_HIDDEN)
        }
        else
        {
            binding.StorageItemQuantityStorage.visibility = View.GONE
            binding.StorageItemQuantityStep.visibility    = View.GONE

            adapter.deactivateButtons()
        }
        binding.StorageItemQuantityView.adapter?.notifyDataSetChanged()
    }

    fun addToShoppingListManually()
    {
        val minQuantity = this.article.minQuantity
        val prefQuantity = this.article.prefQuantity

        AddToShoppingListDialog.showDialog(this, this.article.articleId, minQuantity, prefQuantity)

        this.isAddedToShoppingList = true
    }

    fun addToShoppingList() : Boolean
    {
        val toBuy = Database.getToShoppingListQuantity(this.articleId, this.article.minQuantity, article.prefQuantity)
        if (toBuy == 0)
            return false

        this.addToShoppingListManually()

        return true
    }

    fun showPictureAndDetails(articleId: Int)
    {
        this.article = Database.getArticle(articleId)!!

        this.durableInfinity = article.durableInfinity

        binding.StorageItemQuantityArticleDetailHeader.text = this.article.heading
        binding.StorageItemQuantityArticleDetail.text       = this.article.articleInfo

        val articleImage = Database.getArticleImage(articleId)
        if (articleImage != null)
        {
            val largeBitmap = BitmapFactory.decodeByteArray(articleImage.imageSmall, 0, articleImage.imageSmall!!.size)
            binding.StorageItemQuantityImage.setImageBitmap(largeBitmap)

            this.isImage = true
        }
    }

    fun goToArticleDetails()
    {
        if (this.noArticleDetails)
            return

        val intent = Intent(this, ArticleDetailsActivity::class.java)
        intent.putExtra("ArticleId", this.article.articleId)
        intent.putExtra("NoStorageQuantity",  true)
        intent.putExtra("NoDeleteArticle",    true)
        articleDetailsLauncher.launch(intent)
    }

    fun goToPicture()
    {
        if (!this.isImage)
        {
            return
        }
        val intent = Intent(this, ArticleImageActivity::class.java)
        intent.putExtra("Heading",   this.article.heading)
        intent.putExtra("ArticleId", this.article.articleId)
        startActivity(intent)
    }

    private fun selectStorageForCreation() {
        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.StorageItemQuantityList_StorageForNew)

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            this.storages)

        builder.setAdapter(adapter) { _, which ->
            binding.StorageItemQuantityStorageText.setText(this.storages[which])
        }
        builder.show()
    }

    @SuppressLint("SetTextI18n")
    private fun setQuantityStep() {
        when(stepValue){
              0.01 -> stepValue =   0.10
              0.10 -> stepValue =   0.25
              0.25 -> stepValue =   0.50
              0.50 -> stepValue =   1.00
              1.00 -> stepValue =  10.00
             10.00 -> stepValue = 100.00
            100.00 -> stepValue =   0.01
        }
        binding.StorageItemQuantityStepText.text = "%sx".format(Tools.formatNumber(stepValue))
    }

    private fun showStorageListForArticle(articleId: Int) {

        val storageItemQuantityList = Database.getStorageItemQuantityList(articleId)

        val listAdapter = StorageItemInventoryViewAdapter(storageItemQuantityList.toMutableList(), this::onItemClicked)
        binding.StorageItemQuantityView.layoutManager = LinearLayoutManager(this)
        binding.StorageItemQuantityView.adapter = listAdapter
    }

    @SuppressLint("NotifyDataSetChanged")
    private fun onItemClicked(action: StorageItemInventoryViewAdapter.ActionType, storageItem: StorageItem) {

        if (action == StorageItemInventoryViewAdapter.ActionType.CHANGE_QUANTITY)
        {
            this.changeQuantity(storageItem)
        }
        if (action == StorageItemInventoryViewAdapter.ActionType.INCREASE_QUANTITY)
        {
            storageItem.quantity = storageItem.quantity + 1
            this.saveStorageItem(storageItem)
        }
        if (action == StorageItemInventoryViewAdapter.ActionType.DECREASE_QUANTITY)
        {
            storageItem.quantity = storageItem.quantity - 1
            if (storageItem.quantity < 0.00)
            {
                storageItem.quantity = 0.00
            }

            this.saveStorageItem(storageItem)
        }
        if (action == StorageItemInventoryViewAdapter.ActionType.CHANGE_DATE)
        {
            this.changeDate(storageItem)
        }
        if (action == StorageItemInventoryViewAdapter.ActionType.CHANGE_STORAGE)
        {
            this.changeStorage(storageItem)
        }

        if (action == StorageItemInventoryViewAdapter.ActionType.ADD_QUANTITY)
        {
            storageItem.quantity = storageItem.quantity + stepValue
            storageItem.isChanged = true
            binding.StorageItemQuantityView.adapter?.notifyDataSetChanged()
        }

        if (action == StorageItemInventoryViewAdapter.ActionType.REMOVE_QUANTITY)
        {
            storageItem.quantity = storageItem.quantity - stepValue
            if (storageItem.quantity < 0.00)
            {
                storageItem.quantity = 0.00
            }
            storageItem.isChanged = true
            binding.StorageItemQuantityView.adapter?.notifyDataSetChanged()
        }
    }

    private fun changeStorage(storageItem: StorageItem) {

        val storages = this.storages.toMutableList()

        val selectedStorage = binding.StorageItemQuantityStorageText.text.toString()

        if (selectedStorage.isNotEmpty() && !storages.contains(selectedStorage))
        {
            storages.add(selectedStorage)
        }

        storages.add(0, "[Kein Lagerort]")
        storages.add(1, "[Neuen Lagerort eingeben]")

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            storages)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.StorageItemQuantityList_SelectStorage)
        builder.setAdapter(adapter) { _, which ->
            if (which == 0) {
                storageItem.storageName = null
                this.saveStorageItem(storageItem)
            } else if (which == 1) {
                lifecycleScope.launch {
                    val newStorageName = Tools.askForText(
                        this@StorageItemInventoryActivity,
                        "Lager",
                        "Name vom Lagerort",
                        ""
                    )
                    if (newStorageName != null) {
                        if (!this@StorageItemInventoryActivity.storages.contains(newStorageName)) {
                            val mutableStorages = this@StorageItemInventoryActivity.storages.toMutableList()
                            mutableStorages.add(newStorageName)
                            mutableStorages.sort()
                            this@StorageItemInventoryActivity.storages = mutableStorages

                            val storageAdapter = ArrayAdapter(
                                this@StorageItemInventoryActivity,
                                android.R.layout.simple_dropdown_item_1line,
                                this@StorageItemInventoryActivity.storages
                            )
                            binding.StorageItemQuantityStorageText.setAdapter(storageAdapter)
                        }

                        storageItem.storageName = newStorageName
                    }
                    this@StorageItemInventoryActivity.saveStorageItem(storageItem)
                }
            } else {
                storageItem.storageName = storages[which]
                this.saveStorageItem(storageItem)
            }
        }
        builder.show()
    }

    private fun changeDate(storageItem: StorageItem, onDateSelected: (() -> Unit)? = null) {
        val datePickerFragment = DatePickerFragment(storageItem.bestBefore)
        datePickerFragment.onResult = { date ->
            storageItem.bestBefore = date
            this.saveStorageItem(storageItem)
            onDateSelected?.invoke()
        }
        datePickerFragment.show(supportFragmentManager, "DatePicker")
    }

    private fun changeQuantity(storageItem: StorageItem) {
        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setMessage(R.string.App_EnterQuantity)
        val input = EditText(this)
        input.inputType = android.text.InputType.TYPE_CLASS_NUMBER or android.text.InputType.TYPE_NUMBER_FLAG_DECIMAL
        if (storageItem.quantity > 0.00)
        {
            input.setText(Tools.formatNumber(storageItem.quantity))
        }
        input.setSelection(input.text.length)

        // Setze den Abstand (Padding) für den EditText
        val marginInDp = 20
        val scale = this.resources.displayMetrics.density
        val marginInPixels = (marginInDp * scale + 0.5f).toInt()
        input.setPadding(marginInPixels, marginInPixels, marginInPixels, marginInPixels)

        input.requestFocus()
        input.setSelection(0, input.text.length)
        dialog.setView(input)
        dialog.setNegativeButton(R.string.App_Cancel) { _, _ ->
            this.changeStorage(storageItem)
        }
        dialog.setPositiveButton(R.string.App_Ok) { _, _ ->
            if (input.text.isNullOrEmpty())
                input.setText("0")

            val inputText = input.text.toString()
            val neueAnzahl = inputText.toDoubleOrNull()

            if (neueAnzahl != null) {
                storageItem.quantity = neueAnzahl

                this.saveStorageItem(storageItem)

                val defaultStorageName = binding.StorageItemQuantityStorageText.text.toString().trimEnd()

                if (defaultStorageName.isNullOrEmpty())
                {
                    this.changeStorage(storageItem)
                }

                if (this.quantity >= 1)
                {
                    this.quantity -= neueAnzahl
                }
            }

        }
        val alertDialog = dialog.show()
        alertDialog.window?.setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE)
    }

    fun saveStorageItem(storageItem: StorageItem)
    {
        if (this.isEditMode)
        {
            // Änderung merken, später speichern.
            storageItem.isChanged = true
        }
        else
        {
            // Änderung sofort speichern.
            Database.updateStorageItemQuantity(storageItem)
            this.isChanged = true
        }

        binding.StorageItemQuantityView.adapter?.notifyDataSetChanged()
    }

    fun addArticle() {
        val storageName = binding.StorageItemQuantityStorageText.text.toString().trimEnd()

        val storageItem = StorageItem()
        storageItem.articleId   = this.article.articleId
        storageItem.quantity    = 1.00
        if (!article.durableInfinity)
        {
            storageItem.bestBefore  = LocalDate.now()
        }
        storageItem.storageName = storageName
        storageItem.isChanged   = true

        if (this.quantity > 1)
        {
            storageItem.quantity = this.quantity
        }

        val adapter = binding.StorageItemQuantityView.adapter as StorageItemInventoryViewAdapter
        adapter.addStorageItem(storageItem)
        adapter.notifyDataSetChanged()

        if (!this.article.durableInfinity)
        {
            this.changeDate(storageItem, onDateSelected =
                {
                    this.changeQuantity(storageItem)
                })
        }
    }
}
