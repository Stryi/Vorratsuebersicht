@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"
REM SET adbCmd="E:\android-adk\platform-tools\adb"


ECHO Meine Datenbank zum Emulator Åbertragen - SD Karte
%adbCmd% -s emulator-5554   push Vorraete_Stryi.db3  /storage/sdcard/Vorratsuebersicht/Vorraete.db3

REM ECHO Meine Datenbank zum Emulator Åbertragen - keine SD Karte
REM %adbCmd% -s emulator-5554   push Vorraete_Stryi.db3  /data/user/0/de.Stryi.Vorratsuebersicht/files/Vorraete.db3

ECHO db0 und Testdatenbank Åbertragen
%adbCmd% -s emulator-5554   push ..\..\Assets\Vorraete_db0.db3   /data/data/de.Stryi.Vorratsuebersicht/files/Vorraete_db0.db3
%adbCmd% -s emulator-5554   push ..\..\Assets\Vorraete_Demo.db3  /data/data/de.Stryi.Vorratsuebersicht/files/Vorraete_Test.db3

pause
