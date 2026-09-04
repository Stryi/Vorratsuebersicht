package de.stryi.vorratsuebersicht

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
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import androidx.appcompat.widget.PopupMenu
import androidx.core.content.ContextCompat
import androidx.recyclerview.widget.DividerItemDecoration
import androidx.recyclerview.widget.LinearLayoutManager
import de.stryi.vorratsuebersicht.ArticleListViewAdapter.ArticleViewHolder
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.databinding.ArticleListBinding
import de.stryi.vorratsuebersicht.tools.AddToShoppingListDialog
import de.stryi.vorratsuebersicht.tools.CategoryItem
import de.stryi.vorratsuebersicht.tools.Tools
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter


class ArticleListActivity : AppCompatActivity() {

    private var selectArticleOnly = false
    private var category = ""
    private var subCategory = ""
    private var withoutCategory = false
    private var notInStorage = false
    private var notInShoppingList = false
    private val eanCode: String? = null
    private var lastSearchText: String? = ""
    private var specialFilter = 0

    private var listViewState: Parcelable? = null
    private lateinit var binding: ArticleListBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = ArticleListBinding.inflate(layoutInflater)
        setContentView(binding.root)

        this.setSupportActionBar(binding.ArticleListAppBar)

        binding.ArticleListAppBar.setNavigationOnClickListener { finish() }
        binding.ArticleListAppBar.overflowIcon?.setTint(getColor(R.color.Application_ActionBar_TextColor))

        this.selectArticleOnly = intent.getBooleanExtra("SelectArticleOnly", false)
        this.notInStorage      = intent.getBooleanExtra("NotInStorage", false)
        this.notInShoppingList = intent.getBooleanExtra("NotInShoppingList", false)

        if (this.selectArticleOnly)
        {
            binding.ArticleListAppBar.title = "Auswahl Artikel"
        }

        // Trennlinie in der Liste
        val divider = DividerItemDecoration(this, DividerItemDecoration.VERTICAL)
        ContextCompat.getDrawable(this, R.drawable.divider)?.let { divider.setDrawable(it) }
        binding.ArticleList.addItemDecoration(divider)
        binding.ArticleList.isClickable = true

        // "Pull to Refresh" Funktionalität
        binding.ArticleListSwipeRefreshLayout.setOnRefreshListener {
            this.showArticleList()
            this.loadCategoryList()
            binding.ArticleListSwipeRefreshLayout.isRefreshing = false
        }

        // Kategorie Auswahl
        this.loadCategoryList()

        binding.ArticleListCategories.onItemSelectedListener =
            object : AdapterView.OnItemSelectedListener {
                override fun onItemSelected(parent: AdapterView<*>, view: View?, position: Int, id: Long) {
                    val categoryItem = parent.getItemAtPosition(position) as CategoryItem
                    spinnerCategoryItemSelected(position, categoryItem.category, categoryItem.subCategory)
                }

                override fun onNothingSelected(parent: AdapterView<*>) {
                    // Optional: Verhalten, wenn nichts ausgewählt ist
                    spinnerCategoryItemSelected(0, "", "")
                }
            }

        // Spezialfilter entfernen
        binding.ArticleListFilterClear.setOnClickListener {
            this.specialFilter = 0
            this.binding.ArticleListFilter.text = ""
            this.binding.ArticleListFilterBanner.visibility = View.GONE

            this.showArticleList()
            this.loadCategoryList()
        }

        binding.ArticleListFABAdd.setOnClickListener {
            onCreateArticle()
        }

        showArticleList()
    }

    private fun loadCategoryList()
    {
        val categoryList = mutableListOf<CategoryItem?>()
        categoryList.add(CategoryItem(resources.getString(R.string.ArticleList_AllCategories)))
        categoryList.add(CategoryItem(resources.getString(R.string.ArticleList_NoCategories)))
        categoryList.addAll(Database.getCategoryAndSubcategoryNames())

        val dataAdapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, categoryList)
        dataAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)

        binding.ArticleListCategories.adapter = dataAdapter
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.article_list_menu, menu)

        val searchItem = menu.findItem(R.id.ArticleList_Menu_Search)
        val searchView = searchItem.actionView as SearchView

        val editText = Tools.findFirstEditText(searchView)

        editText?.setTextColor(getColor(R.color.white))

        searchView.setOnQueryTextListener(object : SearchView.OnQueryTextListener {
            override fun onQueryTextSubmit(query: String?): Boolean {
                // Optional: Suche abschließen
                return true
            }

            override fun onQueryTextChange(newText: String?): Boolean {
                lastSearchText = newText.orEmpty()
                showArticleList()
                return true
            }
        })

        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        when (item.itemId) {
            R.id.ArticleList_Menu_Add -> {
                onCreateArticle()
                return true
            }
            R.id.ArticleList_Menu_Filter -> {
                // Spezialfilter auswählen
                this.filterArticleList()
                return true
            }
            R.id.ArticleList_Menu_Share -> {
                shareList()
                return true
            }
            else -> return false
        }
    }

    private fun filterArticleList()
    {
        // Auswahl Spezialfilter
        val actions = resources.getTextArray(R.array.ArticleListeSpecialFilter)

        val adapter = ArrayAdapter(
            this,
            R.layout.dialog_item,
            R.id.DialogItem_Text1,
            actions)

        val builder = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        builder.setAdapter(adapter) { _, which ->
            this.specialFilter = which + 1

            binding.ArticleListFilter.text = actions[which]
            binding.ArticleListFilterBanner.visibility = View.VISIBLE

            this.showArticleList()
        }
        builder.show()
    }

    fun showArticleList()
    {
        val articleList = Database.getArticleList(
            this.category,
            this.subCategory,
            this.eanCode,
            this.notInStorage,
            this.notInShoppingList,
            this.withoutCategory,
            this.specialFilter,
            this.lastSearchText)

        val adapter = ArticleListViewAdapter(articleList, this::onOpenArticleDetails)
        adapter.optionSelect =
            { articleId, anchor ->
                this.showOptionPopUp(articleId, anchor)
            }

        binding.ArticleList.layoutManager = LinearLayoutManager(this)
        binding.ArticleList.adapter = adapter

        val status = if (articleList.count() == 1)
            resources.getString(R.string.ArticleListSummary_Position)
        else
            resources.getString(R.string.ArticleListSummary_Positions)

        binding.ArticleListFooter.text = String.format(status, articleList.count())
    }

    fun onCreateArticle()
    {
        listViewState = binding.ArticleList.layoutManager?.onSaveInstanceState()

        val intent = Intent(this, ArticleDetailsActivity::class.java)

        // Zum Voranstellen bei Neuanlage
        intent.putExtra("Category",    this.category)
        intent.putExtra("SubCategory", this.subCategory)
        if (this.selectArticleOnly)
        {
            intent.putExtra("NoStorageQuantity", true)
            intent.putExtra("NoDeleteArticle",   true)
        }

        detailLauncher.launch(intent)
    }

    fun onOpenArticleDetails(articleId: Int)
    {
        if (this.selectArticleOnly)
        {
            val intent = Intent()
            intent.putExtra("ArticleId", articleId)
            setResult(RESULT_OK, intent)
            finish()
            return
        }

        this.showArticleDetails(articleId)
    }

    fun showArticleDetails(articleId: Int) {

        val articleDetails = Intent(this, ArticleDetailsActivity::class.java)
        articleDetails.putExtra("ArticleId", articleId)

        detailLauncher.launch(articleDetails)

        listViewState = binding.ArticleList.layoutManager?.onSaveInstanceState()
    }


    private val detailLauncher =
        registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result ->
            if (result.resultCode == RESULT_OK) {
                /*
                var articleId = result.data?.getIntExtra("ArticleId", 0)
                if (articleId == 0)
                    return@registerForActivityResult

                var article = Database.getArticle(articleId!!)

                var adapter = binding.ArticleList.adapter as ArticleListViewAdapter

                adapter.notifyItemChanged(index)
                */

                showArticleList()
                loadCategoryList()

                binding.ArticleList.layoutManager?.onRestoreInstanceState(listViewState)
            }
        }

     fun spinnerCategoryItemSelected(position: Int, categoryText: String, subCategoryText: String)
     {
         val withoutCategoryNew = position == 1

         if ((categoryText != this.category) || (subCategoryText != this.subCategory) || withoutCategoryNew != this.withoutCategory)
         {
             this.category        = categoryText
             this.subCategory     = subCategoryText
             this.withoutCategory = withoutCategoryNew

             this.showArticleList()
         }
     }

    fun shareList()
    {
        if (MainActivity.IsGooglePlayPreLaunchTestMode)
        {
            return
        }

        val list = Database.getArticleList(
            this.category,
            this.subCategory,
            this.eanCode,
            this.notInStorage,
            this.notInShoppingList,
            this.withoutCategory,
            this.specialFilter,
            lastSearchText)

        var text = ""

        for(article in list)
        {
            if (article.heading.isNotEmpty())     text += article.heading + "\n"
            if (article.articleInfo.isNotEmpty()) text += article.articleInfo + "\n"
            if (article.notesText.isNotEmpty())   text += article.notesText + "\n"
            text += "\n"
        }

        text += binding.ArticleListFooter.text

        val now = LocalDateTime.now()

        val subject = String.format("%s - %s",
            resources.getString(R.string.Main_Button_Artikelangaben),
            now.format(DateTimeFormatter.ofPattern("dd.MM.yyyy HH:mm")))

        val intent = Intent()
        intent.action = Intent.ACTION_SEND
        intent.putExtra(Intent.EXTRA_SUBJECT, subject)
        intent.putExtra(Intent.EXTRA_TEXT, text)
        intent.type = "text/plain"

        startActivity(intent)
    }

    @SuppressLint("RestrictedApi")
    fun showOptionPopUp(articleId: Int, anchor: ArticleViewHolder)
    {
        val popupMenu = PopupMenu(this, anchor.option)
        popupMenu.menuInflater.inflate(R.menu.article_list_contextmenu, popupMenu.menu)

        if (popupMenu.menu is MenuBuilder) {
            (popupMenu.menu as MenuBuilder).setOptionalIconsVisible(true)
        }

        popupMenu.setOnMenuItemClickListener { menuItem: MenuItem ->
            when (menuItem.itemId) {
                R.id.ArticleList_ContextMenu_Lagerbestand -> {
                    val storageDetails = Intent(anchor.itemView.context, StorageItemInventoryActivity::class.java)
                    storageDetails.putExtra("ArticleId", articleId)
                    storageDetailsLauncher.launch(storageDetails)
                    true
                }
                R.id.ArticleList_ContextMenu_AufEinkaufszettel -> {
                    AddToShoppingListDialog.showDialog(
                        anchor.itemView.context as Activity,
                        articleId,
                        anchor.minQuantity,
                        anchor.prefQuantity,
                        { refreshArticleList() }
                    )
                    true
                }
                else -> false
            }
        }
        popupMenu.show()
    }

    private val storageDetailsLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult())
    { result ->
        if (result.resultCode == RESULT_OK)
        {
            this.refreshArticleList()
        }
    }

    private fun refreshArticleList()
    {
        listViewState = binding.ArticleList.layoutManager?.onSaveInstanceState()
        this.showArticleList()
        binding.ArticleList.layoutManager?.onRestoreInstanceState(listViewState)
    }
}