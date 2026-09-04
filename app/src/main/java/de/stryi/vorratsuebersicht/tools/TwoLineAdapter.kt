package de.stryi.vorratsuebersicht.tools

import android.content.Context
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.TextView
import de.stryi.vorratsuebersicht.R

class TwoLineAdapter(
    context: Context,
    private val items: List<Pair<String, String>>
) : ArrayAdapter<Pair<String, String>>(context, 0, items) {

    override fun getView(position: Int, convertView: View?, parent: ViewGroup): View {
        val view = convertView ?: LayoutInflater.from(context)
            .inflate(R.layout.dialog_item_two_lines, parent, false)

        val (titleText, subtitleText) = items[position]

        view.findViewById<TextView>(R.id.title).text = titleText
        view.findViewById<TextView>(R.id.subtitle).text = subtitleText

        return view
    }
}
