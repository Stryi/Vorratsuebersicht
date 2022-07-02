@ECHO OFF

SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

REM Root Zugriff f�r adb
%adbCmd% root

ECHO ----------------------------------------------------
ECHO Meine Datenbanken zum Emulator uebertragen
ECHO ----------------------------------------------------

ECHO **** Bereinigung
%adbCmd% shell rm    /storage/emulated/0/Vorratsuebersicht/Vorraete.db3
%adbCmd% shell rm    /storage/emulated/0/Vorratsuebersicht/Jacht.db3
%adbCmd% shell rm    /storage/emulated/0/Vorratsuebersicht/Ferienhaus Florida.db3
%adbCmd% shell rm    /storage/emulated/0/Vorratsuebersicht/KAPUTT.db3
%adbCmd% shell rm    /storage/emulated/0/Vorratsuebersicht/*.*
%adbCmd% shell rmdir /storage/emulated/0/Vorratsuebersicht

ECHO **** Uebertragung
%adbCmd% push "..\..\Testdaten\Vorraete.db3"               "/storage/emulated/0/Vorratsuebersicht/Vorraete.db3"
%adbCmd% push "..\..\Testdaten\Jacht.db3"                  "/storage/emulated/0/Vorratsuebersicht/Jacht.db3"
%adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"     "/storage/emulated/0/Vorratsuebersicht/Ferienhaus Florida.db3"
%adbCmd% push "..\..\Testdaten\KAPUTT.db3"                 "/storage/emulated/0/Vorratsuebersicht/KAPUTT.db3"
%adbCmd% push "..\..\Testdaten\Vue_OLD_FORMAT.VueBak"      "/storage/emulated/0/Vorratsuebersicht/Vue_OLD_FORMAT.db3"


ECHO ----------------------------------------------------
ECHO db0 und Testdatenbank uebertragen
ECHO ----------------------------------------------------

%adbCmd% push "..\Assets\Vorraete_db0.db3"                 "/data/user/0/de.stryi.Vorratsuebersicht/files/Vorraete_db0.db3"
%adbCmd% push "..\Assets\Vorraete_Demo.db3"                "/data/user/0/de.stryi.Vorratsuebersicht/files/Vorraete_Test.db3"



ECHO - Daten auf internen Speicher kopieren
rem %adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"         "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/01 Interner Speicher.db3"

ECHO ----------------------------------------------------
ECHO - Daten auf SD Karte kopieren
ECHO ----------------------------------------------------

REM Stryi_10
%adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"     "/storage/0E0E-2316/Android/data/de.stryi.Vorratsuebersicht/files/02 SD Karte.db3"

REM REM %adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"     "/storage/0FE9-280F/Android/data/de.stryi.Vorratsuebersicht/files/02 SD Karte.db3"



ECHO ----------------------------------------------------
ECHO Backup uebertragen
ECHO ----------------------------------------------------

%adbCmd% push "..\..\Testdaten\Vue_OLD_FORMAT.VueBak"      "/storage/emulated/0/Download/Vue_OLD_FORMAT.VueBak""


ECHO ----------------------------------------------------
ECHO Bilder uebertragen
ECHO ----------------------------------------------------

REM 'Camera' Verzeichnis erst verf�gbar, wenn das erste Bild mit der Camera App gemacht wurde.
REM %adbCmd%    shell mkdir /storage/emulated/0/DCIM/Camera/

REM %adbCmd% push "..\..\Testdaten\Pictures\01 Big Landscape 2560 x1920.jpg"   /storage/emulated/0/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\02 Big Portrait 1920 x 2560.jpg"   /storage/emulated/0/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\03 Large Land 4608 x 3456.jpg"     /storage/emulated/0/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\04 Large Port 3456 x 4608.jpg"     /storage/emulated/0/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\05 Small Landscape 640 x480.jpg"   /storage/emulated/0/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\06 Small Portrait 480 x 640.jpg"   /storage/emulated/0/Pictures/


ECHO ----------------------------------------------------
ECHO DropBox Verzeichnis
ECHO ----------------------------------------------------

REM %adbCmd% shell mkdir /storage/emulated/0/DropBox/
REM %adbCmd% push ..\..\Testdaten\Vorraete-DropBox.db3  /storage/emulated/0/DropBox/Vorraete-DropBox.db3
REM %adbCmd% shell chmod 440 /storage/emulated/0/DropBox/Vorraete-DropBox.db3

pause
