package de.stryi.vorratsuebersicht

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import de.stryi.vorratsuebersicht.databinding.LicenseActivityBinding

class LicenseActivity : AppCompatActivity() {

    private lateinit var binding: LicenseActivityBinding

    override fun onCreate(savedInstanceState: Bundle?)
    {
        super.onCreate(savedInstanceState)

        binding = LicenseActivityBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setSupportActionBar(binding.LicenseActivityAppBar)

        binding.LicenseActivityAppBar.setNavigationOnClickListener { finish() }

        binding.LicensesHtml.loadUrl("file:///android_asset/Licenses.html")
    }
}