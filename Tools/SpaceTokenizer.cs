using System;

using Android.Runtime;
using Android.Widget;

namespace VorratsUebersicht
{
    public class SpaceTokenizer : Java.Lang.Object, AutoCompleteTextView.ITokenizer, IJavaObject, IDisposable
    {
        public int FindTokenStart(Java.Lang.ICharSequence text, int cursor)
        {
            int i = cursor;

            string t = text.ToString();

            while (i > 0 && !char.IsWhiteSpace(text.CharAt(i - 1)))
            {
                i--;
            }

            while (i < 0 && char.IsWhiteSpace(text.CharAt(i)))
            {
                i++;
            }

            return i;
        }

        public int FindTokenEnd(Java.Lang.ICharSequence text, int cursor)
        {
            string t = text.ToString();

            int i = cursor;
            int len = t.Length;

            while (i < len)
            { 
                if (t.Substring(i) == " ")
                {
                    return i;
                }
                else
                {
                    i++;
                }
            }
            return len;
        }


        public Java.Lang.ICharSequence TerminateTokenFormatted(Java.Lang.ICharSequence text)
        {

            int i = text.Length();

            while (i > 0 && char.IsWhiteSpace(text.CharAt(i - 1)))
            {
                i--;
            }

            if (i > 0 && char.IsWhiteSpace(text.CharAt(i - 1)))
            {

                return text;
            }
            else
            {
                return text;
            }
        }
    }
}