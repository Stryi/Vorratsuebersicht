package de.stryi.vorratsuebersicht

import android.content.Context
import android.content.Intent
import android.os.Build
import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import android.view.View
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.databinding.LogViewerActivityBinding
import de.stryi.vorratsuebersicht.tools.Logging
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

class LogViewerActivity : AppCompatActivity() {

    private lateinit var binding: LogViewerActivityBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = LogViewerActivityBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setSupportActionBar(binding.LogViewerAppBar)
        binding.LogViewerAppBar.setNavigationOnClickListener { finish() }

        val logText = Logging.getLogFileText(this)
        binding.LogViewerText.text = logText

        // Nach ganz unten scrollen
        binding.LogViewerScrollView.post {
            binding.LogViewerScrollView.fullScroll(View.FOCUS_DOWN)
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        menuInflater.inflate(R.menu.log_viewer_menu, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            R.id.LogViewer_MenuSend -> {
                sendLogFile()
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }

    private fun sendLogFile() {
        val message = resources.getString(R.string.Settings_SendLogFileMessage)

        val dialog = AlertDialog.Builder(this, R.style.MyAlertDialogTheme)
        dialog.setMessage(message)

        dialog.setPositiveButton(resources.getString(R.string.App_Yes)) { _, _ ->

            val context: Context = applicationContext
            val packageInfo = context.packageManager.getPackageInfo(context.packageName, 0)
            val versionName = packageInfo.versionName
            val versionCode = packageInfo.longVersionCode

            val text = StringBuilder()
            text.append("Version $versionName (Code Version ${versionCode})\n")
            text.append("Current Database: ${Database.getDatabasePath()}\n")
            text.append("Android Version: ${Build.VERSION.RELEASE}\n")
            text.append("Android SDK: ${Build.VERSION.SDK_INT}\n")
            text.append("Manufacturer: ${Build.MANUFACTURER}\n")
            text.append("Modell: ${Build.MODEL}\n")
            text.append("CurrentCulture: ${Locale.getDefault().displayName}\n")
            text.append("CurrentUICulture: ${Locale.getDefault().displayName}\n")

            text.appendLine()
            text.appendLine(Logging.getLogFileText(this))

            val subject = "Vue_LOG_" +
                    SimpleDateFormat("yyyy-MM-dd_HH.mm.ss", Locale.getDefault()).format(Date())

            val emailIntent = Intent(Intent.ACTION_SEND).apply {
                putExtra(Intent.EXTRA_EMAIL, arrayOf("cstryi@freenet.de"))
                putExtra(Intent.EXTRA_SUBJECT, subject)
                putExtra(Intent.EXTRA_TEXT, text.toString())
                type = "text/plain"
            }

            startActivity(
                Intent.createChooser(
                    emailIntent,
                    resources.getString(R.string.Settings_SendLogFile)
                )
            )
        }

        dialog.setNegativeButton(resources.getString(R.string.App_No)) { _, _ -> }
        dialog.create().show()
    }
}
