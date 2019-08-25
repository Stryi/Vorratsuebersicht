@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

ECHO Meine Datenbank vom Smartphone hierher Åbertragen
%adbCmd% -s emulator-5554    pull /storage/sdcard/Vorratsuebersicht/Vorraete-Test.db3     ./Vorraete-Test.db3 

pause
