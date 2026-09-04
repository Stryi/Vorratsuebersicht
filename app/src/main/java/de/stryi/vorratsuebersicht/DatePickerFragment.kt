package de.stryi.vorratsuebersicht

import android.graphics.Color
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.core.content.ContextCompat
import androidx.fragment.app.DialogFragment
import androidx.core.graphics.toColorInt
import java.time.LocalDate
import java.time.YearMonth
import java.time.format.DateTimeFormatter

class DatePickerFragment(var date: LocalDate?) : DialogFragment() {

    var baseYear = LocalDate.now().year

    var onResult: ((LocalDate?) -> Unit)? = null

    override fun onCreate(savedInstanceState: Bundle?) {

        if (this.date == null) {
            this.date = LocalDate.now()
        }

        super.onCreate(savedInstanceState)
    }

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View?
    {
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.date_picker_fragemnt, container, false)
    }

    override fun onStart() {
        super.onStart()

        dialog?.window?.setLayout(
            ViewGroup.LayoutParams.MATCH_PARENT,
            ViewGroup.LayoutParams.WRAP_CONTENT
        )
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?)
    {
        super.onViewCreated(view, savedInstanceState)

        val buttonYearBack    = view.findViewById<android.widget.Button>(R.id.year_button_Back)
        val buttonYearForward = view.findViewById<android.widget.Button>(R.id.year_button_Forward)

        buttonYearBack.setOnClickListener    { this.onPrevYearCLicked() }
        buttonYearForward.setOnClickListener { this.onNextYearCLicked() }

        view.findViewById<android.widget.Button>(R.id.year_button1).setOnClickListener { v -> this.onYearClicked(v) }
        view.findViewById<android.widget.Button>(R.id.year_button2).setOnClickListener { v -> this.onYearClicked(v) }
        view.findViewById<android.widget.Button>(R.id.year_button3).setOnClickListener { v -> this.onYearClicked(v) }
        view.findViewById<android.widget.Button>(R.id.year_button4).setOnClickListener { v -> this.onYearClicked(v) }

        view.findViewById<android.widget.Button>(R.id.month_button1).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button2).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button3).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button4).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button5).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button6).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button7).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button8).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button9).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button10).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button11).setOnClickListener { v -> this.onMonthClicked(v) }
        view.findViewById<android.widget.Button>(R.id.month_button12).setOnClickListener { v -> this.onMonthClicked(v) }

        view.findViewById<android.widget.Button>(R.id.day_button1).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button2).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button3).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button4).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button5).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button6).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button7).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button8).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button9).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button10).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button11).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button12).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button13).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button14).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button15).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button16).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button17).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button18).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button19).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button20).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button21).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button22).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button23).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button24).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button25).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button26).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button27).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button28).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button29).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button30).setOnClickListener { v -> this.onDayClicked(v) }
        view.findViewById<android.widget.Button>(R.id.day_button31).setOnClickListener { v -> this.onDayClicked(v) }

        val buttonNoDate = view.findViewById<android.widget.Button>(R.id.buttonNoDate)
        val buttonOk     = view.findViewById<android.widget.Button>(R.id.buttonOk)

        buttonNoDate.setOnClickListener {
            onResult?.invoke(null)
            dismiss()
        }
        buttonOk.setOnClickListener {
            onResult?.invoke(this.date)
            dismiss()
        }

        this.showYears()
        this.updateMonths()
        this.updateDays()
        this.updateMaxMonthDays()
        this.updateSelectedDateDisplay()
    }

    private fun onPrevYearCLicked() {
        this.baseYear--
        this.showYears()
    }

    private fun onNextYearCLicked() {
        this.baseYear++
        this.showYears()
    }

    fun onYearClicked(view: View)
    {
        val yearButton = view as android.widget.Button
        val year = yearButton.text.toString().toInt()

        var day = this.date?.dayOfMonth!!
        val daysInMonth = this.daysInMonth(year, this.date?.monthValue!!)
        if (day > daysInMonth)
            day = daysInMonth

        this.date = LocalDate.of(year, this.date?.month, day)
        this.showYears()
        this.updateDays()
        this.updateMaxMonthDays()
        this.updateSelectedDateDisplay()
    }

    fun onMonthClicked(view: View)
    {
        val monthButton = view as android.widget.Button
        val month = monthButton.text.toString().toInt()

        var day = this.date?.dayOfMonth!!
        val daysInMonth = this.daysInMonth(this.date?.year!!, month)
        if (day > daysInMonth)
            day = daysInMonth

        this.date = LocalDate.of(this.date?.year!!, month, day)
        this.updateMonths()
        this.updateDays()
        this.updateMaxMonthDays()
        this.updateSelectedDateDisplay()
    }

    fun onDayClicked(view: View)
    {
        val dayButton = view as android.widget.Button
        val day = dayButton.text.toString().toInt()
        this.date = LocalDate.of(this.date?.year!!, this.date?.month!!, day)
        this.updateDays()
        this.updateSelectedDateDisplay()
    }

    private fun setYear(yearButton: Int, year: Int) {
        view?.findViewById<android.widget.Button>(yearButton)?.text = year.toString()
        if (year == this.date?.year)
        {
            markAsActive(yearButton)
        }
        else
        {
            markAsInactive(yearButton)
        }
    }

    private fun showYears()
    {
        this.setYear(R.id.year_button1, (baseYear + 0))
        this.setYear(R.id.year_button2, (baseYear + 1))
        this.setYear(R.id.year_button3, (baseYear + 2))
        this.setYear(R.id.year_button4, (baseYear + 3))
    }

    private fun updateMonths() {
        val month = this.date?.monthValue!!

        this.updateStatus(R.id.month_button1, month)
        this.updateStatus(R.id.month_button2, month)
        this.updateStatus(R.id.month_button3, month)
        this.updateStatus(R.id.month_button4, month)
        this.updateStatus(R.id.month_button5, month)
        this.updateStatus(R.id.month_button6, month)
        this.updateStatus(R.id.month_button7, month)
        this.updateStatus(R.id.month_button8, month)
        this.updateStatus(R.id.month_button9, month)
        this.updateStatus(R.id.month_button10, month)
        this.updateStatus(R.id.month_button11, month)
        this.updateStatus(R.id.month_button12, month)
    }

    fun updateMaxMonthDays()
    {
        val month = this.date?.monthValue!!
        val daysInMonth = this.daysInMonth(this.date?.year!!, month)

        if (daysInMonth < 31) {
            view?.findViewById<android.widget.Button>(R.id.day_button31)?.visibility = View.INVISIBLE // Damit die Höhe nicht "springt"
        }
        else{
            view?.findViewById<android.widget.Button>(R.id.day_button31)?.visibility = View.VISIBLE
        }

        if (daysInMonth < 30) {
            view?.findViewById<android.widget.Button>(R.id.day_button30)?.visibility = View.GONE
        }
        else {
            view?.findViewById<android.widget.Button>(R.id.day_button30)?.visibility = View.VISIBLE
        }

        if (daysInMonth < 29) {
            view?.findViewById<android.widget.Button>(R.id.day_button29)?.visibility = View.GONE
        }
        else {
            view?.findViewById<android.widget.Button>(R.id.day_button29)?.visibility = View.VISIBLE
        }

        if (daysInMonth < 28) {
            view?.findViewById<android.widget.Button>(R.id.day_button28)?.visibility = View.GONE
        }
        else {
            view?.findViewById<android.widget.Button>(R.id.day_button28)?.visibility = View.VISIBLE
        }
    }

    private fun updateDays()
    {
        val day = this.date!!.dayOfMonth

        this.updateStatus(R.id.day_button1,  day)
        this.updateStatus(R.id.day_button2,  day)
        this.updateStatus(R.id.day_button3,  day)
        this.updateStatus(R.id.day_button4,  day)
        this.updateStatus(R.id.day_button5,  day)
        this.updateStatus(R.id.day_button6,  day)
        this.updateStatus(R.id.day_button7,  day)
        this.updateStatus(R.id.day_button8,  day)
        this.updateStatus(R.id.day_button9,  day)
        this.updateStatus(R.id.day_button10, day)
        this.updateStatus(R.id.day_button11, day)
        this.updateStatus(R.id.day_button12, day)
        this.updateStatus(R.id.day_button13, day)
        this.updateStatus(R.id.day_button14, day)
        this.updateStatus(R.id.day_button15, day)
        this.updateStatus(R.id.day_button16, day)
        this.updateStatus(R.id.day_button17, day)
        this.updateStatus(R.id.day_button18, day)
        this.updateStatus(R.id.day_button19, day)
        this.updateStatus(R.id.day_button20, day)
        this.updateStatus(R.id.day_button21, day)
        this.updateStatus(R.id.day_button22, day)
        this.updateStatus(R.id.day_button23, day)
        this.updateStatus(R.id.day_button24, day)
        this.updateStatus(R.id.day_button25, day)
        this.updateStatus(R.id.day_button26, day)
        this.updateStatus(R.id.day_button27, day)
        this.updateStatus(R.id.day_button28, day)
        this.updateStatus(R.id.day_button29, day)
        this.updateStatus(R.id.day_button30, day)
        this.updateStatus(R.id.day_button31, day)
    }

    private fun updateStatus(buttonId: Int, value: Int) {
        val button = this.view?.findViewById<android.widget.Button>(buttonId)
        val textStr = button?.text?.toString()
        if (textStr != null && textStr.isNotEmpty()) {
            val text = textStr.toIntOrNull()
            if (text == value)
            {
                markAsActive(buttonId)
            }
            else if (text != null)
            {
                markAsInactive(buttonId)
            }
        }
    }

    private fun markAsActive(buttonId: Int) {
        val button = this.view?.findViewById<android.widget.Button>(buttonId)
        button?.setTextColor(Color.RED)
        button?.setBackgroundColor("#BBBBBB".toColorInt())
    }

    private fun markAsInactive(buttonId: Int) {
        val button = this.view?.findViewById<android.widget.Button>(buttonId)
        button?.let {
            val color = ContextCompat.getColor(it.context, R.color.Default_Text_Color)
            it.setTextColor(color)
            it.setBackgroundColor(Color.TRANSPARENT)
        }
    }

    fun daysInMonth(year: Int, month: Int): Int {
        val ym = YearMonth.of(year, month)
        return ym.lengthOfMonth()
    }

    private fun updateSelectedDateDisplay() {
        val textView = view?.findViewById<android.widget.TextView>(R.id.textViewSelectedDate)
        this.date?.let {
            val formatter = DateTimeFormatter.ofPattern("dd.MM.yyyy")
            textView?.text = it.format(formatter)
        } ?: run {
            textView?.text = ""
        }
    }

}
