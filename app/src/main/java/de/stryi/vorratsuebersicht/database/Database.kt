package de.stryi.vorratsuebersicht.database

import android.content.ContentValues
import android.content.Context
import android.database.DatabaseErrorHandler
import android.database.sqlite.SQLiteDatabase
import android.util.Log
import androidx.core.database.sqlite.transaction
import de.stryi.vorratsuebersicht.database.AndroidDatabase.SQLITE_FILENAME_DEMO
import de.stryi.vorratsuebersicht.database.Records.Article
import de.stryi.vorratsuebersicht.database.Records.ArticleImage
import de.stryi.vorratsuebersicht.database.Records.ShoppingItem
import de.stryi.vorratsuebersicht.database.Records.ArticleInfo
import de.stryi.vorratsuebersicht.database.Records.StorageItem
import de.stryi.vorratsuebersicht.database.Records.StorageItemCsvExport
import de.stryi.vorratsuebersicht.tools.CategoryItem
import de.stryi.vorratsuebersicht.tools.Tools
import de.stryi.vorratsuebersicht.tools.ShoppingListHelper
import de.stryi.vorratsuebersicht.tools.Tools.TRACE
import java.time.LocalDate
import java.time.temporal.ChronoUnit
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter
import java.util.Locale
import java.io.File

object Database
{
    private var db: SQLiteDatabase? = null

    fun init(databaseFilePath: String) : String?
    {
        try
        {
            val dbFile = File(databaseFilePath)
            db = SQLiteDatabase.openDatabase(
                dbFile.path,
                null,
                SQLiteDatabase.OPEN_READWRITE,
                DatabaseErrorHandler { database ->
                    TRACE("DB: Datenbank korrupt: ${database.path}")
                }
            )

            this.checkAndUpgradeSchema()
        }
        catch (e: Exception)
        {
            var message = "DB: ${databaseFilePath}\n\n"
            message += e.message.toString()
            TRACE(message)
            return message
        }
        return null
    }

    //fun isOpen(): Boolean = db?.isOpen == true

    fun closeDatabase() {
        db?.close()
    }

    fun getDatabasePath() : String? {
        return db?.path
    }

    fun getDatabaseSize() : Long {

        if (db == null)
            return 0L

        val file = File(db!!.path)
        return if (file.exists()) file.length() else 0L
    }

    fun getDatabaseName(): String {
        if (db == null)
            return ""

        val file = File(db!!.path)
        return file.nameWithoutExtension
    }

    fun insertArtice(article: Article): Long {
        if (db == null)
            return 0L

        val values = ContentValues()
        values.put("Name",            article.name)
        values.put("Manufacturer",    article.manufacturer)
        values.put("Category",        article.category)
        values.put("SubCategory",     article.subCategory)
        values.put("DurableInfinity", article.durableInfinity)
        values.put("WarnInDays",      article.warnInDays)
        values.put("Size",            article.size)
        values.put("Unit",            article.unit)
        values.put("Notes",           article.notes)
        values.put("EANCode",         article.eanCode)
        values.put("Calorie",         article.calorie)
        values.put("Price",           article.price)
        values.put("StorageName",     article.storageName)
        values.put("Supermarket",     article.supermarket)
        values.put("MinQuantity",     article.minQuantity)
        values.put("PrefQuantity",    article.prefQuantity)


        val newId = db!!.insert("Article", null, values)

        this.increaseChangeCounter()

        return newId
    }

    fun updateArticle(article: Article)
    {
        if (db == null)
            return

        val query = """
            UPDATE Article
            SET Name = ?, Manufacturer = ?, Category = ?, SubCategory = ?, DurableInfinity = ?, WarnInDays = ?,
                Size = ?, Unit = ?, Notes = ?, EANCode = ?, Calorie = ?, Price = ?, StorageName = ?, Supermarket = ?,
                MinQuantity = ?, PrefQuantity = ?
            WHERE ArticleId = ?
        """.trimIndent()
        db!!.execSQL(query, arrayOf<Any?>(article.name, article.manufacturer, article.category, article.subCategory,
            article.durableInfinity, article.warnInDays, article.size, article.unit, article.notes, article.eanCode,
            article.calorie, article.price, article.storageName, article.supermarket,
            article.minQuantity, article.prefQuantity, article.articleId))

        this.increaseChangeCounter()
    }

    fun deleteArticle(articleId: Int) {
        if (db == null)
            return

        db!!.transaction {

            var query = "DELETE FROM ShoppingList WHERE ArticleId = ?"
            execSQL(query, arrayOf(articleId))

            query = "DELETE FROM ArticleImage WHERE ArticleId = ?"
            execSQL(query, arrayOf(articleId))

            query = "DELETE FROM Article WHERE ArticleId = ?"
            execSQL(query, arrayOf(articleId))
        }
        this.increaseChangeCounter()
    }

    fun getArticle(articleId: Int): Article? {
        if (db == null)
            return null

        val query = """
            SELECT *
            FROM Article
            WHERE ArticleId = ?
            """.trimIndent()
        val cursor = db!!.rawQuery(query, arrayOf(articleId.toString()))
        cursor.use {
            if (it.moveToFirst()) {
                return Article.fromCursor(it)
            }
        }
        return null
    }

    fun getArticleName(articleId: Int): String {
        if (db == null)
            return ""

        val query = """
            SELECT Name
            FROM Article
            WHERE ArticleId = ?
            """.trimIndent()
        val cursor = db!!.rawQuery(query, arrayOf(articleId.toString()))
        cursor.use {
            if (it.moveToFirst()) {
                return it.getString(0)
            }
        }
        return ""
    }

    fun getArticleList(
        category: String,
        subCategory: String,
        eanCode: String?,
        notInStorage: Boolean,
        notInShoppingList: Boolean,
        withoutCategory: Boolean,
        specialFilter: Int,
        textFilter: String?
    ): List<Article> {

        val result = mutableListOf<Article>()
        val parameter = mutableListOf<String>()

        if (db == null)
            return result

        var filter = ""

        if (category.isNotEmpty())
        {
            filter += " WHERE Article.Category = ?"
            parameter.add(category)
        }

        if (subCategory.isNotEmpty())
        {
            if (filter.isNotEmpty()) filter += " AND " else filter += " WHERE "

            filter += " Article.SubCategory = ?"
            parameter.add(subCategory)
        }

        if (withoutCategory)
        {
            if (filter.isNotEmpty()) filter += " AND " else filter += " WHERE "

            filter += " (Article.Category IS NULL OR Article.Category = ''" +
                    " OR Article.SubCategory IS NULL OR Article.SubCategory = '') "
        }

        if (!eanCode.isNullOrEmpty())
        {
            if (filter.isNotEmpty()) filter += " AND " else filter += " WHERE "

            filter += " Article.EANCode LIKE ?"
            parameter.add("%$eanCode%")
        }

        if (!textFilter.isNullOrEmpty())
        {
            if (filter.isNotEmpty()) filter += " AND " else filter += " WHERE "

            when (textFilter.uppercase()) {
                "P-" -> filter += " Article.Price IS NULL"
                "K-" -> filter += " Article.Calorie IS NULL"
                "B+" -> {
                    // Artikel zum Bestellen.
                    filter += " ArticleId NOT IN (SELECT ArticleId FROM ShoppingList)"
                    filter += " AND "
                    filter += " ArticleId NOT IN (SELECT ArticleId FROM StorageItem)"
                    filter += " OR (SELECT SUM(Quantity) FROM StorageItem WHERE StorageItem.ArticleId = Article.ArticleId GROUP BY ArticleId) < Article.MinQuantity" +
                            " AND (ArticleId NOT IN (SELECT ArticleId FROM ShoppingList))"
                }

                else -> {
                    filter += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?"
                    filter += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ? OR Article.EANCode LIKE ?)"
                    parameter.add("%$textFilter%")
                    parameter.add("%$textFilter%")
                    parameter.add("%$textFilter%")
                    parameter.add("%$textFilter%")
                    parameter.add("%$textFilter%")
                    parameter.add("%$textFilter%")
                    parameter.add("%$textFilter%")
                    parameter.add("%$textFilter%")
                }
            }
        }

        if (notInStorage)
        {
            if (filter.isNotEmpty()) filter += " AND " else filter += " WHERE "

            filter += "ArticleId NOT IN (SELECT ArticleId FROM StorageItem)"
        }

        if (notInShoppingList)
        {
            if (filter.isNotEmpty()) filter += " AND " else filter += " WHERE "

            filter += "ArticleId NOT IN (SELECT ArticleId FROM ShoppingList)"
        }

        if (specialFilter > 0)
        {
            if (filter.isNotEmpty()) filter += " AND " else filter += " WHERE "

            when (specialFilter) {
                1 -> filter += " Article.Price IS NULL"
                2 -> filter += " Article.Calorie IS NULL"
                3 -> {
                    // Artikel zum Bestellen.
                    filter += " ArticleId NOT IN (SELECT ArticleId FROM ShoppingList)"
                    filter += " AND "
                    filter += " ArticleId NOT IN (SELECT ArticleId FROM StorageItem)"
                    filter += " OR (SELECT SUM(Quantity) FROM StorageItem WHERE StorageItem.ArticleId = Article.ArticleId GROUP BY ArticleId) < Article.MinQuantity" +
                              " AND (ArticleId NOT IN (SELECT ArticleId FROM ShoppingList))"
                }

                4 -> filter += " ((Article.StorageName IS NULL) OR (Article.StorageName == ''))"
            }
        }

        val query = """
            SELECT ArticleId, Name, Manufacturer, Category, SubCategory, DurableInfinity, WarnInDays,
                   Size, Unit, Notes, EANCode, Calorie, Price, StorageName, Supermarket, MinQuantity, PrefQuantity
            FROM Article
            $filter
            ORDER BY Name COLLATE NOCASE
        """.trimIndent()

        val cursor = db!!.rawQuery(query, parameter.toTypedArray())
        cursor.use {
            while (it.moveToNext()) {
                val article = Article.fromCursor(it)
                result.add(article)
            }
        }
        return result
    }

    fun getArticleListToCsvExport() : MutableList<Article>
    {
        val result = mutableListOf<Article>()
        if (db == null)
            return result

        val query = """
            SELECT *
                FROM Article
                ORDER BY ArticleId
        """.trimIndent()

        val cursor = db!!.rawQuery(query, null)
        cursor.use {
            while (it.moveToNext()) {
                val article = Article.fromCursor(it)
                result.add(article)
            }
        }

        return result
    }

    fun getArticlesByEanCode(eanCode: String): List<Article>
    {
        val query = """
            SELECT ArticleId, Name
            FROM Article
            WHERE EANCode LIKE ?
        """.trimIndent()

        val result = mutableListOf<Article>()
        if (db == null)
            return result

        val cursor = db!!.rawQuery(query, arrayOf("%${eanCode}%"))
        cursor.use {
            while (it.moveToNext()) {
                val article = Article.fromCursor(it)
                result.add(article)
            }
        }
        return result
    }

    fun getShoppingListQuantiy(articleId: Int, notFoundDefault: Double = 0.0): Double?
    {
        if (db == null)
            return null

        val query = """
            SELECT Quantity
            FROM ShoppingList
            WHERE ArticleId = ?
        """.trimIndent()
        val cursor = db!!.rawQuery(query, arrayOf(articleId.toString()))
        cursor.use {
            if (it.moveToFirst()) {
                return it.getDoubleOrNull("Quantity")
            }
        }
        return notFoundDefault
    }

    fun getArticleQuantityInStorage(articleId: Int): Double
    {
        if (db == null)
            return 0.00

        val query = """
            SELECT SUM(Quantity) AS Quantity
            FROM StorageItem
            WHERE ArticleId = ?
        """.trimIndent()
        val cursor = db!!.rawQuery(query, arrayOf(articleId.toString()))
        cursor.use {
            if (it.moveToFirst()) {
                return it.getDoubleOrNull("Quantity") ?: 0.0
            }
        }
        return 0.0
    }

    fun getCategoryAndSubcategoryNames(): MutableList<CategoryItem>
    {
        val stringList: MutableList<CategoryItem> = mutableListOf()

        if (db == null)
            return stringList

        val query = """
            SELECT DISTINCT Category, SubCategory
            FROM Article
            WHERE Category IS NOT NULL
            ORDER BY Category COLLATE NOCASE, SubCategory COLLATE NOCASE
        """.trimIndent()

        val cursor = db!!.rawQuery(query, null)

        var lastCategory = ""

        cursor.use {
            while (it.moveToNext()) {
                val article = Article.fromCursor(it)
                val categoryName = article.category
                val subCategoryName = article.subCategory

                if ((categoryName == null) && (subCategoryName == null))
                    continue

                if (categoryName != lastCategory)
                {
                    val item = CategoryItem(
                        categoryName.toString(),
                        categoryName.toString(),
                        "")
                    stringList.add(item)
                    lastCategory = categoryName.toString()
                }

                if (!subCategoryName.isNullOrBlank())
                {
                    // Die Zeichenfülge "  - " vor dem {0} ist wichtig
                    // für das Erkennen der Unterkategorie bei Auswahl.
                    val item = CategoryItem(
                        String.format("  - %s", subCategoryName),
                        categoryName.toString(),
                        subCategoryName)

                    stringList.add(item)
                }
            }
        }
        return stringList
    }

    fun getCategoriesInUse(): MutableList<String>
    {
        if (db == null)
            return mutableListOf()


        val query = """
            SELECT DISTINCT Category
            FROM Article
            WHERE Category IS NOT NULL
            AND Category <> ''
            AND ArticleId IN (SELECT ArticleId FROM StorageItem)
            ORDER BY Category COLLATE NOCASE
        """
        val result = db!!.queryStringList(query.trimIndent(), null)
        return result
    }

    fun getManufacturerNames(): MutableList<String>
    {
        if (db == null)
            return mutableListOf()

        val query = """
            SELECT DISTINCT Manufacturer
            FROM Article
            WHERE Manufacturer IS NOT NULL
            AND Manufacturer <> ''
            ORDER BY Manufacturer COLLATE NOCASE
        """
        val result = db!!.queryStringList(query.trimIndent(), null)

        return result
    }

    fun SQLiteDatabase.queryStringList(
        query: String,
        args: Array<String>? = null
    ): MutableList<String>
    {
        val result = mutableListOf<String>()
        if (db == null)
            return result

        val cursor = this.rawQuery(query, args)
        cursor.use {
            while (it.moveToNext()) {
                result.add(it.getString(0))
            }
        }
        return result
    }

    fun getSubcategoriesOf(category: String? = null, inStorageArticlesOnly: Boolean? = null): MutableList<String>
    {
        if (db == null)
            return mutableListOf()

        val parameters = mutableListOf<String>()

        var query = """
            SELECT DISTINCT SubCategory
            FROM Article
            WHERE SubCategory IS NOT NULL
            AND SubCategory <> ''
        """
        if (category != null)
        {
            query += " AND Category = ?"
            parameters.add(category)
        }

        if (inStorageArticlesOnly == true)
        {
            query += " AND ArticleId IN (SELECT ArticleId FROM StorageItem)"
        }

        query += " ORDER BY SubCategory COLLATE NOCASE"

        val result = db!!.queryStringList(
            query.trimIndent(),
            parameters.toTypedArray())

        return result
    }

    fun getStorageNames(inStorageArticlesOnly: Boolean = false): MutableList<String>
    {
        if (db == null)
            return mutableListOf()

        var query = """
            SELECT DISTINCT Article.StorageName
            FROM Article
            WHERE Article.StorageName IS NOT NULL AND Article.StorageName <> ''
        """

        if (inStorageArticlesOnly)
        {
            query += """
                AND Article.ArticleId IN (
                    SELECT StorageItem.ArticleId
                    FROM StorageItem
                    WHERE StorageItem.StorageName IS NULL OR StorageItem.StorageName = ''
                )"""
        }

        query += """
            UNION
            SELECT StorageName AS Value
            FROM StorageItem
            WHERE StorageName IS NOT NULL AND StorageName <> ''
            ORDER BY 1 COLLATE NOCASE
        """
        val result = db!!.queryStringList(query.trimIndent(), null)

        return result
    }

    fun getSupermarketNames(shoppingListOnly: Boolean = false): MutableList<String>
    {
        if (db == null)
            return mutableListOf()

        var query = """
            SELECT DISTINCT Supermarket
            FROM Article
        """
        if (shoppingListOnly)
        {
            query += " JOIN ShoppingList ON ShoppingList.ArticleId = Article.ArticleId"
        }
        query += """
            WHERE Supermarket IS NOT NULL
            AND Supermarket <> ''
            ORDER BY Supermarket COLLATE NOCASE
        """

        val result = db!!.queryStringList(query.trimIndent(), null)

        val stringList = mutableListOf<String>()

        for (item in result) {
            val supermarketName = item

            if (!shoppingListOnly) {
                stringList.add(supermarketName)
                continue
            }

            for (marketList in supermarketName.split(",")) {
                val name = marketList.trim()
                if (!stringList.contains(name)) {
                    stringList.add(name)
                }
            }
        }

        return stringList
    }

    fun insertArticleImage(articleImage: ArticleImage)
    {
        if (db == null)
            return

        val query = """
            INSERT INTO ArticleImage (ArticleId, Type, CreatedAt, ImageLarge, ImageSmall)
            VALUES (?, ?, ?, ?, ?)
        """.trimIndent()

        db!!.execSQL(query, arrayOf(articleImage.articleId, articleImage.type, articleImage.createdAt,
            articleImage.imageLarge, articleImage.imageSmall))

        this.increaseChangeCounter()
    }

    fun updateArticleImage(articleImage: ArticleImage)
    {
        if (db == null)
            return

        val query = """
            UPDATE ArticleImage
            SET ImageLarge = ?, ImageSmall = ?
            WHERE ImageId = ?
        """.trimIndent()
        db!!.execSQL(query, arrayOf(articleImage.imageLarge, articleImage.imageSmall, articleImage.imageId))

        this.increaseChangeCounter()
    }

    fun deleteArticleImage(articleImage: ArticleImage) {
        if (db == null)
            return

        val query = """
            DELETE FROM ArticleImage
            WHERE ImageId = ?
        """.trimIndent()

        db!!.execSQL(query, arrayOf(articleImage.imageId))

        this.increaseChangeCounter()
    }

    fun getArticleImage(articleId: Int?, showLarge: Boolean?  = null): ArticleImage?
    {
        if (db == null)
            return null

        var cmd = "SELECT ImageId, ArticleId, Type, CreatedAt"
        if (showLarge == null) {
            cmd += ", ImageLarge, ImageSmall"
        }
        else
        {
            if (showLarge)
                cmd += ", ImageLarge"
            else
                cmd += ", ImageSmall"
        }

        cmd += " FROM ArticleImage"
        cmd += " WHERE ArticleId = ?"
        cmd += " AND Type = 0"

        var result: ArticleImage? = null
        val cursor = db!!.rawQuery(cmd, arrayOf(articleId.toString()))
        cursor.use {
            while (it.moveToNext()) {
                result = ArticleImage.fromCursor(it)
            }
        }

        return result
    }

    fun getStorageItemList(
        category: String?,
        subCategory: String?,
        eanCode: String?,
        showNotInStorageArticles: Boolean,
        textFilter: String?  = null,
        storageName: String?  = null,
        withoutStorage: Boolean  = false,
        orderByToConsumeDate: Boolean  = false
    ) : MutableList<ArticleInfo>
    {
        if (db == null)
            return mutableListOf()

        val parameters = mutableListOf<String>()

        var bestBeforeFilter = ""

        if (!storageName.isNullOrEmpty())
        {
            // Positionen direkt mit dem Lager oder Positionen ohne Lager, aber wenn der Artikel das Lager hat.
            bestBeforeFilter += " AND (StorageItem.StorageName = ?"
            bestBeforeFilter += "  OR (IFNULL(StorageItem.StorageName, '') = '') AND StorageItem.ArticleId IN (SELECT Art1.ArticleId FROM Article AS Art1 WHERE Art1.StorageName = ?))"

            parameters.add(storageName)
            parameters.add(storageName)
        }

        if (withoutStorage)
        {
            bestBeforeFilter += " AND (IFNULL(Article.StorageName, '') = '')"
        }

        val bestBeforeSelect = "SELECT BestBefore" +
            " FROM StorageItem" +
            " WHERE StorageItem.ArticleId = Article.ArticleId" +
            bestBeforeFilter +
            " AND BestBefore IS NOT NULL" +
            " ORDER BY BestBefore ASC LIMIT 1"

        var sumQuantityFilter = ""

        if (!storageName.isNullOrEmpty())
        {
            sumQuantityFilter += " AND (StorageItem.StorageName = ?"
            sumQuantityFilter += "  OR (IFNULL(StorageItem.StorageName, '') = '') AND StorageItem.ArticleId IN (SELECT Art2.ArticleId FROM Article AS Art2 WHERE Art2.StorageName = ?))"

            parameters.add(storageName)
            parameters.add(storageName)
        }

        if (withoutStorage)
        {
            sumQuantityFilter += " AND (IFNULL(StorageItem.StorageName, '') = '')"
        }

        val sumQuantitySelect = "SELECT SUM(Quantity)" +
                " FROM StorageItem" +
                " WHERE StorageItem.ArticleId = Article.ArticleId" +
                sumQuantityFilter

        var query = """
            SELECT Article.ArticleId, Name, WarnInDays, Size, Unit, DurableInfinity, MinQuantity,
            PrefQuantity, Price, Calorie, Category, SubCategory, Article.StorageName,
              ShoppingList.Quantity AS ShoppingQuantity,
              ($sumQuantitySelect)  AS StorageQuantity,
              IFNULL(($bestBeforeSelect), '9999.12.31') AS BestBefore
              FROM Article
              LEFT JOIN ShoppingList ON ShoppingList.ArticleId = Article.ArticleId
        """.trimIndent()

        var filter = ""

        if (!showNotInStorageArticles) {
            filter += if (filter.isEmpty()) " WHERE " else " AND "
            filter += "Article.ArticleId IN (SELECT StorageItem.ArticleId FROM StorageItem)"
        }

        if (!category.isNullOrEmpty()) {
            filter += if (filter.isEmpty()) " WHERE " else " AND "
            filter += "Article.Category = ?"
            parameters.add(category)
        }

        if (subCategory != null) {
            filter += if (filter.isEmpty()) " WHERE " else " AND "
            filter += "Article.SubCategory = ?"
            parameters.add(subCategory)
        }

        if (!eanCode.isNullOrEmpty()) {
            filter += if (filter.isEmpty()) " WHERE " else " AND "
            filter += "Article.EANCode LIKE ?"
            parameters.add("%$eanCode%")
        }

        if (!textFilter.isNullOrEmpty()) {
            filter += if (filter.isEmpty()) " WHERE " else " AND "
            filter += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?" +
                    " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ? OR Article.EANCode LIKE ?)"
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
        }

        if (!storageName.isNullOrEmpty())
        {
            filter += if (filter.isEmpty()) " WHERE " else " AND "
            filter += " (Article.StorageName = ? OR Article.ArticleId IN (SELECT ArticleId FROM StorageItem WHERE StorageName = ?))"
            parameters.add(storageName)
            parameters.add(storageName)
        }

        if (withoutStorage)
        {
            filter += if (filter.isEmpty()) " WHERE " else " AND "
            filter += " ($sumQuantitySelect) IS NOT NULL"
        }

        query += filter

        if (orderByToConsumeDate)
        {
            query += " ORDER BY BestBefore ASC, Article.Name COLLATE NOCASE"
        }
        else
        {
            query += " ORDER BY Article.Name COLLATE NOCASE"
        }

        val result = mutableListOf<ArticleInfo>()
        try {
            val cursor = db!!.rawQuery(query, parameters.toTypedArray())
            cursor.use {
                while (it.moveToNext()) {
                    val storageItem = ArticleInfo.fromCursor(it)
                    result.add(storageItem)
                }
            }
        }
        catch (e: android.database.sqlite.SQLiteException)
        {
            throw e
        }

        return result
    }

    fun getStorageItemQuantityListToCsvExport() : MutableList<StorageItemCsvExport>
    {
        val result = mutableListOf<StorageItemCsvExport>()

        if (db == null)
            return result

        val query = """
            SELECT StorageItemId,
                   StorageItem.ArticleId,
                   Name,
                   Manufacturer,
                   Category,
                   SubCategory,
                   Article.StorageName AS ArticleStorageName,
                   DurableInfinity,
                   WarnInDays,
                   Quantity,
                   BestBefore,
                   StorageItem.StorageName
            FROM StorageItem
            JOIN Article ON StorageItem.ArticleId = Article.ArticleId
            ORDER BY StorageItem.ArticleId, BestBefore DESC
            """.trimIndent()

        val cursor = db!!.rawQuery(query, null)
        cursor.use {
            while (it.moveToNext()) {
                val storageItem = StorageItemCsvExport.fromCursor(it)
                result.add(storageItem)
            }
        }

        return result
    }

    fun getBestBeforeItemQuantity(
        articleId: Int,
        storageNameFilter: String? = null,
        withoutStorage: Boolean = false) : List<StorageItem>
    {
        val result = mutableListOf<StorageItem>()

        if (db == null)
            return result

        val parameters = mutableListOf<String>()

        var query = """
            SELECT StorageItem.StorageName, BestBefore, SUM(Quantity) AS Quantity, Article.WarnInDays, Article.DurableInfinity, Article.ArticleId
             FROM StorageItem
             LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId
             WHERE StorageItem.ArticleId = ?
        """

        parameters.add(articleId.toString())

        if (!storageNameFilter.isNullOrEmpty())
        {
            // Positionen direkt mit dem Lager oder Positionen ohne Lager, aber wenn der Artikel das Lager hat.
            query += " AND (StorageItem.StorageName = ?"
            query += "  OR (IFNULL(StorageItem.StorageName, '') = '') AND StorageItem.ArticleId IN (SELECT ArticleId FROM Article WHERE Article.StorageName = ?))"

            parameters.add(storageNameFilter)
            parameters.add(storageNameFilter)
        }
        if (withoutStorage)
        {
            query += " AND (IFNULL(StorageItem.StorageName, '') = '')"
        }
        query += " GROUP BY BestBefore"
        query += " ORDER BY BestBefore"

        val cursor = db!!.rawQuery(query.trimIndent(), parameters.toTypedArray())
        cursor.use {
            while (it.moveToNext()) {
                val storageItem = StorageItem.fromCursor(it)
                result.add(storageItem)
            }
        }
        return result
    }

    fun getShoppingList(supermarket: String? = null, textFilter: String? = null, orderBy: Int? = null) : List<ShoppingItem>
    {
        val result = mutableListOf<ShoppingItem>()

        if (db == null)
            return result

        val parameters = mutableListOf<String>()

        var query = """
            SELECT ShoppingListId, Article.ArticleId, Name, Manufacturer, Supermarket, Size, Unit, Calorie, Quantity, Notes, Price, Bought, Category, SubCategory
             FROM ShoppingList
             LEFT JOIN Article ON ShoppingList.ArticleId = Article.ArticleId
        """.trimIndent()

        if (!textFilter.isNullOrEmpty())
        {
            if (parameters.count() > 0)
                query += " AND "
            else
                query += " WHERE "

            query += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?"
            query += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ? OR Article.EANCode LIKE ?)"
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
            parameters.add("%$textFilter%")
        }

        if (!supermarket.isNullOrEmpty())
        {
            if (parameters.count() > 0)
                query += " AND "
            else
                query += " WHERE "

            query += " Supermarket LIKE  ?"
            parameters.add("%$supermarket%")
        }

        when (orderBy) {
            1 -> query += " ORDER BY Bought, Supermarket COLLATE NOCASE, Category COLLATE NOCASE, Name COLLATE NOCASE"
            2 -> query += " ORDER BY Supermarket COLLATE NOCASE, Bought, Category COLLATE NOCASE, Name COLLATE NOCASE"
            3 -> query += " ORDER BY ShoppingListId"
            4 -> query += " ORDER BY Name COLLATE NOCASE"
        }

        val cursor = db!!.rawQuery(query, parameters.toTypedArray())
        cursor.use {
            while (it.moveToNext()) {
                val shoppingItem = ShoppingItem.fromCursor(it)
                result.add(shoppingItem)
            }
        }
        return result
    }

    fun isArticleInShoppingList(articleId: Int) : Boolean
    {
        if (db == null)
            return false

        val query = """
            SELECT COUNT(*)
            FROM ShoppingList
            WHERE ArticleId = ?
        """.trimIndent()

        val cursor = db!!.rawQuery(query, arrayOf(articleId.toString()))
        cursor.use {
            if (it.moveToFirst()) {
                return it.getInt(0) > 0
            }
        }
        return false
    }

    fun setShoppingItemQuantity(articleId: Int, buyQty: Double)
    {
        if (db == null)
            return

        val isInList = this.isArticleInShoppingList(articleId)
        if (!isInList)
        {
            val query = """
                INSERT INTO ShoppingList (ArticleId, Quantity)
                VALUES (?, ?)
            """.trimIndent()
            db!!.execSQL(query, arrayOf<Any>(articleId, buyQty))
        }
        else
        {
            val query = """
                UPDATE ShoppingList
                SET Quantity = ?
                WHERE ArticleId = ?
            """.trimIndent()
            db!!.execSQL(query, arrayOf<Any>(buyQty, articleId))
        }
        this.increaseChangeCounter()
    }

    fun removeFromShoppingList(articleId: Int)
    {
        if (db == null)
            return

        val query = """
            DELETE FROM ShoppingList
            WHERE ArticleId = ?
        """.trimIndent()

        db!!.execSQL(query, arrayOf(articleId))

        this.increaseChangeCounter()
    }

    fun setShoppingItemBought(articleId: Int, bought: Boolean)
    {
        if (db == null)
            return

        val query = """
            UPDATE ShoppingList
            SET Bought = ?
            WHERE ArticleId = ?
        """.trimIndent()

        db!!.execSQL(query, arrayOf<Any>(bought, articleId))

        this.increaseChangeCounter()
    }

    fun addToShoppingList(articleId: Int, addQuantity: Double)
    {
        if (db == null)
            return

        val isQuantity = this.getShoppingListQuantiy(articleId, 0.0)
        var newQuantity = isQuantity!!.plus(addQuantity)

        if (newQuantity < 0)
            newQuantity = 0.0

        val parameters = mutableListOf<Any>()

        var query: String

        val isInList = this.isArticleInShoppingList(articleId)
        if (!isInList)
        {
            query = "INSERT INTO ShoppingList (ArticleId, Quantity) VALUES (?, ?)"
            parameters.add(articleId)
            parameters.add(newQuantity)
        }
        else
        {
            query = "UPDATE ShoppingList SET Quantity = ? WHERE ArticleId = ?"
            parameters.add(newQuantity)
            parameters.add(articleId)
        }

        db!!.execSQL(query, parameters.toTypedArray())

        this.increaseChangeCounter()
    }

    fun getStorageItemQuantityList(articleId: Int) : List<StorageItem>
    {
        val result = mutableListOf<StorageItem>()

        if (db == null)
            return result

        val query = """
            SELECT StorageItem.*, Article.ArticleId, Article.WarnInDays, Article.DurableInfinity
             FROM StorageItem
             LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId
             WHERE StorageItem.ArticleId = ?
             ORDER BY BestBefore ASC
        """.trimIndent()

        val cursor = db!!.rawQuery(query, arrayOf(articleId.toString()))
        cursor.use {
            while (it.moveToNext()) {
                val storageItem = StorageItem.fromCursor(it)
                result.add(storageItem)
            }
        }
        return result
    }

    fun updateStorageItemQuantity(storageItem: StorageItem)
    {
        if (db == null)
            return

        val defaultStorageId = 0

        if (storageItem.storageItemId == 0)
        {
            // Neuanlage, aber Menge = 0
            if (storageItem.quantity == 0.0)
            {
                return
            }

            try {
                val values = ContentValues()
                values.put("StorageId",   defaultStorageId)
                values.put("ArticleId",   storageItem.articleId)
                values.put("Quantity",    storageItem.quantity)
                values.put("BestBefore",  Tools.toString(storageItem.bestBefore))
                values.put("StorageName", storageItem.storageName)

                val newId = db!!.insertOrThrow("StorageItem", null, values)
                storageItem.storageItemId = newId.toInt()
                this.increaseChangeCounter()
            }
            catch (e: Exception)
            {
                TRACE(e.message.toString())
            }
        }
        else
        {
            if (storageItem.quantity == 0.00)
            {
                val query = "DELETE FROM StorageItem WHERE StorageItemId = ?"
                db!!.execSQL(query, arrayOf(storageItem.storageItemId))
                storageItem.storageItemId = 0
                this.increaseChangeCounter()
            }
            else
            {
                val query = """
                    UPDATE StorageItem
                    SET Quantity = ?, BestBefore = ?, StorageName = ?
                    WHERE StorageItemId = ?
                """.trimIndent()
                db!!.execSQL(query, arrayOf<Any?>(storageItem.quantity, storageItem.bestBefore, storageItem.storageName, storageItem.storageItemId))
                this.increaseChangeCounter()
            }
        }
    }

    fun getArticleCount() : Int
    {
        if (db == null)
            return 0

        val query = """
            SELECT COUNT(*) AS Quantity
            FROM Article""".trimIndent()

        val cursor = db!!.rawQuery(query, null)
        cursor.use {
            if (it.moveToFirst()) {
                return it.getInt(0)
            }
        }
        return 0
    }

    fun getArticleCountAbgelaufen() : Double
    {
        if (this.db == null)
            return 0.00

        val query = """
            SELECT SUM(Quantity) AS Quantity
            FROM StorageItem
            JOIN Article ON StorageItem.ArticleId = Article.ArticleId
            WHERE BestBefore < date('now')
            """.trimIndent()

        val cursor = db!!.rawQuery(query, null)
        cursor.use {
            if (it.moveToFirst()) {
                return it.getDouble(0)
            }
        }
        return 0.00
    }

    /// <summary>
    /// Artikel suchen, für die eine Warnung ausgegeben werden soll.
    /// </summary>
    /// <returns></returns>
    fun getArticleCountBaldZuVerbrauchen() : Double
    {
        if (db == null)
            return 0.00

        val query = """
            SELECT SUM(Quantity) AS Quantity
            FROM StorageItem
            JOIN Article ON StorageItem.ArticleId = Article.ArticleId
            WHERE (date(BestBefore,  (-WarnInDays || ' day')) <= date('now'))
            AND BestBefore >= date('now')
            AND WarnInDays <> 0
        """.trimIndent()

        val cursor = db!!.rawQuery(query, null)
        cursor.use {
            if (it.moveToFirst()) {
                return it.getDouble(0)
            }
        }
        return 0.00
    }

    fun getShoppingItemCount() : Double
    {
        if (db == null)
            return 0.00

        val query = """
            SELECT SUM(Quantity) AS Quantity
            FROM ShoppingList
        """.trimIndent()

        val cursor = db!!.rawQuery(query, null)
        cursor.use {
            if (it.moveToFirst()) {
                return it.getDouble(0)
            }
        }
        return 0.00
    }

    fun getSettingsString(key: String) : String?
    {
        if (db == null)
            return null

        val query = """
            SELECT Value
            FROM Settings
            WHERE [Key] = ?
        """.trimIndent()
        val cursor = db!!.rawQuery(query, arrayOf(key))
        cursor.use {
            if (it.moveToFirst()) {
                return it.getString(0)
            }
        }
        return null
    }

    fun clearSettings(key: String)
    {
        if (db == null)
            return

        val query = """
            DELETE FROM Settings
            WHERE [Key] = ?
            """.trimIndent()
        db!!.execSQL(query, arrayOf(key))
    }

    fun getSettingsDate(key: String) : LocalDate?
    {
        val dateText = getSettingsString(key)
        if (dateText == null)
        {
            return null
        }
        try {
            return Tools.stringToDate(dateText)
        }
        catch (_: Exception)
        {
        }
        return null
    }

    fun getSettingsDateTime(key: String) : LocalDateTime?
    {
        val dateText = getSettingsString(key)
        if (dateText == null)
        {
            return null
        }
        try {
            return Tools.stringToDateTime(dateText)
        }
        catch (_: Exception)
        {
        }
        return null
    }

    fun getSettingsInteger(key: String, defaultValue: Int) : Int
    {
        val dateText = getSettingsString(key)
        if (dateText == null)
        {
            return defaultValue
        }
        return dateText.toIntOrNull() ?: defaultValue
    }

    fun getSettingsList(key: String) : MutableList<String>
    {
        val csvText = getSettingsString(key)
        if (csvText == null)
        {
            return mutableListOf()
        }

        val resultList = mutableListOf<String>()

        for (item in csvText.split(","))
        {
            val cleaned = item.trim()
            if (cleaned.isNotEmpty())
            {
                resultList.add(cleaned)
            }
        }

        return resultList
    }

    fun setSettingsDate(key: String, value: LocalDateTime)
    {
        val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd", Locale.getDefault())
        setSettings(key, value.format(formatter))
    }

    fun setSettingsDateTime(key: String, value: LocalDateTime)
    {
        val formatter = DateTimeFormatter.ofPattern("yyyy.MM.dd HH:mm:ss", Locale.getDefault())
        setSettings(key, value.format(formatter))
    }

    fun setSettingsInteger(key: String, value: Int)
    {
        this.setSettings(key, value.toString())
    }

    fun setSettings(key: String, value: String) {

        val oldValue = this.getSettingsString(key)
        if (oldValue == null)
        {
            db!!.execSQL("INSERT INTO Settings ([Key], Value) VALUES (?, ?)", arrayOf(key, value))
        }
        else
        {
            db!!.execSQL("UPDATE Settings SET Value = ? WHERE [Key] = ?", arrayOf(value, key))
        }
    }

    fun getChangeCounter() : Int {
        return this.getSettingsInteger("CHANGES_COUNT", 0)
    }

    fun setChangeCounter(count: Int) {
        this.setSettingsInteger("CHANGES_COUNT", count)
    }

    fun resetChangeCounter() {
        this.clearSettings("CHANGES_COUNT")
    }

    private fun increaseChangeCounter() {
        var count = this.getChangeCounter()

        count++

        this.setChangeCounter(count)
    }

    fun compressDatabase()
    {
        if (db == null)
            return

        db!!.execSQL("UPDATE Article SET Manufacturer = RTRIM(Manufacturer) WHERE LENGTH(Manufacturer) <> LENGTH(TRIM(Manufacturer))")
        db!!.execSQL("UPDATE Article SET SubCategory  = RTRIM(SubCategory)  WHERE LENGTH(SubCategory)  <> LENGTH(TRIM(SubCategory))")
        db!!.execSQL("UPDATE Article SET StorageName  = RTRIM(StorageName)  WHERE LENGTH(StorageName)  <> LENGTH(TRIM(StorageName))")
        db!!.execSQL("UPDATE Article SET Supermarket  = RTRIM(Supermarket)  WHERE LENGTH(Supermarket)  <> LENGTH(TRIM(Supermarket))")

        db!!.execSQL("UPDATE StorageItem SET StorageName = RTRIM(StorageName) WHERE LENGTH(StorageName) <> LENGTH(TRIM(StorageName))")

        db!!.execSQL("VACUUM")
    }

    fun repairDatabase() : String?
    {
        if (db == null)
            return null

        val cursor = db!!.rawQuery("PRAGMA integrity_check", null)
        cursor.use {
            if (it.moveToFirst()) {
                return it.getString(0)
            }
        }
        return null
    }

    fun prepareTestDatabase(context: Context)
    {
        val dbPath = context.getExternalFilesDir(null)
        val testDbFileName = File(dbPath, SQLITE_FILENAME_DEMO)

        var currentDatabase : String? = null
        try {
            currentDatabase = db!!.path
            this.closeDatabase()
        }
        catch (_: Exception) { }

        val bestBefore = LocalDate.of(2000, 1, 1)
        val today = LocalDate.now()
        val span = ChronoUnit.DAYS.between(bestBefore, today)
        val daysAdd = span.toInt()

        this.init(testDbFileName.absolutePath)

        val query = "UPDATE StorageItem SET BestBefore = date(BestBefore, '+%s day')".format(daysAdd.toString())
        db!!.execSQL(query)

        this.closeDatabase()

        if (currentDatabase != null) {
            this.init(currentDatabase)
        }
    }

    fun getToShoppingListQuantity(articleId: Int, minQty: Int?, prefQty: Int?) : Int {

        val minQuantity  = minQty ?: 0.0
        val prefQuantity = prefQty ?: 0.0

        val isQuantityInStorage = getArticleQuantityInStorage(articleId)

        var toBuyQuantity = ShoppingListHelper.getToBuyQuantity(minQuantity.toInt(), prefQuantity.toInt(), isQuantityInStorage.toInt())

        val shoppingListQuantiy = getShoppingListQuantiy(articleId, 0.00)

        toBuyQuantity = toBuyQuantity - shoppingListQuantiy!!.toInt()

        if (toBuyQuantity < 0) // Mehr auf der Einkaufsliste als berechnet?
            return 0

        return toBuyQuantity

    }

    fun deleteStorageItems(storageName: String)
    {
        if (db == null)
            return

        TRACE("Alle Lagerpositionen löschen")
        var query = """
            DELETE FROM StorageItem
            WHERE StorageName = ?
        """.trimIndent()

        db!!.execSQL(query, arrayOf(storageName))

        query = """
            DELETE FROM StorageItem
            WHERE (StorageName = '' OR StorageName IS NULL)
            AND ArticleId IN (SELECT ArticleId FROM Article WHERE StorageName = ?)
        """.trimIndent()

        db!!.execSQL(query, arrayOf(storageName))
    }

    fun renameStorageName(oldStorageName: String, newStorageName: String)
    {
        if (db == null)
            return

        TRACE("Lagerposition umbenennen")

        val updateStorage = """
            UPDATE StorageItem
            SET StorageName = ?
            WHERE StorageName = ?
            """.trimIndent()

        db!!.execSQL(updateStorage, arrayOf(newStorageName, oldStorageName))

        val updateArticle = """
            UPDATE Article
            SET StorageName = ?
            WHERE StorageName = ?
        """.trimIndent()
        db!!.execSQL(updateArticle, arrayOf(newStorageName, oldStorageName))
    }

    private fun checkAndUpgradeSchema() {
        if (db == null) return

        try {

            // Update 4.00: Extra Tabelle für Bilder
            if (!isTableInDatabase("ArticleImage"))
            {
                TRACE("Creating ArticleImage table")

                db!!.execSQL(
                    "CREATE TABLE [ArticleImage] (" +
                    " [ImageId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " [ArticleId] INTEGER NOT NULL," +
                    " [Type] INTEGER NOT NULL," +         // 0 - Artikelbild(-er), 1 - z.B. Rüchsicht, 3 - Zutaten
                    " [CreatedAt] DATETIME NOT NULL," +   // Zum Sortieren gedacht
                    " [ImageSmall] IMAGE," +
                    " [ImageLarge] IMAGE);")

                checkAndMoveArticleImages()
            }

            // Update 4.10: Gekauft in Einkaufsliste
            if (!isFieldInTheTable("ShoppingList", "Bought"))
            {
                TRACE("Update 4.10: Gekauft in Einkaufsliste")
                db!!.execSQL("ALTER TABLE ShoppingList ADD COLUMN [Bought] BOOLEAN")
            }

            // Update 4.30
            if (!isFieldInTheTable("StorageItem", "StorageName"))
            {
                TRACE("Update 4.30: StorageName im StorageItem")
                db!!.execSQL("ALTER TABLE StorageItem ADD COLUMN [StorageName] TEXT")
            }
        } catch (e: Exception) {
            TRACE("DB: Fehler bei Schema-Upgrade: ${e.message}")
        }
    }

    private fun checkAndMoveArticleImages() {

        val cmdCopyImages =
            "INSERT INTO ArticleImage (ArticleId, Type, ImageSmall, ImageLarge, CreatedAt)" +
            " SELECT ArticleId, 0,  Image AS ImageSmall, ImageLarge, DATETIME('now')" +
            " FROM Article" +
            " WHERE Article.Image IS NOT NULL" +
            " AND Article.ArticleId NOT IN (SELECT ArticleId FROM ArticleImage)" +
            " ORDER BY Name COLLATE NOCASE"

        val cmdClearImages =
            "UPDATE Article" +
            " SET Image = NUll, ImageLarge = NULL" +
            " WHERE ArticleId IN (SELECT ArticleId FROM ArticleImage)"

        TRACE("Moving Images to ArticleImage")

        try {
            db!!.beginTransaction()
            db!!.execSQL(cmdCopyImages)
            db!!.execSQL(cmdClearImages)
            db!!.setTransactionSuccessful()
        }
        catch (e: Exception)
        {
            Log.e("stryi", e.toString())
            TRACE(e.message)
        }
        finally {
            db!!.endTransaction()
        }

    }

    private fun isFieldInTheTable(tableName: String, fieldName: String): Boolean
    {
        val cursor = db!!.rawQuery("PRAGMA table_info($tableName)", null)
        cursor.use {
            val nameIndex = it.getColumnIndex("name")
            while (it.moveToNext())
            {
                val name = it.getString(nameIndex)
                if (name.equals(fieldName, ignoreCase = true))
                {
                    return true
                }
            }
            return false
        }
    }

    fun isTableInDatabase(tableName: String): Boolean
    {
        if (db == null)
            return false

        val query = "SELECT name FROM sqlite_master WHERE type='table' AND name=?"
        val cursor = db!!.rawQuery(query, arrayOf(tableName))
        cursor.use {
            return it.count > 0
        }
    }
}
