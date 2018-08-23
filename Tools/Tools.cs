using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    public static class Tools
    {
        public static T Cast<T>(this Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }

        // https://dotnet-snippets.de/snippet/byte-groessenangaben-als-string-formatieren-kb-mb-gb/1304
        public static string ToFuzzyByteString(long bytes)
        {                    
            double s = bytes; 
            string[] format = new string[]
                  {
                      "{0} bytes", "{0} KB",  
                      "{0} MB", "{0} GB", "{0} TB", "{0} PB", "{0} EB"
                  };

            int i = 0;

            while (i < format.Length && s >= 1024)              
            {                                     
                s = (long) (100*s/1024)/100.0;  
                i++;            
            }                     
            return string.Format(format[i], s);  
        }

        public static void TRACE(string text)
        {
            Log.WriteLine(LogPriority.Debug, "stryi", text);
        }
        public static void TRACE(string format, params object[] args)
        {
            Log.WriteLine(LogPriority.Debug, "stryi", string.Format(format, args));
        }
    }
}