package de.stryi.vorratsuebersicht

import android.annotation.SuppressLint
import android.content.Intent
import android.os.Bundle
import android.view.Menu
import android.widget.ArrayAdapter
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.databinding.SubCategoryActivityBinding

class SubCategoryActivity : AppCompatActivity() {

    private lateinit var binding: SubCategoryActivityBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = SubCategoryActivityBinding.inflate(layoutInflater)
        setContentView(binding.root)

        this.setSupportActionBar(binding.SubCategoryActivityAppBar)
        binding.SubCategoryActivityAppBar.setNavigationOnClickListener { finish() }

        val category = intent.getStringExtra("Category")

        this.title = category

        val subCategories: MutableList<String> = mutableListOf()
        subCategories.add(this.resources.getString(R.string.AnySubCategory_ItemEntry))
        subCategories.addAll(Database.getSubcategoriesOf(category, true))

        val listAdapter = ArrayAdapter(this, android.R.layout.simple_list_item_1, subCategories)
        binding.SubCategoryList.adapter = listAdapter

        binding.SubCategoryList.setOnItemClickListener { _, _, position, _ ->

            val intent = Intent(this, StorageItemListActivity::class.java)

            intent.putExtra("Category",    category)

            if (position > 0)
            {
                intent.putExtra("SubCategory", subCategories[position])
            }

            startActivity(intent)
        }
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.menu_sub_category, menu)
        return true
    }

    /*
    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        when (item.itemId) {
            R.id.action_settings -> {
                startActivity(Intent(this, SettingsActivity::class.java))
                return true
            }
        }
        return super.onOptionsItemSelected(item)
    }
    */
}