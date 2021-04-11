@ECHO OFF

SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

REM Bis Android 5.1
REM SET SD_Card_Path=/storage/sdcard
REM SET App_Path=/data/data/de.Stryi.Vorratsuebersicht/files

REM Ab Android 6.0
SET Internal_Path=/storage/emulated/0
SET SD_Card_Path=/storage/0FE9-280F
SET App_Path=/data/user/0/de.stryi.Vorratsuebersicht/files


%adbCmd% root

ECHO ----------------------------------------------------
ECHO Meine Datenbank zum Emulator uebertragen - SD Karte
ECHO ----------------------------------------------------

REM Bereinigung
REM %adbCmd% shell rm    %Internal_Path%/Vorratsuebersicht/Vorraete.db3
REM %adbCmd% shell rm    %Internal_Path%/Vorratsuebersicht/Vorraete-Test.db3
REM %adbCmd% shell rm    %Internal_Path%/Vorratsuebersicht/KAPUTT.db3
REM %adbCmd% shell rm    %Internal_Path%/Vorratsuebersicht/*.*
REM %adbCmd% shell rmdir %Internal_Path%/Vorratsuebersicht

REM Daten kopieren
%adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"         "%Internal_Path%/Android/data/de.stryi.Vorratsuebersicht/files/01 Interner Speicher.db3"


REM %adbCmd% push "..\..\Testdaten\Jacht.db3"                  "%Internal_Path%/Vorratsuebersicht/Jacht.db3"
REM %adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"     "%Internal_Path%/Vorratsuebersicht/Ferienhaus Florida.db3"
REM %adbCmd% push "..\..\Testdaten\Vorraete_Stryi_Kaputt.db3"  "%Internal_Path%/Vorratsuebersicht/KAPUTT.db3"
REM %adbCmd% push "..\..\Testdaten\Vorraete_Stryi.db3"         "%Internal_Path%/Vorratsuebersicht/Vorraete.db3"

REM %adbCmd% push "..\..\Testdaten\Vue_2020-03-23 11.59.14.db3" "%Internal_Path%/Vorratsuebersicht/Vue_2020-03-23 11.59.14.db3"


ECHO ----------------------------------------------------
ECHO db0 und Testdatenbank uebertragen
ECHO ----------------------------------------------------

REM %adbCmd% root
REM %adbCmd% push "..\..\Testdaten\Vorraete_Stryi.db3"         "%App_Path%/Vorraete.db3"
REM %adbCmd% push "..\Assets\Vorraete_db0.db3"                 "%App_Path%/Vorraete_db0.db3"
REM %adbCmd% push "..\Assets\Vorraete_Demo.db3"                "%App_Path%/Vorraete_Test.db3"


ECHO ----------------------------------------------------
ECHO Backup uebertragen
ECHO ----------------------------------------------------

REM %adbCmd% push ..\..\Testdaten\Vue_2019-07-22_OLD_FORMAT.VueBak  %Internal_Path%/Download/Vue_2019-07-22_OLD_FORMAT.VueBak


ECHO ----------------------------------------------------
ECHO Bilder uebertragen
ECHO ----------------------------------------------------

REM 'Camera' Verzeichnis erst verfügbar, wenn das erste Bild mit der Camera App gemacht wurde.
REM %adbCmd%    shell mkdir %Internal_Path%/DCIM/Camera/

REM %adbCmd% push "..\..\Testdaten\Pictures\01 Big Landscape 2560 x1920.jpg"   %Internal_Path%/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\02 Big Portrait 1920 x 2560.jpg"   %Internal_Path%/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\03 Large Land 4608 x 3456.jpg"     %Internal_Path%/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\04 Large Port 3456 x 4608.jpg"     %Internal_Path%/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\05 Small Landscape 640 x480.jpg"   %Internal_Path%/Pictures/
REM %adbCmd% push "..\..\Testdaten\Pictures\06 Small Portrait 480 x 640.jpg"   %Internal_Path%/Pictures/


ECHO ----------------------------------------------------
ECHO DropBox Verzeichnis
ECHO ----------------------------------------------------

REM %adbCmd% shell mkdir %Internal_Path%/DropBox/
REM %adbCmd% push ..\..\Testdaten\Vorraete-DropBox.db3  %Internal_Path%/DropBox/Vorraete-DropBox.db3
REM %adbCmd% shell chmod 440 %Internal_Path%/DropBox/Vorraete-DropBox.db3

pause
