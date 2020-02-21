@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb.exe"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

REM Bis Android 5.1
SET SD_Card_Path=/storage/sdcard
SET App_Path=/data/data/de.Stryi.Vorratsuebersicht/files

REM Ab Android 6.0
REM SET SD_Card_Path=/storage/emulated/0
REM SET App_Path=/data/user/0/de.Stryi.Vorratsuebersicht/files

ECHO Datenbank vom Emulator loeschen - SD Karte (emuliert)
%adbCmd% -s emulator-5554 shell rm %SD_Card_Path%/Vorratsuebersicht/Vorraete.db3
%adbCmd% -s emulator-5554 shell rm %SD_Card_Path%/Vorratsuebersicht/Vorraete-Test.db3
%adbCmd% -s emulator-5554 shell rm %SD_Card_Path%/Vorratsuebersicht/Vorraete_Stryi_Kaputt.db3

pause 