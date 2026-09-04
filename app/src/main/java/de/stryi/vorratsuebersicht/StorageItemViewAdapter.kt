package de.stryi.vorratsuebersicht

import android.content.Intent
import android.graphics.BitmapFactory
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.database.Records.ArticleInfo
import de.stryi.vorratsuebersicht.tools.Tools


class StorageItemViewAdapter(
    private val storageItems: List<ArticleInfo>,
    private val onItemClick: (articleId: Int) -> Unit,
    private val storageNameFilter: String?,
    private val withoutStorage: Boolean
) :
    RecyclerView.Adapter<StorageItemViewAdapter.StorageItemViewHolder>()
{
    var optionSelect: ((Int, StorageItemViewHolder) -> Unit)? = null

    class StorageItemViewHolder(view: View) : RecyclerView.ViewHolder(view) {

        val image: ImageView = view.findViewById(R.id.StorageItemListView_Image)

        val header:  TextView = view.findViewById(R.id.StorageItemListView_TextHeader)
        val details: TextView = view.findViewById(R.id.StorageItemListView_TextDetails)
        val error:   TextView = view.findViewById(R.id.StorageItemListView_TextError)
        val warning: TextView = view.findViewById(R.id.StorageItemListView_TextWarning)
        val info:    TextView = view.findViewById(R.id.StorageItemListView_TextInfo)

        val option: TextView = view.findViewById(R.id.StorageItemListView_Option)

        val onShoppingList: ImageView = view.findViewById(R.id.StorageItemListView_OnShoppingList)
        val shoppingQuantity: TextView = view.findViewById(R.id.StorageItemListView_ShoppingQuantity)

        var minQuantity: Int? = null
        var prefQuantity: Int? = null
    }

    override fun onCreateViewHolder(
        parent: ViewGroup,
        viewType: Int
    ): StorageItemViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.storage_item_list_view, parent, false)
        return StorageItemViewHolder(view)
    }

    override fun onBindViewHolder(
        holder: StorageItemViewHolder,
        position: Int
    ) {

        val storageItem = storageItems[position]

        holder.itemView.tag = storageItem.articleId
        holder.header.text = storageItem.heading
        holder.details.text = storageItem.storageInfo

        this.loadBestBeforeInformation(storageItem, holder)

        holder.info.visibility    = View.GONE
        holder.warning.visibility = View.GONE
        holder.error.visibility   = View.GONE

        if (!holder.info.text.isEmpty())    { holder.info.visibility    = View.VISIBLE }
        if (!holder.warning.text.isEmpty()) { holder.warning.visibility = View.VISIBLE }
        if (!holder.error.text.isEmpty())   { holder.error.visibility   = View.VISIBLE }

        holder.onShoppingList.visibility   = if (storageItem.isOnShoppingList) View.VISIBLE else View.GONE
        holder.shoppingQuantity.visibility = if (storageItem.isOnShoppingList) View.VISIBLE else View.GONE
        holder.shoppingQuantity.text       = storageItem.shoppingQuantityText

        holder.minQuantity  = storageItem.minQuantity
        holder.prefQuantity = storageItem.prefQuantity

        val articleImage = Database.getArticleImage(storageItem.articleId, false)

        if (articleImage == null) {
            holder.image.setImageResource(R.drawable.photo_camera_24px)
            holder.image.alpha = 0.2.toFloat()
            holder.image.setOnClickListener { }
        } else {
            val bitmap = BitmapFactory.decodeByteArray(
                articleImage.imageSmall,
                0,
                articleImage.imageSmall!!.size
            )
            holder.image.setImageBitmap(bitmap)
            holder.image.alpha = 1.toFloat()
            holder.image.setOnClickListener {
                val intent = Intent(holder.image.context, ArticleImageActivity::class.java)
                intent.putExtra("ArticleId", storageItem.articleId)
                holder.image.context.startActivity(intent)
            }
        }

        holder.itemView.setOnClickListener {
            onItemClick(storageItem.articleId)
        }

        holder.itemView.setOnLongClickListener { _ ->
            this.showContextPopup(holder)
            true
        }

        holder.option.setOnClickListener { _ ->
            this.showContextPopup(holder)
        }
    }

    fun showContextPopup(holder: StorageItemViewHolder)
    {
        val articleId = holder.itemView.tag as Int
        this.optionSelect?.invoke(articleId, holder)
    }

    override fun getItemCount(): Int {
        return storageItems.size
    }

    fun loadBestBeforeInformation(storageItem: ArticleInfo, holder: StorageItemViewHolder)
    {
        val  storageItemBestList = storageItem.getBestBeforeItemQuantity(
            storageNameFilter,
            withoutStorage)

        if (storageItemBestList.count() == 0)
        {
            return
        }

        var info    = ""
        var warning = ""
        var error   = ""

        val withNoDate   = "%s ohne Ablaufdatum"
        val withThisDate = "%s mit Ablaufdatum %s"

        for (result in storageItemBestList)
        {
            val quantityText = Tools.formatNumber(result.quantity)

            if (result.warningLevel == 0)
            {
                if (result.bestBefore == null)
                {
                    if (info.isNotEmpty()) info += "\r\n"
                    info += String.format(withNoDate, quantityText)
                }
                else
                {
                    if (info.isNotEmpty()) info += "\r\n"
                    info += String.format(withThisDate, quantityText, Tools.toHumanString(result.bestBefore))
                }
            }

            if (result.warningLevel == 1)
            {
                if (warning.isNotEmpty()) warning += "\r\n"
                warning += String.format(withThisDate, quantityText, Tools.toHumanString(result.bestBefore))
            }

            if (result.warningLevel == 2)
            {
                if (error.isNotEmpty()) error += "\r\n"
                error += String.format(withThisDate, quantityText, Tools.toHumanString(result.bestBefore))
            }
        }

        holder.info.text    = info
        holder.warning.text = warning
        holder.error.text   = error
    }

    fun getStorageItemList() : List<ArticleInfo>
    {
        return storageItems
    }
}