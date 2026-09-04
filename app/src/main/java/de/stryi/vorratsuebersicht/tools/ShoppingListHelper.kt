package de.stryi.vorratsuebersicht.tools

object ShoppingListHelper {

    fun getToBuyQuantity(minQuantity: Int, prefQuantity: Int, isQuantity: Int): Int {
        var toBuy = 0

        if (minQuantity > 0) {
            if (isQuantity < minQuantity) {
                toBuy = if (prefQuantity > 0) {
                    prefQuantity - isQuantity
                } else {
                    minQuantity - isQuantity
                }
            }
            return toBuy
        }

        if (prefQuantity > 0) {
            if (isQuantity < prefQuantity) {
                toBuy = prefQuantity - isQuantity
            }
            return toBuy
        }

        return toBuy
    }
}
