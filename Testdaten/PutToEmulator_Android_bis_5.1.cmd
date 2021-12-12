@ECHO OFF

SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

REM Root Zugriff für adb
%adbCmd% root

ECHO ----------------------------------------------------
ECHO Meine Datenbanken zum Emulator uebertragen
ECHO ----------------------------------------------------

ECHO **** Bereinigung
%adbCmd% shell rm    /storage/sdcard/Vorratsuebersicht/Vorraete.db3
%adbCmd% shell rm    /storage/sdcard/Vorratsuebersicht/Jacht.db3
%adbCmd% shell rm    /storage/sdcard/Vorratsuebersicht/Ferienhaus Florida.db3.db3
%adbCmd% shell rm    /storage/sdcard/Vorratsuebersicht/KAPUTT.db3
%adbCmd% shell rm    /storage/sdcard/Vorratsuebersicht/*.*
%adbCmd% shell rmdir /storage/sdcard/Vorratsuebersicht

ECHO **** Uebertragung
%adbCmd% push "..\..\Testdaten\Vorraete.db3"               "/storage/sdcard/Vorratsuebersicht/Vorraete.db3"
%adbCmd% push "..\..\Testdaten\Jacht.db3"                  "/storage/sdcard/Vorratsuebersicht/Jacht.db3"
%adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"     "/storage/sdcard/Vorratsuebersicht/Ferienhaus Florida.db3"
%adbCmd% push "..\..\Testdaten\KAPUTT.db3"                 "/storage/sdcard/Vorratsuebersicht/KAPUTT.db3"


ECHO ----------------------------------------------------
ECHO db0 und Testdatenbank uebertragen
ECHO ----------------------------------------------------

%adbCmd% push "..\Assets\Vorraete_db0.db3"                 "/data/user/0/de.stryi.Vorratsuebersicht/files/Vorraete_db0.db3"
%adbCmd% push "..\Assets\Vorraete_Demo.db3"                "/data/user/0/de.stryi.Vorratsuebersicht/files/Vorraete_Test.db3"



ECHO ----------------------------------------------------
ECHO Backup uebertragen
ECHO ----------------------------------------------------

%adbCmd% push ..\..\Testdaten\Vue_2019-07-22_OLD_FORMAT.VueBak  /storage/sdcard/Download/Vue_2019-07-22_OLD_FORMAT.VueBak


ECHO ----------------------------------------------------
ECHO Bilder uebertragen
ECHO ----------------------------------------------------

REM 'Camera' Verzeichnis erst verfügbar, wenn das erste Bild mit der Camera App gemacht wurde.
%adbCmd%    shell mkdir /storage/sdcard/DCIM/Camera/

%adbCmd% push "..\..\Testdaten\Pictures\01 Big Landscape 2560 x1920.jpg"   /storage/sdcard/Pictures/
%adbCmd% push "..\..\Testdaten\Pictures\02 Big Portrait 1920 x 2560.jpg"   /storage/sdcard/Pictures/
%adbCmd% push "..\..\Testdaten\Pictures\03 Large Land 4608 x 3456.jpg"     /storage/sdcard/Pictures/
%adbCmd% push "..\..\Testdaten\Pictures\04 Large Port 3456 x 4608.jpg"     /storage/sdcard/Pictures/
%adbCmd% push "..\..\Testdaten\Pictures\05 Small Landscape 640 x480.jpg"   /storage/sdcard/Pictures/
%adbCmd% push "..\..\Testdaten\Pictures\06 Small Portrait 480 x 640.jpg"   /storage/sdcard/Pictures/


ECHO ----------------------------------------------------
ECHO DropBox Verzeichnis
ECHO ----------------------------------------------------

%adbCmd% shell mkdir /storage/sdcard/DropBox/
%adbCmd% push ..\..\Testdaten\Vorraete-DropBox.db3  /storage/sdcard/DropBox/Vorraete-DropBox.db3
%adbCmd% shell chmod 440 /storage/sdcard/DropBox/Vorraete-DropBox.db3

pause
