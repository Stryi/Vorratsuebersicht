
Vor dem Hochladen ins Google Play Store: 
----------------------------------------

  Im MainActivity das Datum in

      MainActivity.preLaunchTestEndDay = new DateTime(2018, 9, 22);

  auf heutiges Datum setzen, damit die automatische Pre-Launch Prüfung
  bei Google Play nicht beim EAN Scan "gefangen bleibt".

Version 4.75 (Code Version 91)
==============================

E028 - FIX: Von Einkaufsliste ins Lager mit "Unendlich haltbar".

Version 4.74 (Code Version 90)
==============================

C150 - Menge bei "Auf Einkaufszettel" ist jetzt markiert und die Taststur eingeblendet.

Version 4.73 (Code Version 89)
==============================

C149 - EAN Scan: Bei "Auf Einkaufszettel" kann man jetzt die Menge eintragen.

Version 4.72 (Code Version 88)
==============================

C148 - EAN Scan: jetzt von Einkaufsliste ins Lager.
C147 - Standard Kategorie kann jetzt definiert werden.
E027 - FIX C144: MHD wurde immer abgefragt.

Version 4.71 (Code Version 87)
==============================

E026 - FIX: Korrektur Notizen Anzeige (war doppelt).

Version 4.70 (Code Version 86)
==============================

C146 - Sucht auf OpenFoodFacts.org bei mehreren EAN Codes jetzt möglich. (Sucht nacheinander, bis der erste Artikel gefunden wurde.)
C145 - Artikel Notizen jetzt im Lagerbestand.
C144 - "Ins Lagerbstand..." übernimmt die Menge gliech ins Lagerbestand.

Version 4.62 (Code Version 85)
==============================

E025 - FIX: Neuanlage Artikel und "Auf Einkaufsliste" beim Speichern.

Version 4.61 (Code Version 84)
==============================

C143 - "..." Button bei AutoComplete zeigt jetzt alls Einträge.
E024 - Testdatenbank "Kartoffel Mehl" -> "Kartoffelmehl"
C142-Update - Liste wird jetzt aktualisiert.
E023 - FIX: Kategorie "Essen" in den Testdatenbank wird als "Hygiene" angezeigt.

Version 4.55 (Code Version 83)
==============================

C142 - Bei "Auf Einkaufsliste" kann man jetzt die Zahl direkt eingeben.
C141 - Doppelte Einträge in Filtern und Auswahldialogen (wegen Leerzeichen) können jetzt durch "Datenbank Komprimieren" bereinigt werden.
E022 - FIX: Auswahl "..." überschreibt jetzt die Einträge, statt sie hinzuzufügen.
C140 - Einkaufszettel: Liste jetzt Zettel sortiert nach Einkaufsmarkt und (neu) Kategorie.

Version 4.54 (Code Version 82)
==============================

C139 - Artikelstamm: Frage, ob EAN Code hinzugefügt oder ersetzt werden soll.
E021 - FIX: EAN Scannen von Artikeln mit mehreren EAN Codes funktioniert jetzt auch.

Version 4.53 (Code Version 81)
==============================

E020 - FIX: Artikel mit mehreren EAN Codes.

Version 4.52 (Code Version 80)
==============================

C138 - Bilder komprimieren ausschaltbar
C137 - Intern: Backup Pfad wird restauriert, wenn gelöscht.

Version 4.51 (Code Version 79)
==============================

C136 - Backup Pfad jetzt anpassbar. (Impl. von 'wolpos').
C135 - Im Barcode Scann Modus bleiben. (Impl. von 'wolpos').

Version 4.50 (Code Version 78)
==============================

C134 - Alternativer Datumsauswahl-Dialog für Lagerbestand (unter Einstellungen zu aktivieren). (Impl. von 'wolpos').
E019 - FIX: Aus Lagerbestand auf Einkaufsliste (Fehler laut Absturzbericht vom Google)


Version 4.45 (Code Version 77)
==============================

C133 - Vorraete.db3 wird jetzt nicht zwingend angelegt (wenn sie umbenannt wurde).
C132 - LOG Datei wird jetzt geschrieben und kann an den Entwickler geschickt werden.

Version 4.44 (Code Version 76)
==============================

C132 - Datenbank aus dem App-verzeichnis kann jetzt auf Internen Speicher kopiert werden (Datenrettung)
C131 - Kategorien werden jetzt sortiert angezeigt.

Version 4.43 (CodeVersion 75)
=============================

C130 - Autovervollständigung jetzt über "..." abrufbar.
E018 - FIX: Artikelangaben: Auswahllisten für Unterkategorie und "Standard Lagerort" zeigen jetzt sortiert alle Einträge (nicht nur von Artilen mit Lagerbestand).
E017 - FIX: Artikelangaben: Texte bei Autovervollständigung werden jetzt ohne Leerzeichen am Ende abgespeichert.
E016 - FIX: Summe der Lagerbestandsliste bei Warnungen und Abgelaufen jetzt auch mit Komma-Stellen.

Version 4.42 (CodeVersion 74)
=============================

C129 - Datenbank im Applikationsverzeichnis wird als letztes Fallback verwendet.
E015 - FIX: Anzahl beim leeren Lager in der Position (NULL und '' Problematik)

Version 4.41 (CodeVersion 73)
=============================

E014 - FIX: Absturz beim Erfassen der Lagermengen, wenn kein Lagername im Artikelstamm

Version 4.40 (CodeVersion 72)
=============================

C128 - Keine "Zwischendatenbank" im Applikationsverzeichnis mehr (Es gab immer wieder Fehler beim Kopieren auf die SD Karte)
E013 - FIX: Anzahl "Warnungen" auf Startseite berücksichtigt nicht mehr "Unendlich Haltbar".
E012 - FIX: Absicherung vom Absturz beim Hinzufügen vom Artikel ins Lager

Version 4.32 (CodeVersion 71)
=============================

E011 - FIX: Neuanlage der Lagerposition 2x bearbeiten verdoppelt die Position.
E011 - FIX: Lager-Filter in der Bestandsliste berücksichtigt jetzt auch das Lager der Positionen.

Version 4.31 (CodeVersion 70) - Google Play Beta 11.03.2020
=============================

C127 - Alle Angaben vom Lagerbestand können jetzt geändert werden.
C126 - CSV Export der Artikel und Lagerbestände (in Einstellungen)

Version 4.30 (CodeVersion 69)
=============================

C125 - Lagername jetzt auch pro Lagerposition definierbar.

Version 4.26 (CodeVersion 68)      - Google Play Beta am 29.02.2020
==================================

E010 - Eingabe 'Anzahl' im Einkaufszettel erlaub jetzt nur Zahlen.

Version 4.25 (CodeVersion 67)      - Google Play Beta am 29.02.2020
==================================

E009 - Umrechnung Kalorien und Menge jetzt auch bei Großbuchstaben möglich.
C124 - Menge auf dem Einkaufszettel kann jetzt direkt eingegeben werden.
C123 - Hinweis auf mögliche Zusatzkosten beim Zugriff auf OpenFoodFacts.org kann jetzt unterdrückt werden.
C122 - Lagerliste: Sortier-Icon jetzt mit Kalender oder Buchstabe.
E008 - 'Auf Einkaufszettel' Icon jetzt kleiner.

Version 4.24 (CodeVersion 66)
==================================

C121 - Textgröße passt sich jetzt an die Systemeinstellungen an.
E007 - FIX: Notizen überdecken jetzt nicht die Icons in der Artikelliste.
E006 - FIX: Icon in der Artikelliste nach Lagerbestand wird jetzt aktualisiert.
E005 - FIX: Anzahl "Abgelaufen" auf Startseite berücksichtigt nicht mehr "Unendlich Haltbar".

Version 4.23 (CodeVersion 65)
==================================

C120 - Alle Einträge in Subkategorie, Einkaufsmarkt und Lagerort werden jetzt beim Anklicken angezeigt (nicht erst beim ersten Buchstaben).
C119 - Design der Masken etwas verbessert.
C118 - Klick auf Icon zeit jetzt gleich das Bild in Vollansicht.
C117 - Volltextsuche erweitern um Lagerort, Kategorie und Unterkategorie.  

Version 4.22 (CodeVersion 64)      - Produktiv genommen am: 17.02.2020
==================================

C116 - Wenn Anzahl oder Größe nicht 1 ist, so wird die Menge im Lagerbestandsliste berechnet angezeigt (z.B. Anzahl 3, Größe 0,5, Menge 1,6)
C115 - "Lagerbestand" Icon mit Menge bei Artikeln, die im Lager vorhanden sind.

Version 4.21 (CodeVersion 63)
==================================

C114 - "Einkaufswagen" Icon mit Menge bei Artikeln, die bereits auf Einkaufszettel stehen.

Version 4.20 (CodeVersion 62)
==================================

C113 - Datum im Lagerbestand kann jetzt geändert werden.

Version 4.16 (CodeVersion 61)
==================================

C112 - Zusätzliches Kontextmenü 'Als Gekauft markieren' in der Einkaufsliste.

Version 4.15 (CodeVersion 60)
==================================

C111 - Zusätzlicher Datenbankpfad wird jetzt geprüft.
C110 - 'Zu zahlen' in der Statuszeile vom Einkaufszettel

Version 4.14 (CodeVersion 59)
==================================

C109 - Artikel löschen jetzt auch möglich, wenn auf Einkaufsliste (kommt Warnung).

Version 4.13 (CodeVersion 58)
==================================

C108 - Lagerbestand jetzt auch mit 0.5 und 0.25 Schritten.

Version 4.12 (CodeVersion 57)
==================================

C107 - Artikel und Lagerliste kann jetzt auch als Text exportiert werden (Sharen).
C106 - Legerbestansliste wird jetzt nach Ablaufdatum sortiert (kein Filter mehr).

Version 4.11 (CodeVersion 56)
==================================

C105 - BugFix: Absturz beim Klicken auf Foto im Lagerbestand

Version 4.10 (CodeVersion 55)
==================================

C104 - Zusätzlicher benutzerdefinierter Pfad für Datenbanken.
C103 - "Gekauft" Kennzeichen für Einkaufszettel.

Version 4.01 (CodeVersion 54)
==================================

C102 - Preis und Kalorie wird in den Listen jetzt immer angezeigt (für bessere Übersicht).
C102 - Bessere Fehlerbehandlung beim Starten der App.

Version 4.00 (CodeVersion 53)
==================================

C101 - Jetzt wieder: Zusätzliche Informationen in der Statuszeile der Kisten.
C100 - Performance optimierung: Zusätzliche Tabelle für Bilder erstellt.

Version 3.15 (CodeVersion 52)
==================================

C099 - Jetzt werden vorzugsweise die deutschen Namen von openfoodfacts.org genommen.

Version 3.14 (CodeVersion 50, 51)
==================================

C098 - Lagerbestand jetzt auch mit 0.01 und 0.1 Schritten.

Version 3.13 (CodeVersion 49)
==================================

C097 - Suche jetzt auch in Notizen und Einkaufsmarkt.
C096 - Im Artikelstamm wird jetzt der Lagerbestand angezeigt.

Version 3.12 (CodeVersion 48)
==================================

C095 - Alternative Datenbank in ActionBar und Backup anzeigen.

Version 3.11 (CodeVersion 47)
==================================

C094 - Auswahl der Datenbanken beim Starten der App.
C093 - Statistik fasst jetzt 'ml' und 'cl' als 'l' und 'g' als 'kg' zusammen.

Version 3.10 (CodeVersion 46)
==================================

C092 - Bei Neuanlage einer Lagerposition werden nur noch die nicht hinzugefügten Artikel angezeigt.
C091 - In der Einkaufsliste gleich zum Artikeldetail.
C090 - Sprünge zwischen Artikelstamm und Lagerbestand über Contextmenü.

Version 3.01 (CodeVersion 45)
==================================

C089 - FIX: Benutzerdefinierte Kategorien mit Leerzeichen vorne.
C088 - Erneuerung der Datenbank Zugriffskomponenten.

Version 3.00 (CodeVersion 44)
==================================

C087 - Sucha nach Artikeldaten jetzt anhand vom openfoodfacts.org

Version 2.43 (CodeVersion 43)
==================================

C086 - Laden der Artikelliste und Lagerbestand optimiert (leider zugunsten von weniger Angaben).
C085 - Datenbank Reparatur/Check integriert

Version 2.42 (CodeVersion 42)
==================================

C084 - Kategorie und Unterkategorie wird jetzt zusammen in der Artikelliste angezeigt (platzsparender).
C083 - Fehler beim Zugriff auf Download Verzeichnis wird jetzt angezeigt.
C082 - Kalorien und Lager wird jetzt in der Artikelliste angezeigt.
C081 - In der Lagerliste wird jetzt auch die Anzahl summiert.
C080 - Aktualisierung der Komponenten (erhoffte Verbesserung beim EAN Scan).

Version 2.41 (CodeVersion 41)
==================================

C079 - Beim Erfassen der Mänge sind jetzt 1, 10 und 100-e Schritte möglich.

Version 2.40 (CodeVersion 40)
==================================

C078 - Einkaufszettel als TXT exportieren (Sharen)

Version 2.36 (CodeVersion 39)
==================================

C077 - Kategoriesn Speichern beim Umschalten der Datenbanken.

Version 2.34 (CodeVersion 38)
==================================

C076 - Datenverlust erkennen und beseitigen
C075 - Selbst definierte Kategorien (in Einstellungen).
C074 - FIX: Absturz beim Backup.

Version 2.33 (CodeVersion 37)
==================================

C073 - Aufruf für die EAN-Suche entfernt.

Version 2.32 (CodeVersion 36)
==================================

C072 - FIX: Fehler beim Speichern von Preisen mit Komma beseitigt.

Version 2.31 (CodeVersion 35)
==================================

C071 - Umrechnung 100 mg und 100 ml in Gesamtkalorien
C070 - Aufruf für die EAN-Suche
C069 - Artikelvorauswahl jetzt auch mit Unterkategorie


Version 2.30 (CodeVersion 34)
==================================

E-Mail an Beta Tester.

Betreff: Vorratsübersicht: Preis, Statuszeile und Einkaufszettel

Geschätzten Beta Tester,

ich war nach langer Zeit mal wieder fleißig. ;-) 
Hier die Neuerungen, die es zum Testen und begutachten gibt:

- (Standard) Preis zum Artikel kann jetzt erfasst werden.
- Einkaufszettel jetzt mit Auswahl des Einkaufsmarktes und Statuszeile.
- Statuszeile bei Lager- und Artikelliste mit (hoffentlich) nützlichen Informationen.

Viel Spaß mit dem Testen und bitte meldet Euch,
wenn was nicht funktioniert, falsch oder unlogisch ist.

Falls Ihr nicht mehr Beta Tester Seit, bitte g'schwind um E-Mail,
dann bekomme Ihr die Benachrichtigungen nicht mehr.

Gruß
Christian Stryi




Play Store Eintrag:

(Standard-) Preis zum Artikel kann jetzt erfasst werden.
Einkaufszettel jetzt mit Auswahl des Einkaufsmarktes.
Statuszeile bei Lager- und Artikelliste mit (hoffentlich) nützlichen Einformionen.
Backup und Restore jetzt zum Testen (auf eigene Verantwortung)
Problem beim erstmaligen Zugriff auf die SD Karte jetzt (hoffentlich) gelöst.
Beschriftung der Buttons jetzt nicht mehr in Großbuchstaben.


C068 - Preis zum Artikel und in der Statistik.
C067 - Lagerliste und Artikelliste: Summenzeile in der Artikel- und Lagerliste
C066 - Einkaufszettel: Popup Auswahl und Sortierung nach Einkaufsmarkt

Version 2.21 (CodeVersion 33)
==================================

C065 - Icon und Meldungen vom Backup/Restore überrbeitet.

Version 2.20 (CodeVersion 32)
==================================

C064 - Backup und Restore
C063 - Zugriff auf SD Karte nach der Installation

Version 2.16 (CodeVersion 31)
==================================

C062 - Zurück Taste speichert jetzt auch das gedrehte Bild.

Version 2.15 (CodeVersion 30)
==================================

C061 - Bilder drehen jetzt möglich
C060 - Meldung beim erstmaligen Starten der Anwendung jetzt verständlicher.

Version 2.14 (CodeVersion 29)
==================================

C059 - Deutlicher Hinweis auf den Testbetrieb.

Version 2.13 (CodeVersion 28)
==================================

C058 - Notizen in Artikelliste und Einkaufsliste.
C057 - Artikelliste: Filter auf Kategorie.
C056 - Suche nach Umlauten in Artikel-, Einkaufs- und Lagerliste funktioniert jetzt.
C055 - Pre-Launch Tests "austricksen"
C054 - NullReferenzException in Artikel- und Lagerliste vorbeugen


Version 2.12 (CodeVersion 27)
==================================

Play Store Eintrag:

Suche nach Artikeln und Hersteller integriert.
Artikelstamm um Einkaufsmarkt, Lagerort, Mindestmenge und bevorzugte Menge erweitert.
Lagerbestand filtern (nur "abgelaufene")
Aus der Einkaufsliste direkt ins Lagerbestand.
Nach dem EAN Scan der Lagerbestand gleich im Edit-Modus.
Hersteller jetzt auch mit Autovervollständigung.
Spracheingabe für Artikelname (zum Testen).
Absicherung gegen OutOfMemory Abstürze bei Bildern.


EAN Scan am 29.08.2018 aktivieren.

Version 2.11 (CodeVersion 26)
==================================

C053 - FIX: Nach dem Filtern und bearbeiten ist der Filter ohne Wirkung
C052 - Hersteller jetzt auch mit Autovervollständigung.
C051 - Kein Text unter "Artikelname", dann Tastatur einblenden.
C050 - FIX: "Artikel löschen" -> "Nein" beendet den Dialog nicht mehr.
C047 - Aus der Einkaufsliste direkt ins Lagerbestand
C046 - Absicherung gegen OutOfMemory Abstürze bei Bildern
C045 - Nach dem EAN Scan der Lagerbestand gleich im Edit-Modus.
C044 - Suche nach Artikeln um Hersteller erweitert

Version 2.10 (CodeVersion 25)
==================================

C043 - Suche nach Artikeln hinzugefügt
C042 - Artikelstamm um Einkaufsmarkt, Lagerort, Mindestmenge und bervorzugte Menge erweitert.
C041 - Spracheingabe für Artikelname (zum Testen)
C040 - Lagerbestand filtern (nur "abgelaufene")

Version 2.01 (CodeVersion 24) -> Full rollout im Play Store am 15.06.2018
==================================

C038 - Bildaufnahmen mit Android 6 und höher sollte jetzt wieder funktionieren.
C037 - Aus Artikeldetails auf die Einkaufsliste setzen.
C036 - Bessere Behandlung bei fehlerhaften Datenbankzugriff

Version 2.00 (CodeVersion 23)
==================================

C035 - Fehler beim Erfassen von vielen Mengen beseitigt.

Version 2.00 (CodeVersion 22)
==================================
minSdkVersion="16" targetSdkVersion="27"

C034 - Einkaufsliste

Version 1.45 (CodeVersion 21)
==================================
minSdkVersion="16" targetSdkVersion="27"

Google Play Version


Version 1.44 (CodeVersion 20)
==================================
minSdkVersion="15" targetSdkVersion="27"

C033 - Hinweis, wie man Backup macht.

Version 1.44 (CodeVersion 17)
==================================
minSdkVersion="15"

C032 - Auswahl Unterkategorie jetzt unabhängig der Kategorie.
C031 - "Zurück" Pfeil in der Titelleiste

Version 1.43 (CodeVersion 16)
==================================
minSdkVersion="15" targetSdkVersion="27"

C030 - Fehler beim initialen Datenbankzugriff wird jetzt angezeigt
C029 - Löschen (und Neuerstellung) der Datenbank jetzt im Settings möglich

Version 1.42 (CodeVersion 15)
==================================
minSdkVersion="15" targetSdkVersion="27"

C028 - Datums Problem bei einigen Smartphones behoben
C027 - Nach dem EAN-Scann kommt eine Auswahl: Lagerbestand und Artikeldaten 
C026 - Testdaten korrigiert (Kategorie "Lebensmittel" statt "Essen" und Größe/Einheit)
C025 - Auswahl "Alle" oder "Ohne" Unterkategorie
C024 - Fehler beim Foto Aufnehmen bei Android 7 und 8 wird abgefangen (noch keine Lösung!)
C023 - "Warnen: X Tage(n) vor Ablauf" Anzeige bei Lagerbestand wird bei "DurableInfinity=True" nicht angezeigt.
C022 - Default Sprache auf Englisch gesetzt.
C021 - Klick auf ein leeres Bild beim Lagerbestand bringt keine (fast) leere Seite mehr.

Version 1.41 (CodeVersion 14)
==================================
minSdkVersion="15" targetSdkVersion="27"

C020 - Performance beim Laden der Listen weiter verbessert.

Version 1.40 (CodeVersion 13)
==================================
minSdkVersion="16" targetSdkVersion="24"

C019 - Performance beim Laden der Listen verbessert.
C018 - Problem mit Bild erstellen und fehlender SD Karte behoben.
C017 - Farbschema überarbeitet

Version 1.31 (CodeVersion 10)
==================================
minSdkVersion="16" targetSdkVersion="24"

C016 - Eingabe von Zahlen vor Absturz abgesichert.

Version 1.30 (CodeVersion 9)
==================================
minSdkVersion="16" targetSdkVersion="16"

C015 - Eingabe von "Inhalt / Größe" vor Absturz abgesichert (richtig).


Version 1.25 (CodeVersion 8)
==================================
minSdkVersion="16" targetSdkVersion="16"

C016 - Einstellungen in eine separate Seite vershoben.
C015 - Eingabe von "Inhalt / Größe" vor Absturz abgesichert.


Version 1.24 (CodeVersion 7)
==================================
minSdkVersion="16" targetSdkVersion="16"

C014 - Teilweise englische Übersetzung
C013 - Open-Source-Lizenzen werden jetzt als HTML angezeigt.

Version 1.23 (CodeVersion 6)
==================================
minSdkVersion="16" targetSdkVersion="16"

C012 - Open-Source-Lizenzen können jetzt angezeigt werden.


Version 1.22 (CodeVersion 5)
==================================
minSdkVersion="16" targetSdkVersion="16"

Erweiterungen:

C008 - EAN Code kann jetzt doppelt vergeben werden (z.B. für 22120649).
C009 - Wald-Heidelbeeren in der Testdatenbank hat jetzt das richtige Bild.
C010 - Bei Neuanlage Artikel kommen jetzt zwei Icons: "Bild auswähle" und "Foto machen" (Vereinfachung)
C011 - Anzahl Kalorien kann jetzt pro Artikel erfasst werden.

Version 1.21 Alpha (CodeVersion 4)
==================================

Erweiterungen:

C001 - Die Datenbank wird jetzt vorzugsweise auf der SD Karte im Verzeichnis "Vorratsuebersicht" angelegt.
C002 - Beim Komprimieren der Bilder wird jetzt eine Fortschrittsanzeige angezeigt.
C003 - Der Hinweis auf die Testversion kommt jetzt nur einmal am Tag.
C004 - In der Mengenliste wird jetzt 'Anzahl' und nicht 'Menge' als Text angezeigt.
C005 - Text "Warnen in X Tage(n)" geändert in "Warnen X Tage(n) vor Ablauf".
C006 - Menge kann jetzt mit Komma oder Punkt eingegeben werden.
C007 - Klick auf die Anzahl "Demnächst zu verbrauchen:" zeigt jetzt die Liste mit den Artikeln

Behobene Fehler:

E002 - Nach der Installation wird das Ablaufdatum der Artikel in der Testdatenbank aktualisiert.


Bekannte Fehler:

E003 - Das Umschalten auf die Testdatenbank funktioniert nicht immer richtig.
Ist die Testdatenbank aktiv und die App im Hintergrund, so wird beim Aktivieren der App
die Testdatenbank zwar wieder aktiviert, aber der "Schalter" für die Testdatenbank steht auf "0".

E004 - Auf Samsung S3 Mini wird das Hinzufügen von Positionen nicht angezeigt.



Version 1.20 Alpha - Google Play (CodeVersion 2)
================================================

Behobene Fehler:

E001 - Absturz beim Starten behoben.
