package de.stryi.vorratsuebersicht.tools

import android.graphics.Bitmap
import android.graphics.Matrix
import java.io.ByteArrayOutputStream

// Bitmap um 90° drehen
fun Bitmap.rotate(degrees: Float): Bitmap {
    val matrix = Matrix().apply { postRotate(degrees) }
    return Bitmap.createBitmap(this, 0, 0, width, height, matrix, true)
}

        // Bitmap als PNG in ByteArray umwandeln
        fun Bitmap.toPngByteArray(): ByteArray {
    val stream = ByteArrayOutputStream()
    this.compress(Bitmap.CompressFormat.PNG, 100, stream) // quality egal bei PNG
    return stream.toByteArray()
}

// Bild auf bestimmte Größe skalieren
fun Bitmap.resize(newWidth: Int, newHeight: Int): Bitmap {
    return Bitmap.createScaledBitmap(this, newWidth, newHeight, true)
}
