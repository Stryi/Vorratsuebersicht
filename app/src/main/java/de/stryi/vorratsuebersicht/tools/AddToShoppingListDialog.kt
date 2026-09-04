package de.stryi.vorratsuebersicht.tools

import android.app.Activity
import androidx.appcompat.app.AlertDialog
import android.text.InputType
import android.widget.EditText
import de.stryi.vorratsuebersicht.R
import de.stryi.vorratsuebersicht.database.Database
import java.util.Locale

class AddToShoppingListDialog {
    companion object {
        fun showDialog(
                activity: Activity,
                articleId: Int,
                minQuantityParam: Int? = null,
                prefQuantityParam: Int? = null,
                refreshListAction: (() -> Unit)? = null
        ) {
            var minQuantity = minQuantityParam
            var prefQuantity = prefQuantityParam

            var quantityInfo = ""
            var toBuyQuantity = 0.0   // Double anstelle von decimal

            val quantityInStorage = Database.getArticleQuantityInStorage(articleId)
            activity.resources.getString(R.string.ToShoppingList_Inventory)
            quantityInfo += String.format(
                    Locale.getDefault(),
                    "%s %s\n",
                    activity.getString(R.string.ToShoppingList_Inventory),
                    Tools.formatNumber(quantityInStorage)
            )

            if (minQuantity == null || prefQuantity == null) {
                val articleData = Database.getArticle(articleId)
                minQuantity = articleData?.minQuantity
                prefQuantity = articleData?.prefQuantity
            }

            if (minQuantity == null) minQuantity = 0
            if (prefQuantity == null) prefQuantity = 0

            if (minQuantity > 0) {
                quantityInfo += String.format(
                        "%s %s\n",
                        activity.getString(R.string.ToShoppingList_MinQuantity),
                        Tools.formatNumber(minQuantity)
                )
                toBuyQuantity = minQuantity.toDouble()
            }

            if (prefQuantity > 0) {
                quantityInfo += String.format(
                        "%s %s\n",
                        activity.getString(R.string.ToShoppingList_PrefQuantity),
                        Tools.formatNumber(prefQuantity)
                )
                toBuyQuantity = prefQuantity.toDouble()
            }

            if (quantityInStorage > 0) {
                toBuyQuantity -= quantityInStorage
            }

            val shoppingListQuantity = Database.getShoppingListQuantiy(articleId)
            quantityInfo += String.format(
                    Locale.getDefault(),
                    "%s %s\n",
                    activity.getString(R.string.ToShoppingList_ToShoppingList),
                    Tools.formatNumber(shoppingListQuantity)
            )

            // Auf Einkaufsliste ist ein höherer Betrag als ausgerechnet?
            if (shoppingListQuantity!! > toBuyQuantity) {
                toBuyQuantity = shoppingListQuantity
            }

            if (toBuyQuantity == 0.0) {
                toBuyQuantity = 1.0
            }

            val message = String.format(
                    "%s\n%s",
                    quantityInfo,
                    activity.getString(R.string.ToShoppingList_EnterNewQuantity)
            )

            val b = AlertDialog.Builder(activity, R.style.MyAlertDialogTheme)
            val quantityDialog = b.create()
            quantityDialog.window?.setSoftInputMode(android.view.WindowManager.LayoutParams.SOFT_INPUT_STATE_VISIBLE)
            quantityDialog.setTitle(activity.getString(R.string.ToShoppingList_Title))
            quantityDialog.setMessage(message)

            val input = EditText(activity)
            input.inputType = InputType.TYPE_CLASS_NUMBER or InputType.TYPE_NUMBER_FLAG_DECIMAL
            input.setText(Tools.formatNumber(toBuyQuantity))

            // Setze den Abstand (Padding) für den EditText
            val marginInDp = 20
            val scale = activity.resources.displayMetrics.density
            val marginInPixels = (marginInDp * scale + 0.5f).toInt()
            input.setPadding(marginInPixels, marginInPixels, marginInPixels, marginInPixels)

            input.requestFocus()
            input.setSelection(0, input.text.length)
            quantityDialog.setView(input)

            quantityDialog.setButton(AlertDialog.BUTTON_NEGATIVE, activity.getString(R.string.App_Cancel)) { _, _ -> }
            quantityDialog.setButton(AlertDialog.BUTTON_POSITIVE, activity.getString(R.string.App_Ok)) { _, _ ->
                var text = input.text.toString()
                if (text.isEmpty()) {
                    text = "0"
                }

                val parsed = text.toDoubleOrNull()
                if (parsed != null) {
                    val buyQty = parsed
                    if (buyQty == 0.0) {
                        Database.removeFromShoppingList(articleId)
                    } else {
                        Database.setShoppingItemQuantity(articleId, buyQty)
                    }
                    refreshListAction?.invoke()
                }
            }

            quantityDialog.show()
        }
    }
}
