
Vorratsübersicht - Mindesthaltbarkeitsdatum von Vorräten überwachen
===================================================================

Mit dieser App können Sie Mindesthaltbarkeitsdatum der Lebensmittel in Ihrem Vorratsschrank überwachen.

Funktionsweise

Für jeden Artikel kann Menge und das Mindesthaltbarkeitsdatum erfasst werden.
Wird das Datum überschritten, so erscheint ein roter Hinweis.
Wird pro Artikel zusätzlich eine Anzahl Tage für die Warnung angegeben, 
so erfolgt schon vor dem Datum ein gelber Hinweis.

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
