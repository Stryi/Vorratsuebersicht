SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

REM Meine Datenbank zurück zum Smartphone
%adbCmd% -s c4072f55556b    push Vorraete_Stryi.db3    /storage/sdcard1/Vorratsuebersicht/Vorraete.db3

%adbCmd% -s c4072f55556b    push ..\Assets\Vorraete_db0.db3   /data/user/0/de.Stryi.Vorratsuebersicht/files/Vorraete_db0.db3
%adbCmd% -s c4072f55556b    push ..\Assets\Vorraete_Demo.db3  /data/user/0/de.Stryi.Vorratsuebersicht/files/Vorraete_Demo.db3

pause
