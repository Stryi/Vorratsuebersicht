package de.stryi.vorratsuebersicht

import android.annotation.SuppressLint
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.graphics.Matrix
import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import android.view.View
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.view.menu.MenuBuilder
import androidx.lifecycle.lifecycleScope
import de.stryi.vorratsuebersicht.databinding.ArticleImageBinding
import de.stryi.vorratsuebersicht.database.Database
import de.stryi.vorratsuebersicht.tools.Tools
import de.stryi.vorratsuebersicht.tools.toPngByteArray
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.util.Locale


class ArticleImageActivity : AppCompatActivity() {

    private lateinit var binding: ArticleImageBinding

    var articleId: Int = 0
    var editMode: Boolean = false
    var isChanged: Boolean = false

    var rotatedBitmap: Bitmap? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        this.binding = ArticleImageBinding.inflate(layoutInflater)

        setContentView(binding.root)

        this.setSupportActionBar(binding.ArticleImageAppBar)

        this.articleId = intent.getIntExtra("ArticleId", 0)
        this.editMode  = intent.getBooleanExtra("EditMode", false)

        if (intent.hasExtra("Title"))
        {
            this.title = intent.getStringExtra("Title")
        }
        else
        {
            this.title = Database.getArticleName(this.articleId)
        }

        binding.ArticleImageAppBar.overflowIcon?.setTint(getColor(R.color.Application_ActionBar_TextColor))
        binding.ArticleImageAppBar.setNavigationOnClickListener { finish() }
        binding.ArticleImageAppBar.setOnMenuItemClickListener { this.onOptionsItemSelected(it) }
        binding.ArticleImageImage.setOnClickListener    { this.showImageInformation() }
        binding.ArticleImageImageThn.setOnClickListener { this.showImageInformation() }

        if (ArticleDetailsActivity.imageLarge != null)
        {
            this.showPictureFromBitmap()
        }
        else
        {
            this.showPictureFromDatabase()
        }
    }

    @SuppressLint("RestrictedApi")
    override fun onCreateOptionsMenu(menu: Menu): Boolean
    {
        if (menu is MenuBuilder) {
            menu.setOptionalIconsVisible(true)
        }
        menuInflater.inflate(R.menu.article_image_menu, menu)
        return true
    }

    override fun onPrepareOptionsMenu(menu: Menu): Boolean
    {
        if (!this.editMode)
        {
            val itemRotate = menu.findItem(R.id.ArticleImage_Menu_RotateRight)
            itemRotate.isVisible = false
        }

        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        when (item.itemId) {
            R.id.ArticleImage_Menu_RotateRight -> {
                this.rotateImage()
                return true
            }
        }

        return false
    }

    override fun finish()
    {
        if (!this.isChanged)
        {
            super.finish()
            return
        }

        this.showProgressBar()
        lifecycleScope.launch {
            withContext(Dispatchers.IO) {
                saveBitmap()
                System.gc()
            }

            finish()
            hideProgressBar()
        }
    }

    private fun saveBitmap()
    {
        if (rotatedBitmap == null)
            return

        // Großes Bild als PNG-Bytearray speichern
        ArticleDetailsActivity.imageLarge = rotatedBitmap!!.toPngByteArray()

        // Verkleinertes Bild (Thumbnail) erstellen
        val smallBitmap = ImageResizer.resizeImageAndroid(
            rotatedBitmap!!,
            (48 * 2).toFloat(),
            (85 * 2).toFloat())

        // Thumbnail als PNG-Bytearray speichern
        ArticleDetailsActivity.imageSmall = smallBitmap

        val intent = Intent()
        this.setResult(RESULT_OK, intent)

        this.rotatedBitmap = null
        this.isChanged = false
    }

    fun showPictureFromDatabase()
    {
        val article= Database.getArticleImage(this.articleId)

        this.showPictureFromByteArray(article?.imageLarge, article?.imageSmall)
    }

    fun showPictureFromBitmap()
    {
        this.showPictureFromByteArray(ArticleDetailsActivity.imageLarge, ArticleDetailsActivity.imageSmall)
    }

    fun showPictureFromByteArray(imageLarge: ByteArray?, imageSmall: ByteArray?)
    {
        if (imageLarge == null)
        {
            binding.ArticleImageImage.setImageResource(R.drawable.hide_image_24px)
            binding.ArticleImageImage.visibility = View.VISIBLE
            binding.ArticleImageInfo.text = resources.getString(R.string.ArticleImage_SizeLarge)
        }

        if (imageSmall == null)
        {
            binding.ArticleImageImageThn.setImageResource(R.drawable.hide_image_24px)
            binding.ArticleImageImageThn.visibility = View.VISIBLE
            binding.ArticleImageInfoThn.text = resources.getString(R.string.ArticleImage_SizeSmall)
        }

        try
        {
            if (imageLarge != null)
            {
                val largeBitmap = BitmapFactory.decodeByteArray(imageLarge, 0, imageLarge.size)
                binding.ArticleImageImage.setImageBitmap(largeBitmap)

                this.rotatedBitmap = largeBitmap

                val message = String.format(
                    Locale.getDefault(),
                    "Bild (BxH): %,d x %,d\nGröße: %s, Komprimiert: %s",
                    largeBitmap.width,
                    largeBitmap.height,
                    Tools.toFuzzyByteString(largeBitmap.byteCount.toLong()),
                    Tools.toFuzzyByteString(imageLarge.size.toLong()))

                binding.ArticleImageInfo.text = message
            }

            if (imageSmall != null)
            {
                val smallBitmap = BitmapFactory.decodeByteArray(imageSmall, 0, imageSmall.size)
                binding.ArticleImageImageThn.setImageBitmap(smallBitmap)

                val message = String.format(
                    Locale.getDefault(),
                    "Vorschaubild (BxH): %,d x %,d\nGröße: %s, Komprimiert: %s",
                    smallBitmap.width,
                    smallBitmap.height,
                    Tools.toFuzzyByteString(smallBitmap.byteCount.toLong()),
                    Tools.toFuzzyByteString(imageSmall.size.toLong()))

                binding.ArticleImageInfoThn.text = message
            }
        }
        catch (e: Exception)
        {
            binding.ArticleImageInfoThn.text = e.message
            binding.ArticleImageInfoThn.visibility = View.VISIBLE
        }
    }

    private fun rotateImage()
    {
        if (rotatedBitmap == null)
        {
            return
        }

        val matrix = Matrix()
        matrix.postRotate(90f)

        val rotated = Bitmap.createBitmap(
            rotatedBitmap!!,
            0, 0,
            rotatedBitmap!!.width,
            rotatedBitmap!!.height,
            matrix,
            true
        )

        binding.ArticleImageImage.setImageBitmap(rotated)
        rotatedBitmap = rotated
        this.isChanged = true
    }

    private fun showImageInformation()
    {
        if (binding.ArticleImageInfo.visibility != View.VISIBLE)
        {
            binding.ArticleImageInfo.visibility     = View.VISIBLE
            binding.ArticleImageInfoThn.visibility  = View.VISIBLE
            binding.ArticleImageImageThn.visibility = View.VISIBLE
        }
        else
        {
            binding.ArticleImageInfo.visibility     = View.GONE
            binding.ArticleImageInfoThn.visibility  = View.GONE
            binding.ArticleImageImageThn.visibility = View.GONE
        }
    }

    private fun showProgressBar()
    {
        binding.ArticleImageProgressBar.visibility = View.VISIBLE
    }

    private fun hideProgressBar()
    {
        binding.ArticleImageProgressBar.visibility = View.GONE
    }
}