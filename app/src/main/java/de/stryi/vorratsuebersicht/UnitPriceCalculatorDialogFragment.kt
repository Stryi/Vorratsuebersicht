package de.stryi.vorratsuebersicht

import android.app.Dialog
import android.os.Bundle
import android.text.Editable
import android.text.TextWatcher
import android.view.LayoutInflater
import android.view.WindowManager
import androidx.appcompat.app.AlertDialog
import androidx.fragment.app.DialogFragment
import de.stryi.vorratsuebersicht.databinding.DialogUnitPriceCalculatorBinding
import java.text.DecimalFormat
import java.text.DecimalFormatSymbols
import java.util.Locale

class UnitPriceCalculatorDialogFragment : DialogFragment() {

    private var _binding: DialogUnitPriceCalculatorBinding? = null
    private val binding get() = _binding!!

    override fun onCreateDialog(savedInstanceState: Bundle?): Dialog {
        _binding = DialogUnitPriceCalculatorBinding.inflate(LayoutInflater.from(requireContext()))

        val textWatcher = object : TextWatcher {
            override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) {}
            override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) {
                calculateAndShow()
            }
            override fun afterTextChanged(s: Editable?) {}
        }

        binding.UnitPriceCalcQuantity.addTextChangedListener(textWatcher)
        binding.UnitPriceCalcPrice.addTextChangedListener(textWatcher)

        binding.UnitPriceCalcUnitGroup.setOnCheckedChangeListener { _, _ ->
            calculateAndShow()
        }

        calculateAndShow()

        val builder = AlertDialog.Builder(requireContext(), R.style.MyAlertDialogTheme)
        builder.setTitle(R.string.ShoppingList_UnitPriceCalculator)
        builder.setView(binding.root)
        builder.setPositiveButton(R.string.App_Ok) { dialog, _ ->
            dialog.dismiss()
        }

        val dialog = builder.create()
        dialog.setOnShowListener {
            binding.UnitPriceCalcPrice.requestFocus()
            dialog.window?.setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE)
        }
        return dialog
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }

    private fun calculateAndShow() {
        if (_binding == null) return

        val quantityStr = binding.UnitPriceCalcQuantity.text.toString().replace(',', '.').trim()
        val priceStr = binding.UnitPriceCalcPrice.text.toString().replace(',', '.').trim()

        val quantity = quantityStr.toDoubleOrNull()
        val price = priceStr.toDoubleOrNull()

        val selectedUnitId = binding.UnitPriceCalcUnitGroup.checkedRadioButtonId

        val isMass = selectedUnitId == R.id.UnitPriceCalc_RadioKg || selectedUnitId == R.id.UnitPriceCalc_RadioG
        val isKgOrL = selectedUnitId == R.id.UnitPriceCalc_RadioKg || selectedUnitId == R.id.UnitPriceCalc_RadioL

        val label1Res = if (isMass) R.string.UnitPriceCalculator_PricePerKg else R.string.UnitPriceCalculator_PricePerL
        val label2Res = if (isMass) R.string.UnitPriceCalculator_PricePer100G else R.string.UnitPriceCalculator_PricePer100Ml

        val label1 = getString(label1Res)
        val label2 = getString(label2Res)

        if (quantity == null || quantity <= 0.0 || price == null || price < 0.0) {
            binding.UnitPriceCalcResult1.text = "$label1 -"
            binding.UnitPriceCalcResult2.text = "$label2 -"
            return
        }

        val pricePerLargeUnit: Double
        val pricePer100Unit: Double

        if (isKgOrL) {
            pricePerLargeUnit = price / quantity
            pricePer100Unit = pricePerLargeUnit / 10.0
        } else {
            pricePer100Unit = (price / quantity) * 100.0
            pricePerLargeUnit = pricePer100Unit * 10.0
        }

        binding.UnitPriceCalcResult1.text = "$label1 ${formatPrice(pricePerLargeUnit)} €"
        binding.UnitPriceCalcResult2.text = "$label2 ${formatPrice(pricePer100Unit)} €"
    }

    private fun formatPrice(price: Double): String {
        val symbols = DecimalFormatSymbols(Locale.getDefault())
        return if (price < 0.01 && price > 0.0) {
            DecimalFormat("0.00####", symbols).format(price)
        } else {
            DecimalFormat("###,###,##0.00", symbols).format(price)
        }
    }

    companion object {
        fun newInstance(): UnitPriceCalculatorDialogFragment {
            return UnitPriceCalculatorDialogFragment()
        }
    }
}
