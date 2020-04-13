@ECHO OFF

SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

REM Bis Android 5.1
REM SET SD_Card_Path=/storage/sdcard
REM SET App_Path=/data/data/de.Stryi.Vorratsuebersicht/files

REM Ab Android 6.0
SET SD_Card_Path=/storage/emulated/0
SET App_Path=/data/user/0/de.stryi.Vorratsuebersicht/files

SET EmulatorName=emulator-5554
REM SET EmulatorName=emulator-5556


ECHO ----------------------------------------------------
ECHO Meine Datenbank zum Emulator uebertragen - SD Karte
ECHO ----------------------------------------------------

REM Bereinigung
REM %adbCmd% -s %EmulatorName%   shell rm    %SD_Card_Path%/Vorratsuebersicht/Vorraete.db3
REM %adbCmd% -s %EmulatorName%   shell rm    %SD_Card_Path%/Vorratsuebersicht/Vorraete-Test.db3
REM %adbCmd% -s %EmulatorName%   shell rm    %SD_Card_Path%/Vorratsuebersicht/Vorraete_Stryi_Kaputt.db3
%adbCmd% shell rm    %SD_Card_Path%/Vorratsuebersicht/*.*
%adbCmd% shell rmdir %SD_Card_Path%/Vorratsuebersicht

REM Daten kopieren
REM %adbCmd% push "..\..\Testdaten\Jacht.db3"                  "%SD_Card_Path%/Vorratsuebersicht/Jacht.db3"
REM %adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"     "%SD_Card_Path%/Vorratsuebersicht/Ferienhaus Florida.db3"
REM %adbCmd% push "..\..\Testdaten\Vorraete_Stryi.db3"         "%SD_Card_Path%/Vorratsuebersicht/Vorraete.db3"

REM %adbCmd% -s %EmulatorName%   push "..\..\Testdaten\Vorraete_Stryi_Kaputt.db3"  "%SD_Card_Path%/Vorratsuebersicht/Vorraete_Stryi_Kaputt.db3"
REM %adbCmd% -s %EmulatorName%   push "..\..\Testdaten\Vue_2020-03-23 11.59.14.db3" "%SD_Card_Path%/Vorratsuebersicht/Vue_2020-03-23 11.59.14.db3"

ECHO ----------------------------------------------------
ECHO db0 und Testdatenbank uebertragen
ECHO ----------------------------------------------------

%adbCmd% root
%adbCmd% push "..\..\Testdaten\Vorraete_Stryi.db3"         "%App_Path%/Vorraete.db3"
REM %adbCmd% push ..\Assets\Vorraete_db0.db3   %App_Path%/Vorraete_db0.db3
REM %adbCmd% push ..\Assets\Vorraete_Demo.db3  %App_Path%/Vorraete_Test.db3


ECHO ----------------------------------------------------
ECHO Backup uebertragen
ECHO ----------------------------------------------------

%adbCmd% -s %EmulatorName%   push ..\..\Testdaten\Vue_2019-07-22_OLD_FORMAT.VueBak  %SD_Card_Path%/Download/Vue_2019-07-22_OLD_FORMAT.VueBak


ECHO ----------------------------------------------------
ECHO Bilder uebertragen
ECHO ----------------------------------------------------
REM 'Camera' Verzeichnis erst verfügbar, wenn das erste Bild mit der Camera App gemacht wurde.

%adbCmd% -s %EmulatorName%   shell mkdir %SD_Card_Path%/DCIM/Camera/

%adbCmd% -s %EmulatorName%   push "..\Pictures\01 Ravioli.jpeg"   %SD_Card_Path%/DCIM/Camera/
%adbCmd% -s %EmulatorName%   push "..\Pictures\02 Pfirsich.jpeg"  %SD_Card_Path%/DCIM/Camera/
%adbCmd% -s %EmulatorName%   push "..\Pictures\03 Zucker.jpeg"    %SD_Card_Path%/DCIM/Camera/


ECHO ----------------------------------------------------
ECHO DropBox Verzeichnis
ECHO ----------------------------------------------------

%adbCmd% -s %EmulatorName%   push ..\..\Testdaten\Vorraete-DropBox.db3  %SD_Card_Path%/DropBox/Vorraete-DropBox.db3

pause
