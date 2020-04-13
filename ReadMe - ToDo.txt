Prio 1
======

Artikeldetails
- "Auf Einkaufsliste" mit Eingabe-Dialog für die Menge (nach Mindestbestand schon vorgeblendet)

Lagerbestand
- Manuelle Datumseingabe (ohne scrollen zu müssen), z.B. mit Buttons "2020", "2021", Monat "1", "2",...

EAN Scan
- Besseren EAN Scanner suchen 
  Die Java Variante. Da wir noch ältere Android Versionen unterstützen so:
  implementation('com.journeyapps:zxing-android-embedded:3.6.0') { transitive = false }
  implementation 'com.google.zxing:core:3.3.0' 
  https://github.com/journeyapps/zxing-android-embedded 

Sonstiges
- Push Benachrichtigung über bald zu verbrauchende Artikel einmal pro Tag
  Gewünscht von Frank (E-Mail), und von Eva (?)
  https://docs.microsoft.com/de-de/xamarin/android/app-fundamentals/notifications/local-notifications-walkthrough
  https://docs.microsoft.com/de-de/xamarin/android/app-fundamentals/services/service-notifications
- Datenbankauswahl am Anfang mit "Zurück" Taste soll nicht die "Vorraete.db3" erstellen sondern die erste Datenbank nehmen.
  (Freie Auswahl des Datenbanknamens)



Prio 2
======

Einkaufszettel
- Liste automatisch anhand der Mindestmenge erstellen.

Einkaufsliste
- Sortierung: Alphabetisch, Kategorie und Gekauft.

Lagerbestand
- Wird ein Datum < Heute eingegeben, so wird die Farbe nicht auf blau oder rot gesetzt.

Artikelstamm
- Default Werte vorbelegen (Martin T aus CH)
- Mehrere Bilder Pro Artikel

Einstellungen
- Automatischen Backup ("Alle X Tage...", "Y Kopien behalten")
- Freie Sortierung der Kategorien. (M. Pleuger)

Lagerliste
- Zusammenfügen von Unterkategorien bei der Mindestmenge. 
  So das z.B die Unterkategorie Kaffee auf 5 Pack steht.
  Und nicht Hersteller A ein Pack, Hersteller B zwei Pack (vom Markus Neese vorgeschlagen)
- Auswahl Lagerort auch an Subkategorie berücksichtigen


Sonstiges
- Release Notes der App anzeigen
- Spracheingabe, https://docs.microsoft.com/de-de/xamarin/android/platform/speech
- Absturz protokollieren (LOG-Datei)

Synchronisierung
- https://syncthing.net
- https://github.com/rqlite/rqlite - ...distributed relational database, which uses SQLite
- http://litesync.io

Prio 3
======

Von Rezension Soe Ungezähmt am 11.04.2020 um 20:58

    Was cool wäre, wäre eine Option für geöffnete Produkte bei deren Aktivierung
    das MHD/zu verbrauchen bis-Datum auf eine, je nach Produkt, 
    bestimmte Anzahl von Tagen reduziert wird. Ggf. nach Ablauf,
    wird das geöffnete Produkt dann automatisch aus der Datenbank entfernt. :)


Bilder
- Bild entfernen programmieren


Lagerbestand / Artikelliste
- Bilder im Thread oder asynchron laden
  https://blog.xamarin.com/getting-started-with-async-await/


Lagerbestand
- Anzahl gruppieren und Summieren nach Datum.
- Lagerbewegung in einer Tabelle speichern


Gemeldete Abstürze:
===================

Huawei HUAWEI P30 lite (HWMAR), 3840MB RAM, Android 9
Bericht 1
android.runtime.JavaProxyThrowable: at VorratsUebersicht.StorageItemQuantityActivity.ShowPictureAndDetails (System.Int32 articleId, System.String title) [0x00098] in <ab43b9b3430149ab83a338ec4cdd50b2>:0
at VorratsUebersicht.StorageItemQuantityActivity.OnActivityResult (System.Int32 requestCode, Android.App.Result resultCode, Android.Content.Intent data) [0x0002b] in <ab43b9b3430149ab83a338ec4cdd50b2>:0
at Android.App.Activity.n_OnActivityResult_IILandroid_content_Intent_ (System.IntPtr jnienv, System.IntPtr native__this, System.Int32 requestCode, System.Int32 native_resultCode, System.IntPtr native_data) [0x00014] in <b781ed64f1d743e7881ac038e0fbdf85>:0
at (wrapper dynamic-method) System.Object.7(intptr,intptr,int,int,intptr)
at md56c9fe683bd4750f69443fa5376e732f4.StorageItemQuantityActivity.n_onActivityResult (Native Method)
at md56c9fe683bd4750f69443fa5376e732f4.StorageItemQuantityActivity.onActivityResult (StorageItemQuantityActivity.java:66)
at android.app.Activity.dispatchActivityResult (Activity.java:7797)
at android.app.ActivityThread.deliverResults (ActivityThread.java:5071)
at android.app.ActivityThread.handleSendResult (ActivityThread.java:5120)
at android.app.servertransaction.ActivityResultItem.execute (ActivityResultItem.java:49)
at android.app.servertransaction.TransactionExecutor.executeCallbacks (TransactionExecutor.java:108)
at android.app.servertransaction.TransactionExecutor.execute (TransactionExecutor.java:68)
at android.app.ActivityThread$H.handleMessage (ActivityThread.java:2199)
at android.os.Handler.dispatchMessage (Handler.java:112)
at android.os.Looper.loop (Looper.java:216)
at android.app.ActivityThread.main (ActivityThread.java:7625)
at java.lang.reflect.Method.invoke (Native Method)
at com.android.internal.os.RuntimeInit$MethodAndArgsCaller.run (RuntimeInit.java:524)
at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:987)  
