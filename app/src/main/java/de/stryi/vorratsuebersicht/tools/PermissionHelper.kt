package de.stryi.vorratsuebersicht.tools

import android.app.Activity
import android.content.pm.PackageManager
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat

class PermissionHelper {
    fun requestPermission(activity: Activity, permission: String, requestCode: Int, callback: () -> Unit)
    {
        if (ContextCompat.checkSelfPermission(activity, permission) == PackageManager.PERMISSION_GRANTED)
        {
            callback()
        }
        else
        {
            ActivityCompat.requestPermissions(activity, arrayOf(permission), requestCode)
        }
    }
}