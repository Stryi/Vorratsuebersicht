Prio 1
======

- In einem roten Kreis die Anzahl der Artikel anzeigen,
  bei denen die Mindestbestellmenge erreicht oder unterschritten wurde.

  Den Filter 'Für die Einkaufsliste (ohne Bestand)' Funktionalität
  erweitern um 'Für die Einkaufsliste (ohne Bestand oder Mindestmenge erreicht)'.

- Icon "Interner Speicher" und "SD Karte" bei Auswahl der Datenbanken.

- Verzeichnis für die Datenbank außerhalb der Anwendung
  https://developer.android.com/training/data-storage/shared/documents-files

- Kompakte Liste (nur Bilder)

- Datenbankanlage auch auf der SD Karte ermöglichen.
  Datenbank auf SD Karte verschieben?
  Beispiel: "/storage/0E0E-2316/Android/data/de.stryi.Vorratsuebersicht/files"

- Einscannen des Lagerorts als Barcode (vom Daniel am 11.12.2024)
  - man druckt sich eigene Barcode-Etiketten aus und deponiert/klebt sie an die entsprechenden Lagerorte.


Einstellungen
- Nur X Backups lassen, ältere automatisch löschen. (auch vom mik53ke per E-Mail)


Einkaufsliste
- Freie Sortierung (wie im Laden die Reihenfolge ist)  (vom Nernd)

Artikeldetails
- Preiseingabe auch mit ","

Sonstiges
- Push Benachrichtigung (z.B. einmal pro Tag) über bald zu verbrauchende Artikel 
  Gewünscht von Frank (E-Mail), und von Eva (?)
  https://docs.microsoft.com/de-de/xamarin/android/app-fundamentals/notifications/local-notifications-walkthrough
  https://docs.microsoft.com/de-de/xamarin/android/app-fundamentals/services/service-notifications



Suche
-  Kann nicht Ä und ä suchen.




Prio 2
======

Artikeldetails
- Kalorien umrechnen auch von "cl"


Artikelstamm
- Unter-Unterkategorie (Rezension vom Jessica K. am 24.04.2020 um 15:37)
- Zweiter Preis (Angebot). (vom Scare C. per E-Mail) 
- Default Werte vorbelegen (Martin T aus CH) => Verworfen
- Mehrere Bilder Pro Artikel
- Beim Umbenennen vom "Standard Lagerort" fragen, ob das im Lager auch umbenannt werden sollte.
- Verbrauch Stück pro X Tage (vom I. Landgraf per E-Mail) 
  Mein Vorschlag für die Umsetzung:
    Drei neue Parameter "Verbrauche X (Einheit=Y) pro Z Tage(n)".
    Also ich verbrauche X=1 Y=Stück Dose(n) pro Y=10 Tag(en).
    Somit bei X=10 Dosen * Y=10 Tage pro Dose ergibt 100 Tage.
    Problem: ml, g, kg, unterschiedliche MHD verbrauch 10 mg / Tag (z.B. Maggi)

Einstellungen
- Freie Sortierung der Kategorien. (M. Pleuger)

Lagerliste
- Zusammenfügen von Unterkategorien bei der Mindestmenge. 
  So das z.B die Unterkategorie Kaffee auf 5 Pack steht.
  Und nicht Hersteller A ein Pack, Hersteller B zwei Pack (vom Markus Neese vorgeschlagen)
- Auswahl Lagerort auch an Subkategorie berücksichtigen


Sonstiges
- CSV Import (vom Scare C. per E-Mail, I. Landgraf per E-Mail und mi53ke per E-Mail) 
- Spracheingabe, https://docs.microsoft.com/de-de/xamarin/android/platform/speech



Synchronisierung (Mehrbenutzermodus)
- https://syncthing.net
- https://github.com/rqlite/rqlite - ...distributed relational database, which uses SQLite
- http://litesync.io
- FolderSync

Prio 3
======

Von Rezension Soe Ungezähmt am 11.04.2020 um 20:58

    Was cool wäre, wäre eine Option für geöffnete Produkte bei deren Aktivierung
    das MHD/zu verbrauchen bis-Datum auf eine, je nach Produkt, 
    bestimmte Anzahl von Tagen reduziert wird. Ggf. nach Ablauf,
    wird das geöffnete Produkt dann automatisch aus der Datenbank entfernt. :)


Lagerbestand
- Anzahl gruppieren und Summieren nach Datum.
- Lagerbewegung in einer Tabelle speichern



Vorschläge vom Professor Knox
-----------------------------

Prio eher hoch:
 
- Bild bearbeiten erweitern
  - "Crop"
 
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
 
---------------------------------------------------
Von Google Informationen:

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

