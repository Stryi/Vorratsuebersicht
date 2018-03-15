SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

REM %adbCmd% -s c4072f55556b    push Vorraete_db0.db3   /storage/sdcard1/Vorratsuebersicht/Vorraete.db3

%adbCmd% -s c4072f55556b    push Vorraete_db0.db3   /storage/sdcard1/Vorratsuebersicht/Vorraete_db0.db3
%adbCmd% -s c4072f55556b    push Vorraete_Demo.db3  /storage/sdcard1/Vorratsuebersicht/Vorraete_Demo.db3

%adbCmd% -s c4072f55556b    pull /storage/sdcard1/Vorratsuebersicht/Vorraete.db3 ./Vorraete_Stryi.db3 

pause
