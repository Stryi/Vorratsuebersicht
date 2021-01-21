Prio 1
======

Anwendung
- Anwendungsabstürze in die LOG Datei protokollieren.
  https://stackoverflow.com/questions/39503390/global-exception-handling-in-xamarin-cross-platform

Artikelbestand
- Lagerumbuchung komfortabler machen (s. E-Mail vom C. A. Betz). (noch offen, wie)

Einstellungen
- Automatischen Backup ("Alle X Tage...", "Y Kopien behalten")


Artikeldetails
- Kalorien umrechnen auch von "cl"
- Bild Komprimierung an die Auflösung des Smartphones anpassen


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



Vorschläge vom Professor Knox
-----------------------------

Prio eher hoch:
 
- Bild bearbeiten erweitern
  - "Crop"
  - Foto löschen
 
- Einscannen des Lagerorts als Barcode
  - man druckt sich eigene Barcode-Etiketten aus und deponiert/klebt sie an die entsprechenden Lagerorte
 
 
Prio eher mittel:
 
- Komfortableres Ausbuchen: Eigener Button für Ausbuchen von Artikeln
  -  Scannen des Artikel-Barcodes
  -  Auswahlliste der vorhandenen Lagerorte für diesen Artikel -> Lagerort antippen, oder scannen des Lagerort-Barcodes (s.o.)
  -  Liste der Haltbarkeitsdaten, falls an diesem Lagerort mehrere Haltbarkeitsdaten vorhanden sind -> Haltbarkeitsdatum antippen
  -  Abfrage der Anzahl (Dropdown-Liste / Spinner) mit default 1
  -  Bestätigung der Anzahl
  -> Entsprechender Artikel wird ausgebucht
 
- Komfortableres Einbuchen: Button für Einbuchen von Artikeln
  -  Scannen des Artikel-Barcodes
  -  Auswahlliste der bereits gespeicherten Lagerorte für diesen Artikel [mit Element [neuer Lagerort], Lagerort antippen, neu eingeben oder Scannen des Lagerort-Barcodes (s.o.)
  -  Abfrage des Haltbarkeitsdatums -> Eingeben des Haltbarkeitsdatums
  -  Abfrage der Anzahl (Dropdown-Liste / Spinner) mit default 1
  -  Bestätigung der Anzahl
  -> Entsprechender Artikel wird eingebucht
 
- VPE (Verpackungseinheiten (stk) gesondert zur Mengenangabe (z.B. ltr.)
  -  Lebensmittel werden z.B. in Gramm verkauft und enthalten n Stück/Portionen, z.B. 4 Muffins / 2 Portionen (Fertiggericht)
  -> ermöglicht Angabe Kalorien / 100g und Kalorien / Portion
 
 
Prio eher niedrig:
 
- Verwaltung von anderen Artikeln als Lebensmittel, z.B. Reinigungsmittel
  -> in diesem Fall wird der Barcode natürlich nicht auf openfoodfacts gefunden 
   -> Zugriff auf eine andere, allgemeine EAN-Code-Datenbank und Auslesen Artikelname/Foto/Mengeneinheit
 
- Scannen des Haltbarkeitsdatum, ggf. Foto mit manuellem Markieren des Datums durch Aufziehen eines Rahmens (beo Konserven in der Regel nur Jahreszahl oder Monat/Jahr vorhanden)
 
 