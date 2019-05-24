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

    https://world.openfoodfacts.org/data
    https://world.openfoodfacts.org/api/v0/product/4000462810052.json
    https://world.openfoodfacts.org/api/v0/product/5410673854001.json?fields=status,code,product_name,energy_unit,brands
    https://world.openfoodfacts.org/api/v0/product/20005016.json?fields=status,code,product_name,energy_unit,brands,quantity,product_quantity,nova_groups

Lagerbestand
Gemacht: - Anzahl nicht nur über "-" und "+" sondern auch direkt eingeben (von Kathrin B. und Michi H. vorgeschlagen)


Einstellungen
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


18. März 19:00 in der App-Version 41
Sony Xperia XZ1 (G8341), Android 9
Bericht 1 von 1
android.runtime.JavaProxyThrowable: at System.IO.__Error.WinIOError (System.Int32 errorCode, System.String maybeFullPath) [0x00129] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.FileSystemEnumerableIterator`1[TSource].HandleError (System.Int32 hr, System.String path) [0x00006] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.FileSystemEnumerableIterator`1[TSource].CommonInit () [0x00054] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.FileSystemEnumerableIterator`1[TSource]..ctor (System.String path, System.String originalUserPath, System.String searchPattern, System.IO.SearchOption searchOption, System.IO.SearchResultHandler`1[TSource] resultHandler, System.Boolean checkHost) [0x000d6] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.FileSystemEnumerableFactory.CreateFileNameIterator (System.String path, System.String originalUserPath, System.String searchPattern, System.Boolean includeFiles, System.Boolean includeDirs, System.IO.SearchOption searchOption, System.Boolean checkHost) [0x00009] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.Directory.InternalGetFileDirectoryNames (System.String path, System.String userPathOriginal, System.String searchPattern, System.Boolean includeFiles, System.Boolean includeDirs, System.IO.SearchOption searchOption, System.Boolean checkHost) [0x00000] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.Directory.InternalGetFiles (System.String path, System.String searchPattern, System.IO.SearchOption searchOption) [0x00000] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.Directory.GetFiles (System.String path, System.String searchPattern) [0x0001c] in <fe08c003e91342eb83df1ca48302ddbb>:0
at System.IO.DirectoryInfo.GetFiles (System.String searchPattern) [0x0000e] in <fe08c003e91342eb83df1ca48302ddbb>:0
at VorratsUebersicht.SelectFileActivity.ShowFileList () [0x0000c] in <399c20d519f74e46a4196a802d43cada>:0
at VorratsUebersicht.SelectFileActivity.OnRequestPermissionsResult (System.Int32 requestCode, System.String[] permissions, Android.Content.PM.Permission[] grantResults) [0x00001] in <399c20d519f74e46a4196a802d43cada>:0
at Android.App.Activity.n_OnRequestPermissionsResult_IarrayLjava_lang_String_arrayI (System.IntPtr jnienv, System.IntPtr native__this, System.Int32 requestCode, System.IntPtr native_permissions, System.IntPtr native_grantResults) [0x00038] in <ad2f15102b3a4d36b40e9b0cbc11c376>:0
at (wrapper dynamic-method) System.Object.4(intptr,intptr,int,intptr,intptr)
  at md56c9fe683bd4750f69443fa5376e732f4.SelectFileActivity.n_onRequestPermissionsResult (Native Method)
  at md56c9fe683bd4750f69443fa5376e732f4.SelectFileActivity.onRequestPermissionsResult (SelectFileActivity.java:39)
  at android.app.Activity.dispatchRequestPermissionsResult (Activity.java:7608)
  at android.app.Activity.dispatchActivityResult (Activity.java:7458)
  at android.app.ActivityThread.deliverResults (ActivityThread.java:4375)
  at android.app.ActivityThread.handleSendResult (ActivityThread.java:4424)
  at android.app.servertransaction.ActivityResultItem.execute (ActivityResultItem.java:49)
  at android.app.servertransaction.TransactionExecutor.executeCallbacks (TransactionExecutor.java:108)
  at android.app.servertransaction.TransactionExecutor.execute (TransactionExecutor.java:68)
  at android.app.ActivityThread$H.handleMessage (ActivityThread.java:1814)
  at android.os.Handler.dispatchMessage (Handler.java:106)
  at android.os.Looper.loop (Looper.java:280)
  at android.app.ActivityThread.main (ActivityThread.java:6706)
  at java.lang.reflect.Method.invoke (Native Method)
  at com.android.internal.os.RuntimeInit$MethodAndArgsCaller.run (RuntimeInit.java:493)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:858)
  

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