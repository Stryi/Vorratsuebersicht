SET adbCmd="%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb"

%adbCmd% -s emulator-5554   push Vorraete_db0.db3   /storage/sdcard/Vorraete.db3
%adbCmd% -s emulator-5554   push Vorraete_db0.db3   /storage/sdcard/Vorraete_db0.db3
%adbCmd% -s emulator-5554   push Vorraete_Demo.db3  /storage/sdcard/Vorraete_Demo.db3

pause
