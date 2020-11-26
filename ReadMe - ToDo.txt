Prio 1
======

Anwendung
- Anwendungsabstürze in die LOG Datei protokollieren.
  https://stackoverflow.com/questions/39503390/global-exception-handling-in-xamarin-cross-platform

Artikelbestand
- Lagerumbuchung komfortabler machen (s. E-Mail vom C. A. Betz). (noch offen, wie)


Artikeldetails
- Kalorien umrechnen auch von "cl"


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


Debugger Information prüfen

Warnung

Deine App ist derzeit auf API-Ebene 28 ausgerichtet, sollte jedoch eine API-Mindestebene von 29 haben.
Hierdurch soll dafür gesorgt werden, dass deine App die aktuellen APIs nutzt, Nutzern eine optimale Leistung bietet und sicher ist.

Ab August 2020 müssen neue Apps auf mindestens Android 10 (API-Ebene 29) ausgerichtet sein.
Ab November 2020 müssen App-Updates auf mindestens Android 10 (API-Ebene 29) ausgerichtet sein.

Warnung
Mit diesem APK werden ungenutzter Code und ungenutzte Ressourcen an Nutzer gesendet.
Deine App könnte kleiner sein, wenn du das Android App Bundle verwendet hättest.
Wenn du deine App nicht für Gerätekonfigurationen optimierst,
ist deine App größer und braucht länger zum Herunterladen und Installieren auf den Geräten der Nutzer.
Größere Apps haben geringere Installations-Erfolgsraten und belegen mehr Speicher auf den Geräten der Nutzer.

Warnung
Dieses APK enthält Java- oder Kotlin-Code, der möglicherweise verschleiert ist.
Wir empfehlen dir, eine Offenlegungsdatei hochzuladen, damit deine Abstürze und ANRs
besser analysiert und von Fehlern bereinigt werden können. Weitere Informationen: https://developer.android.com/studio/build/shrink-code#decode-stack-trace

Warnung
Dieses APK enthält nativen Code und du hast keine Symbole zum Debuggen hochgeladen.
Wir empfehlen dir, eine Symboldatei hochzuladen, damit deine Abstürze und ANRs besser analysiert
und von Fehlern bereinigt werden können. Weitere Informationen: https://developer.android.com/studio/build/shrink-code#native-crash-support


Prio 2
======

Einkaufszettel
- Liste automatisch anhand der Mindestmenge erstellen.

Einkaufsliste
- Sortierung: Alphabetisch, Kategorie und Gekauft.

Lagerbestand
- Wird ein Datum < Heute eingegeben, so wird die Farbe nicht auf blau oder rot gesetzt.

Artikelstamm
- Unter-Unterkategorie (Rezension vom Jessica K. am 24.04.2020 um 15:37)
- Zweiter Preis (Angebot). (vom Scare C. per E-Mail) 
- Default Werte vorbelegen (Martin T aus CH)
- Mehrere Bilder Pro Artikel
- Beim Umbenennen vom "Standard Lagerort" fragen, ob das im Lager auch umbenannt werden sollte.

Einstellungen
- Automatischen Backup ("Alle X Tage...", "Y Kopien behalten")
- Freie Sortierung der Kategorien. (M. Pleuger)

Lagerliste
- Zusammenfügen von Unterkategorien bei der Mindestmenge. 
  So das z.B die Unterkategorie Kaffee auf 5 Pack steht.
  Und nicht Hersteller A ein Pack, Hersteller B zwei Pack (vom Markus Neese vorgeschlagen)
- Auswahl Lagerort auch an Subkategorie berücksichtigen

Sonstiges
- CSV Import (vom Scare C. per E-Mail) 
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


Gemeldete Abstürze (die ich mir ansehen sollte):
================================================

android.runtime.JavaProxyThrowable: at System.Net.WebConnectionStream.get_Length () [0x00000] in <468651912ca0498082abdfc00964fc02>:0
at VorratsUebersicht.InternetDatabaseSearchActivity.GetUrlPicture (System.String imageUrl) [0x00035] in <2112193acc53469b9ebbb2c441705d52>:0
at VorratsUebersicht.InternetDatabaseSearchActivity+<>c__DisplayClass8_0.<SearchAndShowArticle>b__0 () [0x003ef] in <2112193acc53469b9ebbb2c441705d52>:0
at Java.Lang.Thread+RunnableImplementor.Run () [0x00008] in <2df86f44c8e946618f31cb3aed952f5b>:0
at Java.Lang.IRunnableInvoker.n_Run (System.IntPtr jnienv, System.IntPtr native__this) [0x00008] in <2df86f44c8e946618f31cb3aed952f5b>:0
at (wrapper dynamic-method) Android.Runtime.DynamicMethodNameCounter.48(intptr,intptr)
  at mono.java.lang.RunnableImplementor.n_run (Native Method)
  at mono.java.lang.RunnableImplementor.run (RunnableImplementor.java:30)
  at android.os.Handler.handleCallback (Handler.java:873)
  at android.os.Handler.dispatchMessage (Handler.java:99)
  at android.os.Looper.loop (Looper.java:214)
  at android.app.ActivityThread.main (ActivityThread.java:7050)
  at java.lang.reflect.Method.invoke (Native Method)
  at com.android.internal.os.RuntimeInit$MethodAndArgsCaller.run (RuntimeInit.java:494)
  at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:965)

Huawei Mate 20 Pro (HWLYA), 5632MB RAM, Android 10
Bericht 1
android.runtime.JavaProxyThrowable: at System.Linq.Enumerable.First[TSource] (System.Collections.Generic.IEnumerable`1[T] source) [0x00010] in <715c2ff6913942e6aa8535593b3ef35a>:0
at VorratsUebersicht.Database.GetArticleData (System.Int32 articleId) [0x00078] in <8f65cfdb5fac4bad9251caa1b2de7fec>:0
at VorratsUebersicht.Database.GetToShoppingListQuantity (System.Int32 articleId) [0x00001] in <8f65cfdb5fac4bad9251caa1b2de7fec>:0
at VorratsUebersicht.StorageItemQuantityActivity.AddToShoppingList () [0x0000e] in <8f65cfdb5fac4bad9251caa1b2de7fec>:0
at VorratsUebersicht.StorageItemQuantityActivity.OnOptionsItemSelected (Android.Views.IMenuItem item) [0x0006e] in <8f65cfdb5fac4bad9251caa1b2de7fec>:0
at Android.App.Activity.n_OnOptionsItemSelected_Landroid_view_MenuItem_ (System.IntPtr jnienv, System.IntPtr native__this, System.IntPtr native_item) [0x00011] in <b781ed64f1d743e7881ac038e0fbdf85>:0
at (wrapper dynamic-method) System.Object.7(intptr,intptr,intptr)
at md56c9fe683bd4750f69443fa5376e732f4.StorageItemQuantityActivity.n_onOptionsItemSelected (Native Method)
at md56c9fe683bd4750f69443fa5376e732f4.StorageItemQuantityActivity.onOptionsItemSelected (StorageItemQuantityActivity.java:58)
at android.app.Activity.onMenuItemSelected (Activity.java:4271)
at com.android.internal.policy.PhoneWindow.onMenuItemSelected (PhoneWindow.java:1339)
at com.android.internal.view.menu.MenuBuilder.dispatchMenuItemSelected (MenuBuilder.java:787)
at com.android.internal.view.menu.MenuItemImpl.invoke (MenuItemImpl.java:164)
at com.android.internal.view.menu.MenuBuilder.performItemAction (MenuBuilder.java:934)
at com.android.internal.view.menu.MenuBuilder.performItemAction (MenuBuilder.java:924)
at android.widget.ActionMenuView.invokeItem (ActionMenuView.java:626)
at com.android.internal.view.menu.ActionMenuItemView.onClick (ActionMenuItemView.java:148)
at android.view.View.performClick (View.java:7189)
at android.view.View.performClickInternal (View.java:7163)
at android.view.View.access$3500 (View.java:821)
at android.view.View$PerformClick.run (View.java:27579)
at android.os.Handler.handleCallback (Handler.java:888)
at android.os.Handler.dispatchMessage (Handler.java:100)
at android.os.Looper.loop (Looper.java:213)
at android.app.ActivityThread.main (ActivityThread.java:8147)
at java.lang.reflect.Method.invoke (Native Method)
at com.android.internal.os.RuntimeInit$MethodAndArgsCaller.run (RuntimeInit.java:513)
at com.android.internal.os.ZygoteInit.main (ZygoteInit.java:1101)
