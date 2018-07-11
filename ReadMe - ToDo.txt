
Prio 1
======

Hauptmaske
- Hinweis, dass die Datenbank nicht auf der SD Karte sich befindet
- Hinweis zum Backup der Datenbank (1 x Monat?)

Prio 2
======

Anwendung
- warning CS0618: 'ProgressDialog' is obsolete: 'This class is obsoleted in this android platform'


Bilder
- Komprimierung/Verkleinerung der Bilder nicht immer effizient
- Ausrichtung der Bilder beachten (Quer fotografiert)
- Nach dem Auswahl vom neuen Bild wird noch beim Anklicken das alte angezeigt.
- Bild entfernen programmieren

Kategorie/Unterkategorie
- Unterkategorie Control auf PopUp umstellen, 
  damit man immer alles anzeigen kann, nicht nur bei Eingabe.
- Selbst definierte Kategorien
- "Keine Kategorie" als Popup Auswahl

Artikel-/Bestandliste
- Sortieren der Lagerbestandsliste (Name, Zum Verbrauchen zuerst)

Artikeldetail
- Bilder für das Menü korrigieren
- Lagerort

Settings
- Release Notes der App anzeigen
- Spracheingabe, https://docs.microsoft.com/de-de/xamarin/android/platform/speech

Artikel scannen
- https://corporate.codecheck.info/produkte/produktdaten-api/

Prio 3
======

Einkaufsliste:
- Aus der Lagerbestand Liste auf die Einkaufsliste setzen ("..." Menü?)

- Anzahl gruppieren und Summieren nach Datum.

- Suche im Artikelstamm und Lagerbestand (nach Text oder Hersteller).

- Mehrere Bilder Pro Artikel

- Artikel Liste auf Englische Resourcen setzen

- Für Hinzufügen von Positionen, Artikel oder Bestand ein "Floating Action Button" verwenden
  https://guides.codepath.com/android/floating-action-buttons



Gemeldete Abstürze:
===================


30.06.2018 11:51 in der App-Version 16
Samsung Galaxy S7 (herolte), Android 8.0
Bericht 1 von 1
java.lang.RuntimeException: 
  at android.app.ActivityThread.performResumeActivity (ActivityThread.java:3790)
  at android.app.ActivityThread.handleResumeActivity (ActivityThread.java:3830)
  at android.app.ActivityThread.handleLaunchActivity (ActivityThread.java:3038)
  at android.app.ActivityThread.handleRelaunchActivity (ActivityThread.java:4921)
  at android.app.ActivityThread.-wrap19 (Unknown Source)
  at android.app.ActivityThread$H.handleMessage (ActivityThread.java:1702)
  at android.os.Handler.dispatchMessage (Handler.java:105)
  at android.os.Looper.loop (Looper.java:164)
  at android.app.ActivityThread.main (ActivityThread.java:6944)
  at java.lang.reflect.Method.invoke (Native Method)
  at com.android.internal.os.Zygote$MethodAndArgsCaller.run (Zygote.java:327)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:1374)
Caused by: java.lang.RuntimeException: 
  at android.app.ActivityThread.deliverResults (ActivityThread.java:4491)
  at android.app.ActivityThread.performResumeActivity (ActivityThread.java:3762)
Caused by: java.lang.NullPointerException: 
  at android.widget.AbsListView.onRestoreInstanceState (AbsListView.java:2680)
  at md56c9fe683bd4750f69443fa5376e732f4.StorageItemListActivity.n_onActivityResult (Native Method)
  at md56c9fe683bd4750f69443fa5376e732f4.StorageItemListActivity.onActivityResult (StorageItemListActivity.java:56)
  at android.app.Activity.dispatchActivityResult (Activity.java:7547)
  at android.app.ActivityThread.deliverResults (ActivityThread.java:4487)

10 Berichte
Alle Berichte mit: Samsung Galaxy S7 Edge (hero2lte), 4096MB RAM, Android 7.0

android.runtime.JavaProxyThrowable
md56c9fe683bd4750f69443fa5376e732f4.MainActivity.n_onCreate

android.runtime.JavaProxyThrowable: at SQLite.SQLiteConnection..ctor (System.String databasePath, SQLite.SQLiteOpenFlags openFlags, System.Boolean storeDateTimeAsTicks) [0x00077] in <d99df9bc3e8e44e69fa595e0813d407b>:0
at SQLite.SQLiteConnection..ctor (System.String databasePath, System.Boolean storeDateTimeAsTicks) [0x00000] in <d99df9bc3e8e44e69fa595e0813d407b>:0
at VorratsUebersicht.Android_Database.GetConnection () [0x00044] in <791ae3b0a24b4da0bb5d1adbd637d629>:0
at VorratsUebersicht.Database.GetArticleCount_Abgelaufen () [0x00006] in <791ae3b0a24b4da0bb5d1adbd637d629>:0
at VorratsUebersicht.MainActivity.ShowInfoText () [0x00001] in <791ae3b0a24b4da0bb5d1adbd637d629>:0
at VorratsUebersicht.MainActivity.OnCreate (Android.OS.Bundle bundle) [0x000a8] in <791ae3b0a24b4da0bb5d1adbd637d629>:0
at Android.App.Activity.n_OnCreate_Landroid_os_Bundle_ (System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_savedInstanceState) [0x0000f] in <1f978e077a40491b9118490f4344ce9f>:0
at (wrapper dynamic-method) System.Object.c6c33e78-4e99-4924-9cc8-34e6129429ac(intptr,intptr,intptr)
   at md56c9fe683bd4750f69443fa5376e732f4.MainActivity.n_onCreate (Native Method)
   at md56c9fe683bd4750f69443fa5376e732f4.MainActivity.onCreate (MainActivity.java:32)
   at android.app.Activity.performCreate (Activity.java:6912)
   at android.app.Instrumentation.callActivityOnCreate (Instrumentation.java:1126)
   at android.app.ActivityThread.performLaunchActivity (ActivityThread.java:2877)
   at android.app.ActivityThread.handleLaunchActivity (ActivityThread.java:2985)
   at android.app.ActivityThread.-wrap14 (ActivityThread.java)
   at android.app.ActivityThread$H.handleMessage (ActivityThread.java:1635)
   at android.os.Handler.dispatchMessage (Handler.java:102)
   at android.os.Looper.loop (Looper.java:154)
   at android.app.ActivityThread.main (ActivityThread.java:6692)
   at java.lang.reflect.Method.invoke (Native Method)
   at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run (ZygoteInit.java:1468)
   at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:1358)



1 Bericht

Huawei MediaPad T2 10.0 pro (HWFDR), 2048MB RAM, Android 5.1
------------------------------------------------------------

java.lang.NullPointerException
md56c9fe683bd4750f69443fa5376e732f4.ArticleListActivity.n_onActivit

Exceptions:
java.lang.RuntimeException: 
  at android.app.ActivityThread.performResumeActivity (ActivityThread.java:3260)
  at android.app.ActivityThread.handleResumeActivity (ActivityThread.java:3291)
  at android.app.ActivityThread.handleLaunchActivity (ActivityThread.java:2540)
  at android.app.ActivityThread.handleRelaunchActivity (ActivityThread.java:4251)
  at android.app.ActivityThread.access$1300 (ActivityThread.java:165)
  at android.app.ActivityThread$H.handleMessage (ActivityThread.java:1397)
  at android.os.Handler.dispatchMessage (Handler.java:102)
  at android.os.Looper.loop (Looper.java:135)
  at android.app.ActivityThread.main (ActivityThread.java:5689)
  at java.lang.reflect.Method.invoke (Native Method)
  at java.lang.reflect.Method.invoke (Method.java:372)
  at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run (ZygoteInit.java:960)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:755)
Caused by: java.lang.RuntimeException: 
  at android.app.ActivityThread.deliverResults (ActivityThread.java:3863)
  at android.app.ActivityThread.performResumeActivity (ActivityThread.java:3246)
Caused by: java.lang.NullPointerException: 
  at android.widget.AbsListView.onRestoreInstanceState (AbsListView.java:1889)
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleListActivity.n_onActivityResult (Native Method)
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleListActivity.onActivityResult (ArticleListActivity.java:56)
  at android.app.Activity.dispatchActivityResult (Activity.java:6320)
  at android.app.ActivityThread.deliverResults (ActivityThread.java:3859)

Navigation Bar
http://www.c-sharpcorner.com/article/xamarin-android-create-left-drawer-layout/


Mehrere Lager ermöglichen.

Lagerbewegung in einer Tabelle speichern
