
Prio 1
======

Prio 2
======

Allgemein
- System.Diagnostics.Trace.WriteLine, Console.WriteLine,  ersetzten
  durch TRACE(...) ersetzten.


Artikelliste
- Filter auf Kategorie


Einstellungen
- Automatischen Backup ("Alle X Tage...", "Y Kopien behalten")
- BUG: Backup Hinweis zeit die Test-Datenbank an


Bilder
- Ausrichtung der Bilder beachten (Quer fotografiert), ggf. "Bild drehen" Funktionalität implementieren.
- Nach dem Auswahl vom neuen Bild wird noch beim Anklicken das alte angezeigt.
- Bilder im Verzeichnis "Vorräte Bilder" anschließend löschen.
- Bild entfernen programmieren


Lagerliste
- Index auf Lager (Abfrage dauert lange)
- Letzte Zeile Summe "kcal", "Anzahl" und "Menge nach Einheit"


Artikeldetail
- Artikelangaben speichern dauert lange (wird auch Image immer wieder gespeichert?)
- Klick auf Text neben dem Bild soll auch das Bild zeigen.


Einkaufszettel
- Liste automatisch anhand der Mindestmenge erstellen.


Sonstiges
- Release Notes der App anzeigen
- Spracheingabe, https://docs.microsoft.com/de-de/xamarin/android/platform/speech
- Absturz protokollieren (LOG-Datei)


Prio 3
======

Bilder
- Komprimierung/Verkleinerung der Bilder nicht immer effizient

Lagerbestand / Artikelliste
- Bilder im Thread oder asynchron laden
  https://blog.xamarin.com/getting-started-with-async-await/


Kategorie/Unterkategorie
- Selbst definierte Kategorien
- "Keine Kategorie" als Popup Auswahl


Einkaufsliste:
- Aus der Lagerbestand Liste auf die Einkaufsliste setzen ("..." Menü?)


Lagerbestand
- Anzahl gruppieren und Summieren nach Datum.

Artikeldetails
- Mehrere Bilder Pro Artikel

Artikel scannen
- https://corporate.codecheck.info/produkte/produktdaten-api/


Prio 4
======

- Für Hinzufügen von Positionen, Artikel oder Bestand ein "Floating Action Button" verwenden
  https://guides.codepath.com/android/floating-action-buttons



Gemeldete Abstürze:
===================

New in version 24
java.lang.NullPointerException
in md56c9fe683bd4750f69443fa5376e732f4.ArticleListActivity.n_onActivityResult

Aug 19, 8:05 PM on app version 24
Samsung Galaxy S7 (herolte), Android 8.0
Report 1 of 5
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
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleListActivity.n_onActivityResult (Native Method)
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleListActivity.onActivityResult (ArticleListActivity.java:56)
  at android.app.Activity.dispatchActivityResult (Activity.java:7547)
  at android.app.ActivityThread.deliverResults (ActivityThread.java:4487)




26.07.2018, 11:27 in der App-Version 24
Samsung Galaxy S7 (herolte), Android 8.0
Bericht 1 von 1
android.runtime.JavaProxyThrowable: at VorratsUebersicht.ArticleDetailsActivity.SaveArticle () [0x001bd] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at VorratsUebersicht.ArticleDetailsActivity.OnOptionsItemSelected (Android.Views.IMenuItem item) [0x00059] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at Android.App.Activity.n_OnOptionsItemSelected_Landroid_view_MenuItem_ (System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_item) [0x0000f] in <263adecfa58f4c449f1ff56156d886fd>:0
at (wrapper dynamic-method) System.Object.91e80665-3b33-4e6c-8562-2b85f70d7126(intptr,intptr,intptr)
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleDetailsActivity.n_onOptionsItemSelected (Native Method)
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleDetailsActivity.onOptionsItemSelected (ArticleDetailsActivity.java:59)
  at android.app.Activity.onMenuItemSelected (Activity.java:3527)
  at com.android.internal.policy.PhoneWindow.onMenuItemSelected (PhoneWindow.java:1278)
  at com.android.internal.view.menu.MenuBuilder.dispatchMenuItemSelected (MenuBuilder.java:761)
  at com.android.internal.view.menu.MenuItemImpl.invoke (MenuItemImpl.java:172)
  at com.android.internal.view.menu.MenuBuilder.performItemAction (MenuBuilder.java:908)
  at com.android.internal.view.menu.MenuBuilder.performItemAction (MenuBuilder.java:898)
  at android.widget.ActionMenuView.invokeItem (ActionMenuView.java:741)
  at com.android.internal.view.menu.ActionMenuItemView.onClick (ActionMenuItemView.java:293)
  at android.view.View.performClick (View.java:6897)
  at android.widget.TextView.performClick (TextView.java:12693)
  at android.view.View$PerformClick.run (View.java:26101)
  at android.os.Handler.handleCallback (Handler.java:789)
  at android.os.Handler.dispatchMessage (Handler.java:98)
  at android.os.Looper.loop (Looper.java:164)
  at android.app.ActivityThread.main (ActivityThread.java:6944)
  at java.lang.reflect.Method.invoke (Native Method)
  at com.android.internal.os.Zygote$MethodAndArgsCaller.run (Zygote.java:327)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:1374)

22. Juli 23:08 in der App-Version 24
Samsung Galaxy Xcover4 (xcover4lte), Android 7.0

android.runtime.JavaProxyThrowable: at VorratsUebersicht.Database.GetCategories () [0x00055] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at VorratsUebersicht.MainActivity.ShowCategoriesSelection () [0x0000e] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at VorratsUebersicht.MainActivity.<OnCreate>b__11_0 (System.Object <p0>, System.EventArgs <p1>) [0x00001] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at Android.Views.View+IOnClickListenerImplementor.OnClick (Android.Views.View v) [0x00011] in <263adecfa58f4c449f1ff56156d886fd>:0
at Android.Views.View+IOnClickListenerInvoker.n_OnClick_Landroid_view_View_ (System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_v) [0x0000f] in <263adecfa58f4c449f1ff56156d886fd>:0
at (wrapper dynamic-method) System.Object.471bc43b-80b5-4bee-81c4-9b0e41d5ab1d(intptr,intptr,intptr)
  at mono.android.view.View_OnClickListenerImplementor.n_onClick (Native Method)
  at mono.android.view.View_OnClickListenerImplementor.onClick (View_OnClickListenerImplementor.java:30)
  at android.view.View.performClick (View.java:6257)
  at android.widget.TextView.performClick (TextView.java:11145)
  at android.view.View$PerformClick.run (View.java:23705)
  at android.os.Handler.handleCallback (Handler.java:751)
  at android.os.Handler.dispatchMessage (Handler.java:95)
  at android.os.Looper.loop (Looper.java:154)
  at android.app.ActivityThread.main (ActivityThread.java:6836)
  at java.lang.reflect.Method.invoke (Native Method)
  at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run (ZygoteInit.java:1520)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:1410)



22. Juli 15:16 in der App-Version 24
Samsung Galaxy Tab S2 (gts210vewifi), Android 7.0
Bericht 1 von 1
android.runtime.JavaProxyThrowable: at VorratsUebersicht.Database.GetSubcategoriesOf (System.String category) [0x00087] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at VorratsUebersicht.ArticleDetailsActivity.ShowPictureAndDetails (System.Int32 articleId, System.String eanCode) [0x00259] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at VorratsUebersicht.ArticleDetailsActivity.OnCreate (Android.OS.Bundle savedInstanceState) [0x0021d] in <81b7fdaf0dfc44ce9b6e4e2da45aa98f>:0
at Android.App.Activity.n_OnCreate_Landroid_os_Bundle_ (System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_savedInstanceState) [0x0000f] in <263adecfa58f4c449f1ff56156d886fd>:0
at (wrapper dynamic-method) System.Object.f7ba5654-bb22-44a4-ba7e-8ae042540e3a(intptr,intptr,intptr)
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleDetailsActivity.n_onCreate (Native Method)
  at md56c9fe683bd4750f69443fa5376e732f4.ArticleDetailsActivity.onCreate (ArticleDetailsActivity.java:35)
  at android.app.Activity.performCreate (Activity.java:6948)
  at android.app.Instrumentation.callActivityOnCreate (Instrumentation.java:1126)
  at android.app.ActivityThread.performLaunchActivity (ActivityThread.java:2924)
  at android.app.ActivityThread.handleLaunchActivity (ActivityThread.java:3042)
  at android.app.ActivityThread.-wrap14 (ActivityThread.java)
  at android.app.ActivityThread$H.handleMessage (ActivityThread.java:1639)
  at android.os.Handler.dispatchMessage (Handler.java:102)
  at android.os.Looper.loop (Looper.java:154)
  at android.app.ActivityThread.main (ActivityThread.java:6780)
  at java.lang.reflect.Method.invoke (Native Method)
  at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run (ZygoteInit.java:1496)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:1386)

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
