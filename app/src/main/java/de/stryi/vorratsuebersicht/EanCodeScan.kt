package de.stryi.vorratsuebersicht

import android.os.Bundle
import android.os.Debug
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import androidx.fragment.app.DialogFragment
import com.google.zxing.ResultPoint
import com.journeyapps.barcodescanner.BarcodeResult
import com.journeyapps.barcodescanner.DecoratedBarcodeView
import com.journeyapps.barcodescanner.BarcodeCallback
import de.stryi.vorratsuebersicht.tools.Settings

class EanCodeScan : DialogFragment() {

    var onResult: ((String) -> Unit)? = null
    var isTorchOn = false

    private lateinit var barcodeView: DecoratedBarcodeView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
    }

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View?
    {
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_ean_code_scan, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?)
    {
        super.onViewCreated(view, savedInstanceState)

        barcodeView = view.findViewById(R.id.zxing_barcode_scanner)
        barcodeView.decodeContinuous(object : BarcodeCallback {
            override fun barcodeResult(result: BarcodeResult) {
                barcodeView.pause() // Nur 1x auslösen
                onResult?.invoke(result.text)
                dismiss()
            }

            override fun possibleResultPoints(resultPoints: List<ResultPoint>) {}
        })

        val cameraId = Settings.getBoolean("UseFrontCameraForEANScan", false)
        barcodeView.cameraSettings.requestedCameraId = if (cameraId) 1 else 0

        val buttonFlash = view.findViewById<Button>(R.id.buttonZxingFlashOnOff)
        buttonFlash.setOnClickListener {
            if (isTorchOn) {
                barcodeView.setTorchOff()
            } else {
                barcodeView.setTorchOn()
            }
            isTorchOn = !isTorchOn
        }

        val buttonCamera = view.findViewById<Button>(R.id.buttonZxingCameraSwitch)
        buttonCamera.setOnClickListener {

            var cameraId = barcodeView.cameraSettings.requestedCameraId

            if (cameraId < 0) { cameraId = 0 }
            cameraId = cameraId + 1
            if (cameraId > 1) { cameraId = 0 }

            barcodeView.pause()
            barcodeView.cameraSettings.requestedCameraId = cameraId
            barcodeView.resume()
        }

        val buttonTest = view.findViewById<Button>(R.id.buttonZxingTest)
        buttonTest.setOnClickListener {
            onResult?.invoke("8076800195057")
            dismiss()
        }
        if (Debug.isDebuggerConnected())
        {
            buttonTest.visibility = View.VISIBLE
        }
    }


    override fun onResume() {
        super.onResume()
        barcodeView.resume()
    }

    override fun onPause() {
        super.onPause()
        barcodeView.pause()
    }
}