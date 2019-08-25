@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

SET SD_Card_Path=/storage/sdcard/Vorratsuebersicht
REM SET SD_Card_Path=/storage/emulated/0/Vorratsuebersicht

SET SD_Emulator_Path=/data/user/0/de.Stryi.Vorratsuebersicht/files

SET App_Path=/data/data/de.Stryi.Vorratsuebersicht/files

ECHO Meine Datenbank zum Emulator Åbertragen - SD Karte
%adbCmd% -s emulator-5554   push Vorraete_Stryi.db3   %SD_Card_Path%/Vorraete.db3
%adbCmd% -s emulator-5554   push Vorraete-Test.db3    %SD_Card_Path%/Vorraete-Test.db3

REM ECHO Meine Datenbank zum Emulator Åbertragen - keine SD Karte
REM %adbCmd% -s emulator-5554   push Vorraete_Stryi.db3  %SD_Emulator_Path%/Vorraete.db3

ECHO db0 und Testdatenbank Åbertragen
%adbCmd% -s emulator-5554   push ..\Assets\Vorraete_db0.db3   %App_Path%/Vorraete_db0.db3
%adbCmd% -s emulator-5554   push ..\Assets\Vorraete_Demo.db3  %App_Path%/Vorraete_Test.db3


REM Ueberage Backup
REM %adbCmd% -s emulator-5554   push Vue_2019-07-22_OLD_FORMAT.VueBak  /storage/sdcard/Download/Vue_2019-07-22_OLD_FORMAT.VueBak

ECHO Bilder Åbertragen
%adbCmd% -s emulator-5554   push "..\Pictures\01 Ravioli.jpeg"   /storage/sdcard/DCIM/Camera/
%adbCmd% -s emulator-5554   push "..\Pictures\02 Pfirsich.jpeg"  /storage/sdcard/DCIM/Camera/
%adbCmd% -s emulator-5554   push "..\Pictures\03 Zucker.jpeg"    /storage/sdcard/DCIM/Camera/

pause
