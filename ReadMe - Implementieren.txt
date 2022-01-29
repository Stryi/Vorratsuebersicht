--------------------------------------------------------------------------------
Projekt: Vorratsübersicht
--------------------------------------------------------------------------------

Vor dem Hochladen ins Google Play Store 
=======================================

Im MainActivity das Datum in

    MainActivity.preLaunchTestEndDay = new DateTime(2018, 9, 22);

auf heutiges Datum setzen, damit die automatische Pre-Launch Prüfung
bei Google Play nicht beim EAN Scan "gefangen bleibt".


Versionskontrolle
=================
GitHub: https://github.com/Stryi/Vorratsuebersicht


Entwicklungsumgebung
====================

Zum Übersetzen wird das Visual Studio Community 2019 verwendet.
Als Bibliothek wir Xamarin eingesetzt.
Programmiert ist es in C# Programmiersprache.

Google Play Store
=================

Play Store    : https://play.google.com/store/apps/details?id=de.stryi.Vorratsuebersicht&hl=de
Beta Test     : https://play.google.com/apps/testing/de.stryi.Vorratsuebersicht
Interner Test : https://play.google.com/apps/internaltest/4697556532584863618
Web Seite     : https://sites.google.com/site/vorratsuebersicht



Verwendete Datenbank
====================

Als Datenbank wir SQLite eingesetzt. Die Datenbank wird lokal auf dem Device abgelegt.

Für Android 10 Target Framework mit API Level 29 wird requestLegacyExternalStorage=True gesetzt,

    https://developer.android.com/training/data-storage/use-cases#opt-out-scoped-storage

damit die Datenbank auf dem internen Speicher 

    /storage/emulated/0/Vorratsuebersicht/Vorraete.db3

angelegt werden kann.

Ab Android 11 (Target Framework 30) wird der Parameter nicht mehr berücksichtigt.
Dann wird die Datenbank im Applikationsverzeichnis angelegt.

    /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files


Im privaten speicher wird auch eine Testdatenbank mit einigen Testdaten angelegt.
Diese kann zurückgesetzt werden auf Ursprungszustand oder komplett geleert werden.

    /data/user/0/de.stryi.Vorratsuebersicht/files/Vorraete_Test.db3


Projekt übersetzen
==================

Ursprünglische Implementiert anhand vom Beispiel:

https://developer.xamarin.com/guides/xamarin-forms/working-with/databases/

Ausgabeverzeichnis für "Bin" und "Obj" ist im Projekt auf

    ..\..\..\..\Builds\Vorratsübersicht\bin\Debug\

eingestellt, da ich den Quellcode über OneDrive auf meinen zweiten Computer "verteile".
Damit werden keine MByte Dateien in OneDrive übertragen.


NuGet Packages:

- FastAndroidCamera by James Athey
- Newtonsoft.Json
- SQLite-net-pcl  by SQLie-net
- Xamarin.Android von Microsoft
- Xamarin.Essentians by Microsoft
- ZXing.Net.Mobile by Redth


Icons der Anwendung:
====================

https://material.io/icons

Filled -> Selected Icon -> Android -> White -> PNG

Open Source
The icons are available under the Apache License Version 2.0. 
We'd love attribution in your app's "about" screen, but it's not 
required. The only thing we ask is that you not re-sell these icons.

Anwendungssymbol:
=================

Vü
https://romannurik.github.io/AndroidAssetStudio/icons-launcher.html#foreground.type=text&foreground.text.text=V%C3%BC&foreground.text.font=Acme&foreground.space.trim=1&foreground.space.pad=0.3&foreColor=rgba(96%2C%20125%2C%20139%2C%200)&backColor=rgb(68%2C%20138%2C%20255)&crop=1&backgroundShape=square&effects=elevate&name=ic_launcher

Vü nach den neuen Regeln von Google Play
https://romannurik.github.io/AndroidAssetStudio/icons-launcher.html#foreground.type=text&foreground.text.text=V%C3%BC&foreground.text.font=Acme&foreground.space.trim=1&foreground.space.pad=0.3&foreColor=rgba(96%2C%20125%2C%20139%2C%200)&backColor=rgb(68%2C%20138%2C%20255)&crop=1&backgroundShape=square&effects=none&name=ic_launcher_new

Vü mit kleinerer Schriftart
https://romannurik.github.io/AndroidAssetStudio/icons-launcher.html#foreground.type=text&foreground.text.text=V%C3%BC&foreground.text.font=Acme&foreground.space.trim=1&foreground.space.pad=0.4&foreColor=rgba(96%2C%20125%2C%20139%2C%200)&backColor=rgb(68%2C%20138%2C%20255)&crop=1&backgroundShape=square&effects=none&name=ic_launcher_new2

Probleme:
=========

1. Sortierung berücksichtigt nicht die deutschen Umlaute (SQLite).

2. Beim Fehler "Couldn't connect to logcat, GetProcessId returned: 0" beim Deployen auf ein Device

    - USB Kabel rausstecken
    - Smartphone neu starten
    - USB Kabel einstecken

    Zuerst die VMware starten und dann mit Visual Studio starten

    Emulator beenden und Visual Studio neu starten.

	App auf dem Smartphone oder Emulator deinstallieren.

3. Kann nicht deployen.
    Output:
            1>Failure [INSTALL_FAILED_UID_CHANGED]
            1>Build succeeded.
            1>An error occured. See full exception on logs for more details.
            1>Failure [INSTALL_FAILED_UID_CHANGED]

    adb shell
    cd data
    cd data
    rm -r de.stryi.Vorratsuebersicht

4. Device could not find component named: VorratsUebersicht.VorratsUebersicht/md5ec161f59b77d527a9195ed0174d46a11.SplashScreen

    Erneut alles neu Übersetzen.
    App auf dem Device (Emulator) deinstallieren.

5. Couldn't connect debugger. You can see more details in Xamarin Diagnostic output and the full exception on logs.

    App deinstallieren und vom Google Play Store installieren

6. Couldn't connect to logcat, GetProcessId returned: 0
   Couldn't connect debugger. You can see more details in Xamarin Diagnostic output and the full exception on logs.

   Xamarin Diagnostic:

        [D:RunShellCommand]:      c4072f55556b date +%s
        [D:RunShellCommand]:      c4072f55556b setprop "debug.mono.extra" "debug=127.0.0.1:29282:29283,timeout=1586421326,loglevel=0,server=y"
        [D:RunShellCommand]:      c4072f55556b getprop
        [D:RunShellCommand]:      c4072f55556b "echo" "-n" "${EMULATED_STORAGE_SOURCE}"
        [D:RunShellCommand]:      c4072f55556b "echo" "-n" "${EMULATED_STORAGE_TARGET}"
        [D:RunShellCommand]:      c4072f55556b am broadcast -a "mono.android.intent.action.EXTERNAL_STORAGE_DIRECTORY" -n "Mono.Android.DebugRuntime/com.xamarin.mono.android.ExternalStorageDirectory"
        [D:RunShellCommand]:      c4072f55556b "echo" "-n" "${EXTERNAL_STORAGE}"
        [D:RunShellCommand]:      c4072f55556b am start -a "android.intent.action.MAIN" -c "android.intent.category.LAUNCHER" -n "de.stryi.Vorratsuebersicht/md56c9fe683bd4750f69443fa5376e732f4.SplashScreenActivity"
        [D:RunShellCommand]:      c4072f55556b ps
        [D:RunShellCommand]:      c4072f55556b am force-stop de.stryi.Vorratsuebersicht
        [D:RunShellCommand]:      c4072f55556b setprop "debug.mono.connect" ""

    Lösung:

    - App auf dem Smartphine deinstalliert
    - Nach dem Start kam dann

        Error: Device could not find component named: de.stryi.Vorratsuebersicht/md56c9fe683bd4750f69443fa5376e732f4.SplashScreenActivity
        The application could not be started. Ensure that the application has been installed to the target device and has a launchable activity (MainLauncher = true).
        Additionally, check Build->Configuration Manager to ensure this project is set to Deploy for this configuration.
        Couldn't connect debugger. You can see more details in Xamarin Diagnostic output and the full exception on logs.

     -  Rebuild -> Deploy -> Start
     -  Funktioniert!!!

7. Error Starting Application: Failed to forward ports.
   Couldn't connect debugger. You can see more details in Xamarin Diagnostic output and the full exception on logs.


    [D:NotifyPhase]:          Creating directories
    [D:NotifySync]:           SkipCreateDirectory  /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/.__override__ 0
    [D:NotifyPhase]:          Uploading files
    [D:NotifyPhase]:          Upload completed
    [D:NotifyPhase]:          Enumerating remote files
    [D:NotifyPhase]:          Determining required operations
    [D:NotifyPhase]:          Creating directories
    [D:NotifySync]:           SkipCreateDirectory  /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/.__override__ 0
    [D:NotifySync]:           SkipCreateDirectory  /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/.__override__/resources 0
    [D:NotifyPhase]:          Uploading files
    [D:NotifyPhase]:          Upload completed
    [D:RunShellCommand]:      emulator-5554 "echo" "-n" "${EMULATED_STORAGE_SOURCE}"
    [D:RunShellCommand]:      emulator-5554 "echo" "-n" "${EMULATED_STORAGE_TARGET}"
    [D:RunShellCommand]:      emulator-5554 am broadcast -a "mono.android.intent.action.EXTERNAL_STORAGE_DIRECTORY" -n "Mono.Android.DebugRuntime/com.xamarin.mono.android.ExternalStorageDirectory"
    [D:RunShellCommand]:      emulator-5554 am force-stop de.stryi.Vorratsuebersicht
    [D:RunShellCommand]:      emulator-5554 setprop "debug.mono.connect" ""


    - App Deinstallieren
    - Restart adb Server
    
    Danach:

        Error: Device could not find component named: de.stryi.Vorratsuebersicht/md56c9fe683bd4750f69443fa5376e732f4.SplashScreenActivity
        The application could not be started. Ensure that the application has been installed to the target device and has a launchable activity (MainLauncher = true).
        Additionally, check Build->Configuration Manager to ensure this project is set to Deploy for this configuration.
        Couldn't connect debugger. You can see more details in Xamarin Diagnostic output and the full exception on logs.

    - Build -> Deploy Solution

        Irgendwas mit VorratsUebersicht.VorratsUebersicht/md5ec161f59b77d527a9195ed0174d46a11.SplashScreen

    - Build -> Rebuild Solution
      Erneut Starten

    - Ggf. am Quellcode was ändern und neu übersetzen.
    - Platz auf dem Smartphone schaffen.


