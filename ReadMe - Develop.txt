------------------------------------------------------------------------
Projektname: Vorratsübersicht - Mindesthaltbarkeitsdatum der Vorräte überwachen.

Play Store  : https://play.google.com/store/apps/details?id=de.stryi.Vorratsuebersicht&hl=de
Beta Test   : https://play.google.com/apps/testing/de.stryi.Vorratsuebersicht

Web Seite   : https://sites.google.com/site/vorratsuebersicht

GitHub      : https://github.com/Stryi/Vorratsuebersicht
------------------------------------------------------------------------


Funktionsweise
--------------

    1 Datenbanken

    1.1 Die produktive Datenbank

    Die Datenbank für die erfassten Daten wird im internen Speicher angelegt als
    
        /data/data/de.stryi.Vorratsuebersicht/files/Vorraete.db3
        
    Ist eine SD Karte vorhanden oder wird eine nachträglich reingelegt,
    so wird ein Verzeichnis "Vorratsuebersicht" auf der SD Karte angelegt
    und die Datenbank aus dem internen Speicher dort kopiert als

        /storage/sdcard/Vorratsuebersicht/Vorraete.db3

    Es wird vorzugsweise die Datenbank aus der SD Karte verwendet, 
    damit, falls das Smartphone nicht mehr funktioniert, die Datenbank
    in ein neues Smartphone übernommen werden kann.


    1.2 Die Test Datenbank


    Die Test-Datenbank kann mit Beispieldaten oder als leere Datenbank
    immer wieder überschrieben werden. Die darin enthaltene Änderungen gehen dann verloren.


    2 Auswahl der Datenbank

    Die Anwendung prüft zuerst, ob die Datenbank im Hauptverzeichnis einer SD-Karte sich befindet.
    Falls dort keine Datenbank vorhanden ist, so wie die Datenbank im Anwendungsverzeichnis verwendet.

    3. EAN Code

    Für jeden Artikel kann ein EAN Code eingegeben werden.
    Ein EAN Code kann aber für mehrere Artikel vergeben werden (z.B. 22120649)

    3.1. EAN Code scannen

    Beim Scannen des EAN Codes wird nach dem Artikel gesucht.

    - Wird kein Artikel gefunden, so kann eine Neuanlage gemacht werden.
    - Wird ein Artikel eindeutig gefunden, so kann man gleich die Menge bearbeiten
    - Wird ein EAN Code bei mehreren Artikeln gefunden,
      so erscheint eine Liste der Artikel mit diesem EAN Code.


Bekannte Fehler:
================

    Die Sortierung funktioniert nicht für deutsche Umlaute.


Fehler Berichte:
================

    Bitte genau beschreiben, wann der Fehler auftritt, d.h. wie kann man den Fehler nachvollziehen.
    Bilder von dem Fehler sind immer hilfsreich und sind gewünscht.


Geplante Funktionalitäten
=========================

    - Einkaufsliste

    - Auswertung des Lagers (Gesamt Gewicht, Gesamt Kalorien, Liste der Artikel,...)

    - Backup, Restore

    - Lagerort Verwaltung

    - Historie der Lagerbewegung (Lagerbewegungsdaten)
    
