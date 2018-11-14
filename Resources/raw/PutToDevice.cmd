SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

REM Meine Datenbank zurück zum Smartphone
%adbCmd% -s c4072f55556b    push Vorraete_Stryi.db3    /storage/sdcard1/Vorratsuebersicht/Vorraete.db3

pause
