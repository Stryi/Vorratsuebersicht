@ECHO OFF

SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

SET SD_Card_Path=/storage/sdcard
REM SET SD_Card_Path=/storage/emulated/0

SET App_Path=/data/data/de.Stryi.Vorratsuebersicht/files
REM SET App_Path=/data/user/0/de.Stryi.Vorratsuebersicht/files

ECHO ----------------------------------------------------
ECHO Meine Datenbank zum Emulator uebertragen - SD Karte
ECHO ----------------------------------------------------

%adbCmd% -s emulator-5554   push Vorraete_Stryi.db3        %SD_Card_Path%/Vorratsuebersicht/Vorraete.db3
%adbCmd% -s emulator-5554   push Vorraete-Test.db3         %SD_Card_Path%/Vorratsuebersicht/Vorraete-Test.db3
%adbCmd% -s emulator-5554   push Vorraete_Stryi_Kaputt.db3 %SD_Card_Path%/Vorratsuebersicht/Vorraete_Stryi_Kaputt.db3

ECHO ----------------------------------------------------
ECHO DropBox Verzeichnis
ECHO ----------------------------------------------------

%adbCmd% -s emulator-5554   push Vorraete-DropBox.db3  %SD_Card_Path%/DropBox/Vorraete-DropBox.db3


ECHO ----------------------------------------------------
ECHO db0 und Testdatenbank uebertragen
ECHO ----------------------------------------------------

%adbCmd% -s emulator-5554   push ..\Assets\Vorraete_db0.db3   %App_Path%/Vorraete_db0.db3
%adbCmd% -s emulator-5554   push ..\Assets\Vorraete_Demo.db3  %App_Path%/Vorraete_Test.db3


REM Uebergabe Backup
REM %adbCmd% -s emulator-5554   push Vue_2019-07-22_OLD_FORMAT.VueBak  /storage/sdcard/Download/Vue_2019-07-22_OLD_FORMAT.VueBak

ECHO ----------------------------------------------------
ECHO Bilder uebertragen
ECHO ----------------------------------------------------
REM 'Camera' Verzeichnis erst verfügbar, wenn das erste Bild mit der Camera App gemacht wurde.

%adbCmd% -s emulator-5554   push "..\Pictures\01 Ravioli.jpeg"   %SD_Card_Path%/DCIM/Camera/
%adbCmd% -s emulator-5554   push "..\Pictures\02 Pfirsich.jpeg"  %SD_Card_Path%/DCIM/Camera/
%adbCmd% -s emulator-5554   push "..\Pictures\03 Zucker.jpeg"    %SD_Card_Path%/DCIM/Camera/

pause
