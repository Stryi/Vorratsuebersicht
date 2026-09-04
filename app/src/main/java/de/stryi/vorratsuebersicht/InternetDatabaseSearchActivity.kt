package de.stryi.vorratsuebersicht

import android.annotation.SuppressLint
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import android.view.View
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import androidx.lifecycle.lifecycleScope
import com.google.gson.Gson
import de.stryi.vorratsuebersicht.databinding.InternetDatabaseSearchBinding
import de.stryi.vorratsuebersicht.tools.QuantityAndUnit
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import okhttp3.OkHttpClient
import okhttp3.Request
import java.io.IOException
import java.util.Locale


class InternetDatabaseSearchActivity :AppCompatActivity()
{
    private lateinit var binding: InternetDatabaseSearchBinding

    private var formatedResponseFromServer : String = ""

    var foodInfo: FoodInformation? = null
    var foodSize: QuantityAndUnit? = null
    var kcalPer100 : Double? = null

    companion object {
        var picture: Bitmap? = null
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = InternetDatabaseSearchBinding.inflate(layoutInflater)

        setContentView(binding.root)

        this.setSupportActionBar(binding.InternetDatabaseResultAppBar)

        binding.InternetDatabaseResultAppBar.setNavigationOnClickListener { finish() }

        binding.InternetDatabaseResultProgressText.setText(R.string.InternetDatabaseSearch_Searching)
        binding.InternetDatabaseResultDescription.setText(R.string.InternetDatabaseSearch_Info)

        val eanCode = intent.getStringExtra("EANCode")

        if (eanCode == null)
        {
            binding.InternetDatabaseResultProgress.visibility = View.INVISIBLE
            binding.InternetDatabaseResultText.text = "Kein EAN Code angegeben."
            return
        }

        binding.InternetDatabaseResultEanCode.text = this.resources.getString(R.string.InternetDatabaseSearch_EANCode).format(eanCode)

        // TODO: eanCode könnte auch mehrere enthalten (Komma seppariert)
        this.searchAndShowArticle(eanCode)
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.internet_database_search_menu, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        when (item.itemId) {
            R.id.InternetDatabaseResult_Save -> {

                if (foodInfo?.status == 1)
                {
                    val intent = Intent()
                    intent.putExtra("Name", foodInfo?.product?.getProduktName())
                    intent.putExtra("Hersteller", foodInfo?.product?.brands)
                    if (foodSize != null)
                    {
                        intent.putExtra("Quantity", foodSize?.quantity)
                        intent.putExtra("Unit",     foodSize?.unit)
                    }
                    if (kcalPer100 != null) {
                        intent.putExtra("KCalPer100", kcalPer100!!)
                    }

                    this.setResult(RESULT_OK, intent)
                }
                else
                {
                    this.setResult(RESULT_CANCELED)
                }

                this.finish()
                return true
            }
        }

        return false
    }

    private fun searchAndShowArticle(eanCode : String) {

        lifecycleScope.launch {
            try {
                // Führe die Netzwerkanfrage im IO-Dispatcher (Hintergrund) aus
                foodInfo = withContext(Dispatchers.IO) {
                    getFoodInformation(eanCode)
                }

                foodSize   = QuantityAndUnit.parse(foodInfo?.product?.quantity)
                kcalPer100 = getKcalPer100(foodInfo)

                picture = withContext(Dispatchers.IO) {
                    getUrlPicture(foodInfo?.product?.image_url)
                }

                if (picture != null)
                {
                    binding.InternetDatabaseResultImage.setImageBitmap(picture)
                    binding.InternetDatabaseResultImage.visibility = View.VISIBLE
                }

                binding.InternetDatabaseResultProgress.visibility = View.INVISIBLE

                // Wechsle zurück zum Main-Thread, um die UI zu aktualisieren
                if (foodInfo == null || foodInfo?.status == 0)   // OpenFoodFacts gibt status=0 bei Nicht-Fund
                {
                    val status   = "Status: %s - %s\n\nResponse: %s".format(
                        foodInfo?.status,
                        foodInfo?.status_verbose,
                        formatedResponseFromServer)

                    binding.InternetDatabaseResultText.text = status


                }
                else
                {
                    var info = resources.getString(R.string.ArticleDetails_ArticleName)
                    info += "\n%s\n\n".format(foodInfo?.product?.getProduktName())

                    info += resources.getString(R.string.ArticleDetails_Manufacturer)
                    info += "\n%s\n\n".format(foodInfo?.product?.brands)

                    if (foodSize != null)
                    {
                        info += resources.getString(R.string.InternetDatabaseSearch_Size)
                            .format(foodSize?.quantity, foodSize?.unit)
                    }
                    else
                    {
                        info += resources.getString(R.string.InternetDatabaseSearch_UnknownSize)
                            .format(foodInfo?.product?.quantity)
                    }

                    if (kcalPer100 != null)
                    {
                        info += "\n\n"
                        info += resources.getString(R.string.InternetDatabaseSearch_CaloriesPerUnit)
                            .format(kcalPer100)
                    }

                    binding.InternetDatabaseResultText.text = info
                    val languageCode = Locale.getDefault().language
                    binding.InternetDatabaseResultEanCode.text =
                        "Link: https://$languageCode.openfoodfacts.org/product/" + foodInfo?.code
                }

            } catch (e: Exception) {
                // Fehlerbehandlung bleibt auf dem Main-Thread
                binding.InternetDatabaseResultProgress.visibility = View.INVISIBLE
                binding.InternetDatabaseResultText.text = "Fehler bei der Suche: ${e.message}"
            }
        }
    }

    private fun getFoodInformation(eanCode: String): FoodInformation? {

        val fields = "product_name,product_name_de,brands,quantity,nutriments,image_url"

        val url = "https://world.openfoodfacts.org/api/v0/product/$eanCode.json?fields=$fields"

        val info = this.packageManager.getPackageInfo(this.packageName, 0)
        val userAgent = ("Vorratsuebersicht - Android - Version " + info.versionName + " - https://sites.google.com/site/vorratsuebersicht")

        val client = OkHttpClient().newBuilder()
            .connectTimeout(10, java.util.concurrent.TimeUnit.SECONDS)
            .build()

        val request = Request.Builder()
            .header("User-Agent", userAgent)
            .url(url)
            .build()

        val gson = Gson()

        val call = client.newCall(request)
        val response = call.execute()
        try {
            if (!response.isSuccessful) {
                throw IOException("Unexpected code $response")
            }

            val bodyString = response.body?.string()
            if (bodyString == null) {
                return null
            }
            this.formatedResponseFromServer = bodyString

            return gson.fromJson(bodyString, FoodInformation::class.java)
        } finally {
            response.close()  // wichtig: Ressourcen freigeben
        }
    }

    data class FoodInformation(
        val code: String?,
        val status: Int?,
        val status_verbose: String?,
        val product: Product?
    ) {
        data class Product(
            val product_name: String?,
            val product_name_de: String?,
            val brands: String?,
            val code: String?,
            val image_url: String?,
            val quantity: String?,
            val product_quantity: Int?,
            val nutriments: Nutriments?
        ) {
            fun getProduktName(): String? {
                // Vorzugsweise die Deutsche Sprache nehmen
                return if (!product_name_de.isNullOrEmpty()) {
                    product_name_de
                } else {
                    product_name
                }
            }
        }

        data class Nutriments(
            val energy_unit: String?,
            val energy_value: Double?,   // kcal pro 100 g oder 100 ml
            val energy_100g: Double?     // {energy_unit} pro 100 g oder 100 ml
        )
    }

    fun getKcalPer100(foodInfo: FoodInformation?): Double?
    {
        var value : Double? = null

        if (foodInfo?.product?.nutriments?.energy_unit.equals("kcal", ignoreCase = true)) {
            value = foodInfo?.product?.nutriments?.energy_value
        }

        if (foodInfo?.product?.nutriments?.energy_unit.equals("kJ", ignoreCase = true)) {
            // kcal = kJ / 4,184 dividieren
            value = foodInfo?.product?.nutriments?.energy_100g?.div(4.184)
            value = value?.let { kotlin.math.round(it) }
        }
        return value
    }

    fun getUrlPicture(imageUrl : String?) : Bitmap?
    {
        if (imageUrl == null)
            return null

        val client = OkHttpClient
            .Builder()
            .build()
        val request = Request.Builder()
            .url(imageUrl)
            .build()
        val call = client.newCall(request)
        val response = call.execute()
        try {
            if (!response.isSuccessful) {
                throw IOException("Unexpected code $response")
            }

            val body = response.body?.bytes()
            if (body == null) {
                return null
            }
            return BitmapFactory.decodeByteArray(body, 0, body.size)
        } finally {
            response.close()  // wichtig: Ressourcen freigeben
        }
    }
}