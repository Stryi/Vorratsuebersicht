@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

ECHO Meine Datenbank vom Smartphone hierher Åbertragen
%adbCmd% -s c4072f55556b    pull /storage/sdcard1/Vorratsuebersicht/Vorraete.db3 ./Vorraete_Stryi.db3 

pause
