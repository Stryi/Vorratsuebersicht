SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

REM Meine Datenbank vom Smartphone hierher übertragen
%adbCmd% -s c4072f55556b    pull /storage/sdcard1/Vorratsuebersicht/Vorraete.db3 ./Vorraete_Stryi.db3 

pause
