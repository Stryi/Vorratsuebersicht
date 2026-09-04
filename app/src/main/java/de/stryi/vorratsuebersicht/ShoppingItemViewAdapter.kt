package de.stryi.vorratsuebersicht

import android.content.Intent
import android.graphics.BitmapFactory
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.CheckBox
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.database.Records.ShoppingItem

class ShoppingItemViewAdapter(
    private val shoppingItems: List<ShoppingItem>,
    private val onItemClick: (shoppingItem: ShoppingItem) -> Unit,
    private val onRefresh: () -> Unit)
    : RecyclerView.Adapter<ShoppingItemViewAdapter.ShoppingItemViewHolder>()
{
    companion object {
        var sparseView: Int = 1
    }

    class ShoppingItemViewHolder(view: View) : RecyclerView.ViewHolder(view) {

        val image: ImageView = view.findViewById(R.id.ShoppingItemListView_Image)

        val heading:    TextView = view.findViewById(R.id.ShoppingItemListView_Heading)
        val subHeading: TextView = view.findViewById(R.id.ShoppingItemListView_SubHeading)
        val quantity :  TextView = view.findViewById(R.id.ShoppingItemListView_Quantity)
        val bought: CheckBox = view.findViewById(R.id.ShoppingItemListView_Bought)
    }

    override fun onCreateViewHolder(
        parent: ViewGroup,
        viewType: Int
    ): ShoppingItemViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.shopping_item_list_view, parent, false)
        return ShoppingItemViewHolder(view)
    }

    override fun onBindViewHolder(
        holder: ShoppingItemViewHolder,
        position: Int)
    {
        val shoppingItem = shoppingItems[position]
        holder.heading.text     = shoppingItem.heading
        holder.subHeading.text  = shoppingItem.shoppingInfo
        holder.quantity.text    = shoppingItem.quantityText

        if (shoppingItem.quantity == null || shoppingItem.quantity == 0.0)
        {
            holder.quantity.visibility = View.INVISIBLE
        }
        else
        {
            holder.quantity.visibility = View.VISIBLE
        }

        holder.bought.isChecked = shoppingItem.bought == true
        holder.bought.setOnClickListener {
            shoppingItem.bought = holder.bought.isChecked
            Database.setShoppingItemBought(shoppingItem.articleId, shoppingItem.bought == true)
            this.onRefresh()
        }

        val articleImage = Database.getArticleImage(shoppingItem.articleId, false)

        if (articleImage == null)
        {
            holder.image.setImageResource(R.drawable.photo_camera_24px)
            holder.image.alpha = 0.2.toFloat()
            holder.image.setOnClickListener { }
        }
        else
        {
            val bitmap = BitmapFactory.decodeByteArray(articleImage.imageSmall, 0, articleImage.imageSmall!!.size)
            holder.image.setImageBitmap(bitmap)
            holder.image.alpha = 1.toFloat()
            holder.image.setOnClickListener {
                val intent = Intent(holder.image.context, ArticleImageActivity::class.java)
                intent.putExtra("ArticleId", shoppingItem.articleId)
                holder.image.context.startActivity(intent)
            }
        }

        holder.itemView.setOnClickListener {
            onItemClick(shoppingItem)
        }
    }

    override fun getItemCount(): Int {
        return shoppingItems.size
    }

    fun getShoppingList(): List<ShoppingItem> {
        return shoppingItems
    }
}