@ECHO OFF

SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

ECHO Meine Datenbank vom Smartphone hierher uebertragen.
%adbCmd% pull "/storage/emulated/0/Vorratsuebersicht/Vorraete.db3" "..\..\Testdaten\Vorraete_Stryi.db3"

REM Testdatenbanken
REM Testdatenbanken
%adbCmd% pull "/storage/emulated/0/Vorratsuebersicht/Jacht.db3"              "..\..\Testdaten\Jacht.db3"                  
%adbCmd% pull "/storage/emulated/0/Vorratsuebersicht/Ferienhaus Florida.db3" "..\..\Testdaten\Ferienhaus Florida.db3"

pause
