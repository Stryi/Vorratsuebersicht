@ECHO OFF
SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

ECHO Meine Datenbank vom Smartphone hierher �bertragen
%adbCmd% -s emulator-5554    pull /storage/sdcard/Vorratsuebersicht/Vorraete.db3     ./Vorraete_Emulator.db3 
%adbCmd% -s emulator-5554    pull /storage/sdcard/Vorratsuebersicht/Vorraete.db3-shm ./Vorraete_Emulator.db3-shm
%adbCmd% -s emulator-5554    pull /storage/sdcard/Vorratsuebersicht/Vorraete.db3-wal ./Vorraete_Emulator.db3-wal

pause
