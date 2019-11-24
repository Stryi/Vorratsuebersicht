@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb.exe"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

ECHO Datenbank vom Emulator loeschen - SD Karte (emuliert)
%adbCmd% -s emulator-5554 shell rm /storage/emulated/0/Vorratsuebersicht/Vorraete.db3
%adbCmd% -s emulator-5554 shell rm /storage/emulated/0/Vorratsuebersicht/Vorraete-Test.db3

ECHO Datenbank vom Emulator loeschen - SD Karte
%adbCmd% -s emulator-5554 shell rm /storage/sdcard/Vorratsuebersicht/Vorraete.db3
%adbCmd% -s emulator-5554 shell rm /storage/sdcard/Vorratsuebersicht/Vorraete-Test.db3

pause 