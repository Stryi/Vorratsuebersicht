package de.stryi.vorratsuebersicht

import android.annotation.SuppressLint
import androidx.appcompat.app.AlertDialog
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
import androidx.core.content.ContextCompat
import androidx.recyclerview.widget.DividerItemDecoration
import androidx.recyclerview.widget.LinearLayoutManager
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.database.Records.ShoppingItem
import de.stryi.vorratsuebersicht.databinding.ShoppingItemListBinding
import de.stryi.vorratsuebersicht.tools.Settings
import de.stryi.vorratsuebersicht.tools.Tools
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

class ShoppingItemListActivity : AppCompatActivity()
{
    private var supermarket: String? = null
    private var listViewState: Parcelable? = null
    private var lastSearchText = ""

    companion object {
        var orderBy = 0
    }

    private lateinit var binding: ShoppingItemListBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = ShoppingItemListBinding.inflate(layoutInflater)
        setContentView(binding.root)

        this.setSupportActionBar(binding.ShoppingItemListAppBar)

        binding.ShoppingItemListAppBar.setNavigationOnClickListener { finish() }
        binding.ShoppingItemListAppBar.overflowIcon?.setTint(getColor(R.color.Application_ActionBar_TextColor))

        binding.ShoppingItemListSpinner.onItemSelectedListener =
            object : AdapterView.OnItemSelectedListener {
                override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long)
                {
                    spinnerSupermarketItemSelected(position)
                }

                override fun onNothingSelected(parent: AdapterView<*>?) {
                    // optional, kann leer bleiben
                }
            }

        // Trennlinie in der Liste
        val divider = DividerItemDecoration(this, DividerItemDecoration.VERTICAL)
        ContextCompat.getDrawable(this, R.drawable.divider)?.let { divider.setDrawable(it) }
        binding.ShoppingItemList.addItemDecoration(divider)
        binding.ShoppingItemList.isClickable = true

        // „Pull to Refresh“ Funktionalität
        binding.ShoppingItemListSwipeRefreshLayout.setOnRefreshListener {
            this.showShoppingList()
            binding.ShoppingItemListSwipeRefreshLayout.isRefreshing = false
        }

        // FloatingActionButton Klick-Listener
        binding.ShoppingItemListAddFab.setOnClickListener {
            this.selectArticle()
        }

        this.showShoppingList()
        this.loadSupermarketList()
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean
    {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.shopping_item_list_menu, menu)

        val sortMenuItem = menu.findItem(R.id.ShoppingList_Sort)

        when(orderBy) {
            1 -> sortMenuItem.setIcon(R.drawable.baseline_sort_check_white_24)
            2 -> sortMenuItem.setIcon(R.drawable.baseline_sort_shop_white_24)
            3 -> sortMenuItem.setIcon(R.drawable.baseline_sort_time_white_24)
            4 -> sortMenuItem.setIcon(R.drawable.baseline_sort_az_white_24)
        }

        val viewType = menu.findItem(R.id.ShoppingList_Sparse)
        viewType.isChecked = ShoppingItemViewAdapter.sparseView == 1

        val searchItem = menu.findItem(R.id.ShoppingList_Search)
        val searchView = searchItem.actionView as SearchView
        searchView.setOnQueryTextListener(object : SearchView.OnQueryTextListener {
            override fun onQueryTextSubmit(query: String?): Boolean {
                // Optional: Suche abschließen
                return true
            }

            override fun onQueryTextChange(newText: String?): Boolean {
                lastSearchText = newText.orEmpty()
                showShoppingList()
                return true
            }
        })

        val editText = Tools.findFirstEditText(searchView)

        editText?.setTextColor(getColor(R.color.white))

        return true
    }

    override fun onOptionsItemSelected(item: MenuItem) : Boolean
    {
        when(item.itemId)
        {
            R.id.ShoppingList_Add -> {
                this.selectArticle()
                return true
            }

            R.id.ShoppingList_Sort -> {
                orderBy++
                if (orderBy > 4)
                    orderBy = 1

                this.showShoppingList()
                this.invalidateOptionsMenu()

                Settings.putInt("ShoppingListOrder", orderBy)

                return true
            }

            R.id.ShoppingList_Share -> {
                shareList()
                return true
            }

            R.id.ShoppingList_Sparse -> {
                ShoppingItemViewAdapter.sparseView = 1 - ShoppingItemViewAdapter.sparseView
                this.showShoppingList()
                this.invalidateOptionsMenu()

                Settings.putInt("ShoppingListViewType", ShoppingItemViewAdapter.sparseView)
            }

        }
        return false
    }

    private fun shareList()
    {
        if (MainActivity.IsGooglePlayPreLaunchTestMode)
        {
            return
        }

        val adapter = binding.ShoppingItemList.adapter as ShoppingItemViewAdapter
        val shoppingList = adapter.getShoppingList()

        var text = ""

        for(item in shoppingList)
        {
            if (item.heading.isNotEmpty())      text += item.heading + "\n"
            if (item.shoppingInfo.isNotEmpty()) text += item.shoppingInfo + "\n"
            if (item.quantityText.isNotEmpty()) text += item.quantityText + "\n"

            text += "\n"
        }
        text += binding.ShoppingItemListFooter.text

        val now = LocalDateTime.now()

        val subject = String.format("%s - %s",
            resources.getString(R.string.Main_Button_Einkaufsliste),
            now.format(DateTimeFormatter.ofPattern("dd.MM.yyyy HH:mm")))

        val intent = Intent()
        intent.action = Intent.ACTION_SEND
        intent.putExtra(Intent.EXTRA_SUBJECT, subject)
        intent.putExtra(Intent.EXTRA_TEXT, text)
        intent.type = "text/plain"

        startActivity(intent)
    }

    private fun loadSupermarketList() {

        val supermarketList = mutableListOf<String?>()
        supermarketList.add(this.resources.getString(R.string.ShoppingList_AllSupermarkets))
        supermarketList.addAll(Database.getSupermarketNames(true))

        if (supermarketList.count() == 1)
        {
            binding.ShoppingItemListSelectSupermarket.visibility = View.GONE
            this.supermarket = null
        }

        if (supermarketList.count() > 1)
        {
            binding.ShoppingItemListSelectSupermarket.visibility = View.VISIBLE

            val dataAdapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, supermarketList)
            dataAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
            binding.ShoppingItemListSpinner.adapter = dataAdapter
        }
    }

    private fun showShoppingList() {

        val shoppingList = Database.getShoppingList(
            this.supermarket,
            this.lastSearchText,
            orderBy)

        val adapter = ShoppingItemViewAdapter(shoppingList,
            this::onOpenShoppingItemDetails,
            this::onRefresh)

        this.listViewState = binding.ShoppingItemList.layoutManager?.onSaveInstanceState()

        binding.ShoppingItemList.layoutManager = LinearLayoutManager(this)
        binding.ShoppingItemList.adapter = adapter

        binding.ShoppingItemList.layoutManager?.onRestoreInstanceState(listViewState)

        this.updateStatistic()
    }

    fun onOpenShoppingItemDetails(shoppingItem: ShoppingItem)
    {
        val removeText    = this.resources.getString(R.string.ShoppingList_Remove)
        val toStorage     = this.resources.getString(R.string.ShoppingList_ToStorage)
        val articleDetail = this.resources.getString(R.string.ShoppingList_ArticleDetails)
        val bought        = this.resources.getString(R.string.ShoppingList_MarkAsBought)

        val actions = arrayOf("+10", "+1", "-1", "-10", removeText, toStorage, articleDetail, bought)

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            actions)

        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setTitle(shoppingItem.heading)
        dialog.setAdapter(adapter) { _, which ->
            when (which) {
                0 -> { // +10
                    Database.addToShoppingList(shoppingItem.articleId, 10.0)
                    this.showShoppingList()
                }

                1 -> { // +1
                    Database.addToShoppingList(shoppingItem.articleId, 1.0)
                    this.showShoppingList()
                }

                2 -> { // -1
                    Database.addToShoppingList(shoppingItem.articleId, -1.0)
                    this.showShoppingList()
                }

                3 -> { // -10
                    Database.addToShoppingList(shoppingItem.articleId, -10.0)
                    this.showShoppingList()
                }

                4 -> { // Entfernen (gekauft)
                    Database.removeFromShoppingList(shoppingItem.articleId)
                    this.loadSupermarketList()
                    this.showShoppingList()
                }
                5 -> { // Ins Lagerbestand
                    var shoppingItemCount = shoppingItem.quantity
                    if (shoppingItemCount == 0.00)
                        shoppingItemCount = 1.00

                    val storageInventory = Intent(this, StorageItemInventoryActivity::class.java)
                    storageInventory.putExtra("ArticleId", shoppingItem.articleId)
                    storageInventory.putExtra("EditMode", true)
                    storageInventory.putExtra("Quantity", shoppingItemCount)
                    storageItemInventoryLauncher.launch(storageInventory)
                }
                6 -> { // Artikelangaben
                    val articleDetails = Intent(this, ArticleDetailsActivity::class.java)
                    articleDetails.putExtra("ArticleId", shoppingItem.articleId)
                    startActivity(articleDetails)
                }
                7 -> { // Als 'Gekauft' markieren
                    Database.setShoppingItemBought(shoppingItem.articleId, true)
                    shoppingItem.bought = true
                    this.showShoppingList()
                }
            }
        }
        dialog.show()
    }

    fun onRefresh()
    {
        this.showShoppingList()
    }

    private fun spinnerSupermarketItemSelected(position: Int) {
        var newSupermarketName = ""

        if (position > 0)
        {
            newSupermarketName = binding.ShoppingItemListSpinner.selectedItem.toString()
        }

        if (newSupermarketName != this.supermarket)
        {
            this.supermarket = newSupermarketName
            this.showShoppingList()
        }
    }


    @SuppressLint("StringFormatInvalid")
    fun updateStatistic()
    {
        var sumQuantity = 0.00
        var sumAmount   = 0.00
        var toPay       = 0.00
        var sumNoPrice  = 0.00

        val adapter = binding.ShoppingItemList.adapter as ShoppingItemViewAdapter
        val shoppingList = adapter.getShoppingList()

        for(item in shoppingList)
        {
            sumQuantity += item.quantity!!
            if (item.price != null && item.price!! > 0.00)
            {
                sumAmount += item.quantity!! * item.price!!
                if (item.bought == true)
                {
                    toPay += item.quantity!! * item.price!!
                }
            }
            else
            {
                sumNoPrice++
            }
        }

        var status: String
        if (shoppingList.count() == 1)
            status = Tools.formatResource(this, R.string.ShoppingListSummary_Position, shoppingList.count())
        else
            status = Tools.formatResource(this, R.string.ShoppingListSummary_Positions, shoppingList.count())

        if (sumQuantity > 0)
        {
            status += ", " + Tools.formatResource(this, R.string.ShoppingListSummary_Quantity, sumQuantity)
        }
        if (sumAmount   > 0)
        {
            status += ", " + Tools.formatResource(this, R.string.ShoppingListSummary_Amount, sumAmount)
        }

        if (sumNoPrice == 1.00)
        {
            status += ", " + Tools.formatResource(this, R.string.ShoppingListSummary_WithoutPrice, sumNoPrice)
        }

        if (sumNoPrice > 1.00)
        {
            status += ", " + Tools.formatResource(this, R.string.ShoppingListSummary_WithoutPriceN, sumNoPrice)
        }

        if (toPay > 0)
        {
            status += "\n" + Tools.formatResource(this, R.string.ShoppingListSummary_ToPay, toPay)
        }

        binding.ShoppingItemListFooter.text = status
    }

    fun selectArticle()
    {
        // Select Article
        val articleListIntent = Intent(this, ArticleListActivity::class.java)
        articleListIntent.putExtra("SelectArticleOnly", true)
        articleListIntent.putExtra("NotInShoppingList", true)
        articleListLauncher.launch(articleListIntent)
    }

    private val articleListLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {

                if (result.data == null)
                    return@registerForActivityResult

                val id = result.data!!.getIntExtra("ArticleId", -1)
                if (id == -1)
                    return@registerForActivityResult

                Database.addToShoppingList(id, 1.00)
                this.showShoppingList()
                this.loadSupermarketList()
            }
        }

    private val storageItemInventoryLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {

                if (result.data == null)
                    return@registerForActivityResult

                val id = result.data!!.getIntExtra("ArticleId", -1)
                if (id == -1)
                    return@registerForActivityResult

                Database.removeFromShoppingList(id)
                this.showShoppingList()
                this.loadSupermarketList()
            }
        }
}
