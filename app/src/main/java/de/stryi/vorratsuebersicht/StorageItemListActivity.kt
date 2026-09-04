package de.stryi.vorratsuebersicht

import StockStatistic
import android.annotation.SuppressLint
import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.os.Parcelable
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.widget.AdapterView
import android.widget.ArrayAdapter
import android.widget.SearchView
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import androidx.appcompat.widget.PopupMenu
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.DividerItemDecoration
import androidx.recyclerview.widget.LinearLayoutManager
import de.stryi.vorratsuebersicht.StorageItemViewAdapter.StorageItemViewHolder
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.databinding.StorageItemListBinding
import de.stryi.vorratsuebersicht.tools.AddToShoppingListDialog
import de.stryi.vorratsuebersicht.tools.Settings
import de.stryi.vorratsuebersicht.tools.Tools
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter


class StorageItemListActivity : AppCompatActivity() {

    private var withoutStorage: Boolean = false
    private var storageNameFilter: String = ""
    private var showEmptyStorageArticles: Boolean = false
    private var eanCode: String? = null
    private var filterExpiryDate: String? = null
    private var subCategory: String? = null
    private var category: String? = null
    private lateinit var binding: StorageItemListBinding
    private var lastSearchText: String? = null

    private var listViewState: Parcelable? = null

    private var orderByToConsumeDate : Boolean = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = StorageItemListBinding.inflate(layoutInflater)
        setContentView(binding.root)

        this.setSupportActionBar(binding.StorageItemListAppBar)

        binding.StorageItemListAppBar.setNavigationOnClickListener { finish() }
        binding.StorageItemListAppBar.overflowIcon?.setTint(getColor(R.color.Application_ActionBar_TextColor))

        binding.StorageItemListStorages.onItemSelectedListener =
            object : AdapterView.OnItemSelectedListener {
                override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long)
                {
                    spinnerStorageItemSelected(position)
                }

                override fun onNothingSelected(parent: AdapterView<*>?) {
                    // optional, kann leer bleiben
                }
            }

        // Trennlinie in der Liste
        val divider = DividerItemDecoration(this, DividerItemDecoration.VERTICAL)
        ContextCompat.getDrawable(this, R.drawable.divider)?.let { divider.setDrawable(it) }
        binding.StorageItemView.addItemDecoration(divider)
        binding.StorageItemView.isClickable = true

        // "Pull to Refresh" Funktionalität
        binding.StorageItemViewSwipeRefreshLayout.setOnRefreshListener {
            this.showStorageItemList()
            binding.StorageItemViewSwipeRefreshLayout.isRefreshing = false
        }

        this.orderByToConsumeDate     = Settings.getBoolean("StorageItemListOrder", false)

        this.category                 = intent.getStringExtra ("Category")
        this.subCategory              = intent.getStringExtra ("SubCategory")
        val  orderByDate              = intent.getBooleanExtra("OrderByToConsumeDate", false)
        this.filterExpiryDate         = intent.getStringExtra ("FilterExpiryDate")
        this.eanCode                  = intent.getStringExtra("EANCode")
        this.showEmptyStorageArticles = intent.getBooleanExtra("ShowEmptyStorageArticles", false) // Auch Artikel ohne Lagerbestand anzeigen

        if (orderByDate)
        {
            this.orderByToConsumeDate = true
        }

        if (!this.category.isNullOrEmpty())
        {
            binding.StorageItemListAppBar.subtitle = this.category
        }

        if (!this.subCategory.isNullOrEmpty())
        {
            binding.StorageItemListAppBar.subtitle =  this.category + " / " + this.subCategory
        }

        if (!this.eanCode.isNullOrEmpty())
        {
            binding.StorageItemListAppBar.subtitle = "EAN Code: " + this.eanCode
        }

        this.initializeStorageFilter()

        binding.StorageItemListFABAdd.setOnClickListener {
            this.selectArticle()
        }

        this.showStorageItemList()
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean
    {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.storage_item_list_menu, menu)

        val sortMenuItem = menu.findItem(R.id.StorageItemList_Sort)
        if (orderByToConsumeDate) {
            sortMenuItem.setIcon(R.drawable.baseline_sort_date_white_24)
        }
        else {
            sortMenuItem.setIcon((R.drawable.baseline_sort_az_white_24))
        }

        val searchItem = menu.findItem(R.id.StorageItemList_Search)
        val searchView = searchItem.actionView as SearchView
        searchView.setOnQueryTextListener(object : SearchView.OnQueryTextListener {
            override fun onQueryTextSubmit(query: String?): Boolean {
                // Optional: Suche abschließen
                return true
            }

            override fun onQueryTextChange(newText: String?): Boolean {
                lastSearchText = newText.orEmpty()
                showStorageItemList()
                return true
            }
        })

        val editText = Tools.findFirstEditText(searchView)

        editText?.setTextColor(getColor(R.color.white))

        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {

        when (item.itemId) {
            R.id.StorageItemList_Add -> {
                this.selectArticle()
                return true
            }
            R.id.StorageItemList_Sort -> {
                orderByToConsumeDate = !orderByToConsumeDate
                this.showStorageItemList()
                this.invalidateOptionsMenu()

                Settings.putBoolean("StorageItemListOrder", orderByToConsumeDate)

                return true
            }
            R.id.ArticleList_Menu_Share -> {
                shareList()
                return true
            }
            else -> return false
        }
    }

    private fun shareList()
    {
        if (MainActivity.IsGooglePlayPreLaunchTestMode)
        {
            return
        }

        val adapter = binding.StorageItemView.adapter as StorageItemViewAdapter
        val storageItemList = adapter.getStorageItemList()

        var text = ""

        for(item in storageItemList)
        {
            if (item.heading.isNotEmpty())      text += item.heading + "\n"
            if (item.storageInfo.isNotEmpty()) text += item.storageInfo + "\n"
            //if (item. .isNotEmpty()) text += item.quantityText + "\n"

            text += "\n"
        }

        val now = LocalDateTime.now()

        val subject = String.format("%s - %s",
            resources.getString(R.string.Main_Button_Lagerbestand),
            now.format(DateTimeFormatter.ofPattern("dd.MM.yyyy HH:mm")))

        val intent = Intent()
        intent.action = Intent.ACTION_SEND
        intent.putExtra(Intent.EXTRA_SUBJECT, subject)
        intent.putExtra(Intent.EXTRA_TEXT, text)
        intent.type = "text/plain"

        startActivity(intent)
    }

    private fun initializeStorageFilter()
    {
        val storageList = mutableListOf<String?>()
        storageList.add(this.resources.getString(R.string.StorageItem_AllStoragesStorage))
        storageList.add(this.resources.getString(R.string.StorageItem_NoStoragesStorage))
        storageList.addAll(Database.getStorageNames(true))

        if (storageList.count() > 1)
        {
            binding.StorageItemListSelectStorageSection.visibility = View.VISIBLE

            val adapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, storageList)
            adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
            binding.StorageItemListStorages.adapter = adapter
        }
        else
        {
            binding.StorageItemListSelectStorageSection.visibility = View.GONE
        }
    }

    fun showStorageItemList() {

        binding.StorageItemListLoadingIndicator.visibility = View.VISIBLE
        binding.StorageItemView.visibility = View.GONE

        lifecycleScope.launch {

            val storageItemQuantityList = withContext(Dispatchers.IO) {

                val list = Database.getStorageItemList(
                    category,
                    subCategory,
                    eanCode,
                    showEmptyStorageArticles,
                    lastSearchText,
                    storageNameFilter,
                    withoutStorage,
                    orderByToConsumeDate
                )

                list.forEach {
                    it.getBestBeforeItemQuantity(storageNameFilter, withoutStorage)
                }

                list.removeAll {
                    it.shouldBeRemoved(filterExpiryDate)
                }

                list
            }

            val adapter = StorageItemViewAdapter(
                storageItemQuantityList,
                ::onOpenStorageItemDetails,
                storageNameFilter,
                withoutStorage
            )
            adapter.optionSelect =
                { articleId, control ->
                    this@StorageItemListActivity.showOptionPopUp(articleId, control)
                }

            binding.StorageItemView.layoutManager = LinearLayoutManager(this@StorageItemListActivity)
            binding.StorageItemView.adapter = adapter

            listViewState?.let { state ->
                binding.StorageItemView.layoutManager?.onRestoreInstanceState(state)
            }

            val statistic = StockStatistic()
            val text = statistic.getText(storageItemQuantityList, this@StorageItemListActivity)
            binding.StorageItemListFooter.text = text

            binding.StorageItemListLoadingIndicator.visibility = View.GONE
            binding.StorageItemView.visibility = View.VISIBLE
        }
    }

    fun onOpenStorageItemDetails(articleId: Int)
    {
        val intent = Intent(this, StorageItemInventoryActivity::class.java)
        intent.putExtra("ArticleId", articleId)
        storageItemLauncher.launch(intent)

        listViewState = binding.StorageItemView.layoutManager?.onSaveInstanceState()
    }

    private val storageItemLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {

                this.showStorageItemList()
                binding.StorageItemView.layoutManager?.onRestoreInstanceState(listViewState)

            }
        }

    private fun spinnerStorageItemSelected(position: Int) {
        var newStorageName = ""
        var withoutStorage = false

        if (position == 1)
        {
            withoutStorage = true
        }

        if (position > 1)
        {
            newStorageName = binding.StorageItemListStorages.selectedItem.toString()
        }

        if ((newStorageName != this.storageNameFilter) || (withoutStorage != this.withoutStorage))
        {
            this.storageNameFilter = newStorageName
            this.withoutStorage    = withoutStorage

            this.showStorageItemList()
        }
    }

    fun selectArticle()
    {
        // Select Article
        val articleListIntent = Intent(this, ArticleListActivity::class.java)
        articleListIntent.putExtra("SelectArticleOnly", true)
        articleListIntent.putExtra("NotInStorage", true)
        articleListLauncher.launch(articleListIntent)

        listViewState = binding.StorageItemView.layoutManager?.onSaveInstanceState()

    }

    private val articleListLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {

                if (result.data == null)
                    return@registerForActivityResult

                val id = result.data!!.getIntExtra("ArticleId", -1)
                if (id == -1)
                    return@registerForActivityResult

                val storageItemQuantity = Intent(this, StorageItemInventoryActivity::class.java)
                storageItemQuantity.putExtra("ArticleId", id)
                storageItemQuantity.putExtra("EditMode",  true)
                storageItemInventoryLauncher.launch(storageItemQuantity)
            }
        }

    val storageItemInventoryLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {
                this.showStorageItemList()
                this.initializeStorageFilter()

                //binding.StorageItemView.layoutManager?.onRestoreInstanceState(listViewState)
            }
        }

    @SuppressLint("RestrictedApi")
    fun showOptionPopUp(articleId: Int, holder: StorageItemViewHolder)
    {
        val popupMenu = PopupMenu(holder.itemView.context, holder.option)
        popupMenu.menuInflater.inflate(R.menu.storage_item_list_contextmenu, popupMenu.menu)

        if (popupMenu.menu is MenuBuilder) {
            (popupMenu.menu as MenuBuilder).setOptionalIconsVisible(true)
        }

        popupMenu.setOnMenuItemClickListener { menuItem: MenuItem ->
            when (menuItem.itemId) {
                R.id.StorageItemList_ContextMenu_Artikelangaben -> {
                    val articleDetails = Intent(holder.itemView.context, ArticleDetailsActivity::class.java)
                    articleDetails.putExtra("ArticleId", articleId)
                    detailLauncher.launch(articleDetails)

                    true
                }
                R.id.StorageItemList_ContextMenu_AufEinkaufszettel -> {
                    AddToShoppingListDialog.showDialog(
                        holder.itemView.context as Activity,
                        articleId,
                        holder.minQuantity,
                        holder.prefQuantity,
                        { refreshStorageItemList() }
                    )
                    true
                }
                else -> false
            }
        }
        popupMenu.show()
    }

    private val detailLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {
                refreshStorageItemList()
            }
        }

    private fun refreshStorageItemList()
    {
        listViewState = binding.StorageItemView.layoutManager?.onSaveInstanceState()
        this.showStorageItemList()
        binding.StorageItemView.layoutManager?.onRestoreInstanceState(listViewState)
    }
}
