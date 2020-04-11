--------------------------------------------------------------------------------
Projekt: Vorratsübersicht
--------------------------------------------------------------------------------

Implementiert anhand vom Beispiel:

https://developer.xamarin.com/guides/xamarin-forms/working-with/databases/

NuGet Packages:

- FastAndroidCamera by James Athey
- SQLite-net-pcl  by Frank A. Krueger
- SQLitePCLRaw.bundle_green by Eric Sink
- SQLitePCLRaw.core by Eric Sink, et al
- SQLitePCLRaw.lib.e_sqlite3.android by Eric Sink, D. Richard Hipp, et al
- SQLitePCLRaw.provider.e_sqlite3.android by Eric Sink, et al
- ZXing.Net.Mobile by Redth


Icons der Anwendung:
====================

https://material.io/icons

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



