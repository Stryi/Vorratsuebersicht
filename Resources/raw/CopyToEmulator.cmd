SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"

REM %adbCmd% -s emulator-5554   push Vorraete_db0.db3   /storage/sdcard/Vorratsuebersicht/Vorraete.db3
REM %adbCmd% -s emulator-5554   push Vorraete_db0.db3   /storage/sdcard/Vorratsuebersicht/Vorraete_db0.db3
REM %adbCmd% -s emulator-5554   push Vorraete_Demo.db3  /storage/sdcard/Vorratsuebersicht/Vorraete_Demo.db3

REM Meine Datenbank zum Emulator übertragen
%adbCmd% -s emulator-5554   push Vorraete_Stryi.db3  /storage/sdcard/Vorratsuebersicht/Vorraete.db3
REM %adbCmd% -s emulator-5554   push Vorraete_Stryi.db3  /data/user/0/de.Stryi.Vorratsuebersicht/files/Vorraete.db3

pause
