Gebrauchsanweisung
==================

Funktionsweise
---------------

Jeder Artikel muss zuerst einmalig erfasst werden.
Ein automatisches Laden der Angaben anhand des EAN Codes
ist bis jetzt anhand OpenFoodFacts.org möglich.
Es sind dort aber bis jetzt nicht alle Artiklen erfasst.

    Manuelle Erfassung: Artikelliste  ->  (+)  -> Artikelangaben erfassen und "Häckchen" klicken.

    Angaben aus dem Internet: Artikel scannen  ->  "Artikelangaben im Internet auf OpenFoodFacts.org suchen?" -> Ja

Erst danach kann für jeden Artikel die Menge und das Mindesthaltbarkeitsdatum erfasst werden.

    Lagerbestand -> (+) -> "Häckchen" klicken -> (+) auswählen, Mindesthaltbarkeitsdatum auswählen und Menge mit (+) erfassen.

Hat ein Artikel ein EAN Code, so kann man nach "Artikel scannen" direkt die Artikeldaten bearbeiten,
den Lagerbestand erfassen oder den Artikel auf die Einkaufsliste setzen.


Vorgehensweise im Detail
------------------------


1. Neuen Artikel gekauft.

1.1 Der Artikel hat kein EAN-Code (Bananen, Äpfel, selbst gemachte Marmelade, Eier, ...)

Aus dem Hauptmenü "Artikelliste" auswählen und "+" drücken.
Ein Foto machen oder auswählen und die Stammdaten wie
"Artikelname", "Kategorie" usw. erfassen.
"OK" Häkchen klicken und einmal "Zurück" Taste (zum Hauptmenü).

1.2 Artikel hat einen EAN Code.

Aus dem Hauptmenü "Artikel scannen auswählen, danach den EAN Code scannen.
Wenn der Artikel mit dem EAN Code noch gar nicht erfasst wurde,
so komme ich in eine leere Artikeldetail Maske mit dem EAN Code schon eingetragen.

Weitere Angaben kann ich dann zu dem Artikel machen (Name, Kategorie, Gewicht, Kalorien,...)
Mit "OK" Häkchen komme ich wieder zurück zur Hauptmaske.

Somit habe ich damit (einmalig) einen Artikel erfasst.

2. Lagerbestand eintragen.

Ohne EAN Code:
Aus der Hauptmaske "Lagerbestand" klicken und dann "(+)".
Den neuen Artikel auswählen (Klicken).
Eine Lagerbestandsmaske öffnet siche (mit "Abbrechen", "OK" und "Einkaufswagen")
Mit dem unteren "+" kann ich dann das Ablaufdatum und Anzahl erfassen.
Bei Artikeln, die kein Ablaufdatum haben (Honig, Dosen, Mehl,...)
kommt keine Abfrage nach dem Datum.

Mit EAN Code:
Aus dem Hauptmenü klicke ich "Artikel scannen".
Artikel scannen. Es kommt eine Auswahl:

   - Lagerbestand
   - Artikelangaben
   - Einkaufsliste

Damit kann man dann sich entscheiden,
ob ich was vom Lager nehmen oder reintun will,
die Artikelangaben ändern will oder
auf die Einkaufsliste setzen will (weil ich das letzte gerade genommen habe).


Logik der Bestellmenge
----------------------

Der Anwender bekommt beim Speichern der Bestandsliste bei 

	[ToBuy] -[Menge auf Einkaufszettel] > 0 
	
eine Meldung, ob er das Artikel mit der Menge (s. Formel) auf die Einkaufsliste setzen will.

Beim Betätigen des 'Einkaufswagen' Icons wird ohne Nachfrage 'ToBuy' Menge bestellt
oder (falls schon auf dem Einkaufszettel vorhanden) die Menge im Einkaufszettel um 1 erhöht.

	Nur 'MinQuantity' gesetzt: Bestellt wird beim Unterschreiten auf die 'MinQuantity' Menge.


		MinQuantity		PrefQuantity		IsQuantity		ToBuy
		5				-					6				-
		5				-					5				-
		5				-					4				1
		5				-					3				2
		5				-					2				3

	Nur 'PrefQuantity' gesetzt: Verhalten wie bei 'MinQuantity'.
	Bestellt wird beim Unterschreiten auf die 'PrefQuantity' Menge.

		MinQuantity		PrefQuantity		IsQuantity		ToBuy
		-				8					9				-
		-				8					8				-
		-				8					7				1
		-				8					1				6


	Nur 'MinQuantity' und 'PrefQuantity' gesetzt: 
	Beim Unterschreiten der MinQuantity	wird auf die PrefQuantity bestellt

		MinQuantity		PrefQuantity		IsQuantity		ToBuy
		5				8					9				-
		5				8					8				-
		5				8					7				-
		5				8					6				-
		5				8					5				-
		5				8					4				4
		5				8					3				5
		5				8					4				6
		5				8					1				7

