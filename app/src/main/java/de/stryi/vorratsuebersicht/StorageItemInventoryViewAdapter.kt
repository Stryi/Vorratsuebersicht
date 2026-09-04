package de.stryi.vorratsuebersicht

import androidx.appcompat.app.AlertDialog
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageButton
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import de.stryi.vorratsuebersicht.database.Records.StorageItem
import de.stryi.vorratsuebersicht.tools.Tools

class StorageItemInventoryViewAdapter(private val storageItems: MutableList<StorageItem>,
                                      private val onItemClicked: (action: ActionType, StorageItem) -> Unit) :
    RecyclerView.Adapter<StorageItemInventoryViewAdapter.StorageItemInventoryViewHolder>()
{
    var actionButtonsVisible = false

    enum class ActionType {
        CHANGE_QUANTITY,
        INCREASE_QUANTITY,
        DECREASE_QUANTITY,
        CHANGE_DATE,
        CHANGE_STORAGE,
        ADD_QUANTITY,
        REMOVE_QUANTITY
    }

    class StorageItemInventoryViewHolder(view: View) : RecyclerView.ViewHolder(view) {

        var quantity : TextView = view.findViewById(R.id.StorageItemQuantityList_Quantity)
        var date     : TextView = view.findViewById(R.id.StorageItemQuantityList_Date)
        var storage  : TextView = view.findViewById(R.id.StorageItemQuantityList_Storage)
        var buttonAdd    : ImageButton = view.findViewById(R.id.StorageItemQuantityList_Add)
        var buttonRemove : ImageButton = view.findViewById(R.id.StorageItemQuantityList_Remove)
        var labelChanged: TextView = view.findViewById(R.id.StorageItemQuantityList_Changed)
    }

    override fun onCreateViewHolder(
        parent: ViewGroup,
        viewType: Int
    ): StorageItemInventoryViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.storage_item_inventory_view, parent, false)
        return StorageItemInventoryViewHolder(view)

    }

    override fun onBindViewHolder(
        holder: StorageItemInventoryViewHolder,
        position: Int
    ) {
        val storageItem = storageItems[position]

        val dateText     = holder.itemView.context.getString(R.string.StorageItemQuantityList_BestBefore)
        val storageText  = holder.itemView.context.getString(R.string.StorageItemQuantityList_StorageLabel)

        if (storageItem.isChanged)
        {
            holder.labelChanged.text = "*"
        }
        else
        {
            holder.labelChanged.text = ""
        }


        holder.quantity.text = Tools.formatNumber(storageItem.quantity)
        holder.date.text     = dateText.format(Tools.dateToString(storageItem.bestBefore))
        holder.storage.text  = storageText.format(storageItem.storageName ?: "")

        if (storageItem.warningLevel == 0)
        {
            holder.date.setTextColor(holder.itemView.context.getColor(R.color.Default_Text_Color))
        }

        if (storageItem.warningLevel == 1)
        {
            holder.date.setTextColor(holder.itemView.context.getColor(R.color.Text_Warning))
        }

        if (storageItem.warningLevel == 2)
        {
            holder.date.setTextColor(holder.itemView.context.getColor(R.color.Text_Error))
        }

        if (this.actionButtonsVisible)
        {
            holder.buttonAdd.visibility    = View.VISIBLE
            holder.buttonRemove.visibility = View.VISIBLE
            holder.labelChanged.visibility = View.VISIBLE
        }
        else
        {
            holder.buttonRemove.visibility = View.GONE
            holder.buttonAdd.visibility    = View.GONE
            holder.labelChanged.visibility = View.GONE
        }

        holder.buttonAdd.setOnClickListener {
            onItemClicked(ActionType.ADD_QUANTITY, storageItem)
        }

        holder.buttonRemove.setOnClickListener {
            onItemClicked(ActionType.REMOVE_QUANTITY, storageItem)
        }

        holder.itemView.setOnClickListener {
            val selectQuantity = holder.itemView.resources.getString(R.string.StorageItemQuantityList_ActionSelectQuantity)
            val expiryDate     = holder.itemView.resources.getString(R.string.StorageItemQuantityList_ActionSelectExpiryDate)
            val selectStorage  = holder.itemView.resources.getString(R.string.StorageItemQuantityList_ActionSelectStorage)

            val actions = arrayOf(
                selectQuantity,
                "$selectQuantity +1",
                "$selectQuantity -1",
                expiryDate,
                selectStorage)

            val builder = AlertDialog.Builder(holder.itemView.context, R.style.MyAlertDialogTheme)
            builder.setTitle(R.string.StorageItemQuantityList_ChangeDetails)
            builder.setItems(actions) { _, which ->
                when (which) {
                    0 -> onItemClicked(ActionType.CHANGE_QUANTITY,   storageItem) // Anzahl eingeben
                    1 -> onItemClicked(ActionType.INCREASE_QUANTITY, storageItem) // Anzahl + 1
                    2 -> onItemClicked(ActionType.DECREASE_QUANTITY, storageItem) // Anzahl - 1
                    3 -> onItemClicked(ActionType.CHANGE_DATE,       storageItem) // Ablaufdatum
                    4 -> onItemClicked(ActionType.CHANGE_STORAGE,    storageItem) // Lagerort
                }
            }
            builder.show()
        }
    }

    fun activateButtons()
    {
        this.actionButtonsVisible = true
    }

    fun deactivateButtons()
    {
        this.actionButtonsVisible = false
    }

    override fun getItemCount(): Int {
        return storageItems.size
    }

    fun addStorageItem(storageItem: StorageItem) {
        storageItems.add(storageItem)
    }

    fun getChangedStorageItems(): List<StorageItem> {
        return storageItems.filter { it.isChanged }
    }
}