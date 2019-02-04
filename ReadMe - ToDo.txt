Prio 1
======

Kategorie/Unterkategorie
- "Keine Kategorie" als Popup Auswahl?


Sonstiges
- Push Benachrichtigung über bald zu verbrauchende Artikel einmal pro Tag
  https://docs.microsoft.com/de-de/xamarin/android/app-fundamentals/notifications/local-notifications-walkthrough
  https://docs.microsoft.com/de-de/xamarin/android/app-fundamentals/services/service-notifications


Prio 2
======

Artikeldetail
- Aufruf mit EAN Nummer ins Internet

Lagerbestand
- Anzahl nicht nur über "-" und "+" sondern auch direkt eingeben (von Kathrin B. und Michi H. vorgeschlagen)


Einkaufszettel
- Alle Listen als SCV exportieren (Markus Neese)


Einstellungen
- Automatischen Backup ("Alle X Tage...", "Y Kopien behalten")


Lagerliste
- Zusammenfügen von Unterkategorien bei der Mindestmenge. 
  So das z.B die Unterkategorie Kaffee auf 5 Pack steht.
  Und nicht Hersteller A ein Pack, Hersteller B zwei Pack (vom Markus Neese vorgeschlagen)
- Auswahl Lagerort auch an Subkategorie berücksichtigen
- Lagebestand als TXT exportieren  (Sharen)

Artikelliste
- Die Möglichkeit Artikel zu kopieren/duplizieren (vom Michi H. vorgeschlagen)
- Artielliste als TXT exportieren (Teilen)


Einkaufszettel
- Liste automatisch anhand der Mindestmenge erstellen.


Sonstiges
- Release Notes der App anzeigen
- Spracheingabe, https://docs.microsoft.com/de-de/xamarin/android/platform/speech
- Absturz protokollieren (LOG-Datei)


Prio 3
======

Bilder
- Bild entfernen programmieren


Lagerbestand / Artikelliste
- Bilder im Thread oder asynchron laden
  https://blog.xamarin.com/getting-started-with-async-await/


Einkaufsliste:
- Aus der Lagerbestand Liste auf die Einkaufsliste setzen ("..." Menü?)


Lagerbestand
- Anzahl gruppieren und Summieren nach Datum.
- Lagerbewegung in einer Tabelle speichern


Artikeldetails
- Mehrere Bilder Pro Artikel


Artikel scannen (Anhaben aus EAN Datenbank, kostenpflichtig?)
- https://www.codecheck.info/product.search?q=4006544205006
- https://corporate.codecheck.info/produkte/produktdaten-api/
- https://www.ean-suche.de/?q=4000462810052


Gemeldete Abstürze:
===================


1. Okt. 13:16 in der App-Version 31
LGE LG G6 (lucye), Android 8.0
Bericht 1 von 1
android.runtime.JavaProxyThrowable: at System.IO.File.Delete (System.String path) [0x00078] in <f32579baafc1404fa37ba3ec1abdc0bd>:0
at VorratsUebersicht.Android_Database.DeleteDatabase () [0x00008] in <1510bfb8da6c4598984cdc2b75c68e63>:0
at VorratsUebersicht.SettingsActivity+<>c.<ButtonDeleteDb_Click>b__5_0 (System.Object s, Android.Content.DialogClickEventArgs ev) [0x00006] in <1510bfb8da6c4598984cdc2b75c68e63>:0
at Android.Content.IDialogInterfaceOnClickListenerImplementor.OnClick (Android.Content.IDialogInterface dialog, System.Int32 which) [0x00012] in <263adecfa58f4c449f1ff56156d886fd>:0
at Android.Content.IDialogInterfaceOnClickListenerInvoker.n_OnClick_Landroid_content_DialogInterface_I (System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_dialog, System.Int32 which) [0x0000f] in <263adecfa58f4c449f1ff56156d886fd>:0
at (wrapper dynamic-method) System.Object.b89e0aed-5b9f-4cb1-b2df-da0c2d713d76(intptr,intptr,intptr,int)
  at mono.android.content.DialogInterface_OnClickListenerImplementor.n_onClick (Native Method)
  at mono.android.content.DialogInterface_OnClickListenerImplementor.onClick (DialogInterface_OnClickListenerImplementor.java:30)
  at com.android.internal.app.AlertController$ButtonHandler.handleMessage (AlertController.java:166)
  at android.os.Handler.dispatchMessage (Handler.java:105)
  at android.os.Looper.loop (Looper.java:164)
  at android.app.ActivityThread.main (ActivityThread.java:6710)
  at java.lang.reflect.Method.invoke (Native Method)
  at com.android.internal.os.Zygote$MethodAndArgsCaller.run (Zygote.java:240)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:770)

  
3. Okt. 19:02 in der App-Version 31
Huawei Honor 5X (HNKIW-Q), Android 5.1
Bericht 1 von 1
android.runtime.JavaProxyThrowable: at VorratsUebersicht.StorageItemQuantityActivity.OnPrepareOptionsMenu (Android.Views.IMenu menu) [0x00049] in <1510bfb8da6c4598984cdc2b75c68e63>:0
at Android.App.Activity.n_OnPrepareOptionsMenu_Landroid_view_Menu_ (System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_menu) [0x0000f] in <263adecfa58f4c449f1ff56156d886fd>:0
at (wrapper dynamic-method) System.Object.4d48c13c-d765-44ad-bef6-031a8ded5cfc(intptr,intptr,intptr)
  at md56c9fe683bd4750f69443fa5376e732f4.StorageItemQuantityActivity.n_onPrepareOptionsMenu (Native Method)
  at md56c9fe683bd4750f69443fa5376e732f4.StorageItemQuantityActivity.onPrepareOptionsMenu (StorageItemQuantityActivity.java:50)
  at android.app.Activity.onPreparePanel (Activity.java:2932)
  at com.android.internal.policy.impl.PhoneWindow.preparePanel (PhoneWindow.java:579)
  at com.android.internal.policy.impl.PhoneWindow.doInvalidatePanelMenu (PhoneWindow.java:927)
  at com.android.internal.policy.impl.PhoneWindow$1.run (PhoneWindow.java:261)
  at android.os.Handler.handleCallback (Handler.java:739)
  at android.os.Handler.dispatchMessage (Handler.java:95)
  at android.os.Looper.loop (Looper.java:135)
  at android.app.ActivityThread.main (ActivityThread.java:5669)
  at java.lang.reflect.Method.invoke (Native Method)
  at java.lang.reflect.Method.invoke (Method.java:372)
  at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run (ZygoteInit.java:960)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:755)