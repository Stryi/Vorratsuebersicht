package de.stryi.vorratsuebersicht.tools

data class CategoryItem(
    val text: String,
    val category: String = "",
    val subCategory: String = ""
) {
    override fun toString(): String {
        return text
    }
}