@ECHO OFF

SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

REM Meine Datenbank zurück zum Smartphone
REM %adbCmd% push ..\..\Testdaten\Vorraete_Stryi.db3    /storage/emulated/0/Vorratsuebersicht/Vorraete.db3

REM Die leere und die Demo Datenbank.
REM %adbCmd% push ..\Assets\Vorraete_db0.db3   /data/user/0/de.Stryi.Vorratsuebersicht/files/Vorraete_db0.db3
REM %adbCmd% push ..\Assets\Vorraete_Demo.db3  /data/user/0/de.Stryi.Vorratsuebersicht/files/Vorraete_Demo.db3

REM Testdatenbanken
%adbCmd% push "..\..\Testdaten\Jacht.db3"                  "/storage/emulated/0/Vorratsuebersicht/Jacht.db3"
%adbCmd% push "..\..\Testdaten\Ferienhaus Florida.db3"     "/storage/emulated/0/Vorratsuebersicht/Ferienhaus Florida.db3"
%adbCmd% push "..\..\Testdaten\Vorraete_Stryi_Kaputt.db3"  "/storage/emulated/0/Vorratsuebersicht/KAPUTT.db3"

pause
