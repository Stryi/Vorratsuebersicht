package de.stryi.vorratsuebersicht

import android.app.Dialog
import android.os.Bundle
import android.view.LayoutInflater
import android.widget.TextView
import androidx.appcompat.app.AlertDialog
import androidx.fragment.app.DialogFragment

class ProgressDialogFragment : DialogFragment() {

    private var messageTextView: TextView? = null
    private var currentProgress: Int = 0

    override fun onCreateDialog(savedInstanceState: Bundle?): Dialog {
        val view = LayoutInflater.from(requireContext()).inflate(R.layout.dialog_progress, null)
        messageTextView = view.findViewById(R.id.dialog_progress_text)

        updateText()

        val builder = AlertDialog.Builder(requireContext(), R.style.MyAlertDialogTheme)
        builder.setView(view)

        val dialog = builder.create()
        dialog.setCanceledOnTouchOutside(false)
        isCancelable = false
        return dialog
    }

    fun setProgress(progress: Int) {
        currentProgress = progress
        updateText()
    }

    private fun updateText() {
        val baseMsg = arguments?.getString(ARG_MESSAGE) ?: getString(R.string.Start_PleaseWait)
        val text = if (currentProgress in 1..99) {
            "$baseMsg ($currentProgress%)"
        } else {
            baseMsg
        }
        messageTextView?.text = text
    }

    companion object {
        private const val ARG_MESSAGE = "arg_message"

        fun newInstance(message: String? = null): ProgressDialogFragment {
            val fragment = ProgressDialogFragment()
            if (message != null) {
                val args = Bundle()
                args.putString(ARG_MESSAGE, message)
                fragment.arguments = args
            }
            return fragment
        }
    }
}
