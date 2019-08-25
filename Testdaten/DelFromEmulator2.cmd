@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

ECHO Datenbank vom Emulator löschen - SD Karte (emuliert)
%adbCmd% -s emulator-5554 shell rm /storage/emulated/0/Vorratsuebersicht/Vorraete.db3

REM ECHO Datenbank vom Emulator löschen - SD Karte
REM %adbCmd% -s emulator-5554 push Vorraete_Stryi.db3 /data/user/0/de.Stryi.Vorratsuebersicht/files/Vorraete.db3

pause 