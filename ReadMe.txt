
Vorratsübersicht - Mindesthaltbarkeitsdatum von Vorräten überwachen
===================================================================

Mit dieser App können Sie Mindesthaltbarkeitsdatum der Lebensmittel in Ihren Vorräten überwachen.

Funktionsweise

Zuerst wird ein Artikel (einmalig) in "Artikelliste" mit seinen Angaben und ggf. Bild erfasst.
Danach kann im "Lagerbestand" den Artikel auswählen und die Menge mit Ablaufdatum eintragen werden.
Nähert sich das Ablaufdatum oder wird das Datum überschritten, so erscheint eine Warnung in der App.

Wird beim Artikel der EAN Code eingetragen (oder gescannt),
so kann man über "Artikel scannen" direkt den Lagerbestand oder die Artikelangaben bearbeiten.
Die Artikeldaten werden über den EAN Code nicht automatisch (z.B. über's Internet) ermittelt.

Für die Warnung vor dem Ablaufdatum kann pro Artikel anzahl Tage definieren werden.




Die Artikel können anhand vom EAN Code erfasst und gesucht werden.
Die Angaben zum Artikel müssen jedoch (einmalig) manuell gemacht werden, 
da sie nicht automatisch (aus dem Internet) geladen werden.

Zum Testen oder Kennenlernen der App kann man auf eine Testdatenbank umschalten.
Diese enthält schon einige Artikel (ist keine Werbung für die Produkte).

Die App ist mein privates Hobby-Projekt ist. Die Benutzung erfolgt auf eigene Gefahr.
Die App wird gewissenhaft entwickelt, dennoch kann ich für Schäden durch App keine Haftung übernehmen.


Fehler berichten:

Diese App befindet sich noch mitten in der Entwicklung, daher kann sie noch Fehlern enthalten.

Sollte Ihnen ein Fehler aufgefallen sein, bitte diesen genau beschreiben (wie kann er nachgestellt werden),
möglichst Screenshots erstellen und an die unten aufgeführte E-Mail Adresse versenden.


Bekannte Probleme und Fehler:

1. Die Sortierung der Artikel berücksichtig nicht die deutschen Umlaute 
Die eingesetzte SQLite Datenbank unterstützt diese Sortierung nicht.

2. Eine Popup Benachrichtigung findet nicht statt. 
Um zu überprüfen, ob das Mindesthaltbarkeitsdatum
überschritten wurde, muss die App gestartet werden.


Version 2.20 (CodeVersion 32)
==================================

Vor dem Hochladen ins Google Store: 

  Im MainActivity das Datum in

      MainActivity.preLaunchTestEndDay = new DateTime(2018, 9, 22);

  auf heutiges Datum setzen, damit die automatische Pre-Launch Prüfung
  bei Google Play nicht beim EAN Scan "gefangen bleibt".

Play Store Eintrag:

Backup und Restore jetzt zum Testen (auf eigene Verantwortung)
Bilder drehen jetzt möglich (korrektur).
Notizen in Artikelliste und Einkaufsliste.
Artikelliste: Filter auf Kategorie.
Suche nach Artikeln und Hersteller integriert.
Artikelstamm um Einkaufsmarkt, Lagerort, Mindestmenge und bevorzugte Menge erweitert.
Lagerbestand filtern (nur "abgelaufene")
Aus der Einkaufsliste direkt ins Lagerbestand.
Nach dem EAN Scan der Lagerbestand gleich im Edit-Modus.
Hersteller jetzt auch mit Autovervollständigung.

C063 - Backup und Restore

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
C022 - Default Sprache auf Englich gesetzt.
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
C013 - Open-Source-Lizencen werden jetzt als HTML angezeigt.

Version 1.23 (CodeVersion 6)
==================================
minSdkVersion="16" targetSdkVersion="16"

C012 - Open-Source-Lizencen können jetzt angezeigt werden.


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
