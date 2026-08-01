# Lavenders Calendar

Lavenders Calendar is a calm, bilingual desktop calendar for Windows. It provides a two-week calendar view with local SQLite storage, event creation and editing, keyboard-accessible controls, and a soft lavender visual theme.

The interface can be switched between Finnish and English. Settings also allow weekends to be shown or hidden and the lavender light or dark theme to be selected. Preferences are saved locally in `%AppData%\Lavenders\settings.json`.

## System requirements

- Windows 11 on an x64 processor
- No separate .NET installation is required when using the self-contained release

The application stores calendar data locally in `%AppData%\Lavenders\events.db`. It does not require an online account or cloud connection.

## Installing and running

Tagged releases are packaged with Velopack. Install with `Lavenders.Calendar-win-Setup.exe`; later releases can be downloaded and applied from the update banner inside Lavenders. Portable ZIP builds may still be provided, but cannot update themselves.

Back up `%AppData%\Lavenders\events.db` before manually deleting application data. Removing the application files does not delete the calendar database.

## Building

Prerequisites:

- .NET 10 SDK
- Windows with the .NET desktop development tools

```powershell
dotnet restore Lavenders.slnx
dotnet build Lavenders.slnx --configuration Release
dotnet test Lavenders.slnx --configuration Release
```

## Publishing v1.0.0

The `WinX64` profile creates a self-contained, single-file Windows x64 build:

```powershell
dotnet publish src\Lavenders.UI\Lavenders.UI.csproj -p:PublishProfile=WinX64
```

Output is written to `src\Lavenders.UI\bin\publish\win-x64\`.

## Data and privacy

All events are stored locally. Startup failures are logged to `%LocalAppData%\Lavenders\Logs\startup.log`; the log can contain technical exception details but is not uploaded automatically.

Calendar creation, editing, browsing, settings, and database access work without
an internet connection. An installed copy contacts GitHub only to check for and,
after the user chooses **Update and restart**, download Lavenders releases. Update
checks do not include calendar events, descriptions, settings, or other user data.

## License

Lavenders is released under the [MIT License](LICENSE). Third-party components
remain subject to their respective terms; see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
