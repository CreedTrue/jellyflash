# Jellyflash

A Jellyfin plugin that enables support for playing `.swf` Flash files natively in the web client using the open-source [Ruffle](https://ruffle.rs/) emulator.

`Jellyflash` is **MIT licensed** and free for community use.

## Plugin Repository

Jellyflash is distributed as a Jellyfin plugin **repository** so it installs cleanly from the Catalog (the self-contained package layout is built automatically).

Add the repository to your Jellyfin server:

1. **Dashboard → Advanced → Plugins → Catalog** (or *Catalog* under Plugins).
2. Click **Add Repository**.
3. Enter the manifest URL:
   ```
   https://raw.githubusercontent.com/CreedTrue/jellyflash/manifest-release/manifest.json
   ```
4. Search for **Jellyflash** in the Catalog and click **Install**, then restart when prompted.

## How It Works

1. **Backend Integration**: The C# plugin registers an `IItemResolver` that looks for `.swf` files. This ensures your Flash files are scanned as `Video` items and appear in your Jellyfin library instead of being ignored.
2. **Frontend Injection**: When the plugin starts, it attempts to automatically inject a script into the Jellyfin web client's `index.html`. This script runs on the web client, listens for when you navigate to a `.swf` item, and adds a "Play with Ruffle" button that opens an embedded flash emulator over the page.

## Building

Since the plugin is compiled for `.NET` (version 8 or 9 depending on the target), a Docker-based build script is provided:

**Windows**:
```powershell
.\build.ps1
```

**Linux / macOS**:
```bash
docker run --rm -v "$(pwd)/Jellyflash:/app" -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet build -c Release
```

Once built, the output will be written to `Jellyflash/bin/Release/<abi>/` (for example `net8.0` or `net9.0`). This is raw build output, used only for local testing — it is **not** the install package (see Installation).

## Publishing a Release

The included GitHub Actions pipeline generates and publishes the self-contained plugin package to the repository. Publishing a new version:

1. Bump the `version` and update the `changelog` in `build.yaml`.
2. Push a `v1.0.0.1` tag (or edit the settings to match your tag naming) on `master`.
3. GitHub Actions: the **Build Plugin** workflow compiles and packages the plugin, the **Publish Release** workflow uploads the package to the tagged GitHub Release and regenerates `manifest.json` on the `manifest-release` branch.
4. Users install/update from the Catalog using the repository URL above.

`GITHUB_TOKEN` is provided automatically by GitHub; no extra secrets are required.

## Installation

The supported install path is the plugin **repository** described at the top of this page. Jellyfin resolves and writes the runtime `meta.json` and installs the plugin into `/config/plugins/Jellyflash/` automatically.

If a previous broken/manual install has already been recorded as disabled on the server:

1. With Jellyfin **stopped**, delete `/config/plugins/Jellyflash` (stopping the server avoids Windows file-lock errors).
2. Restart once to clear the poisoned state.
3. Then install from the Catalog (or a manual complete layout).

> **Manual layouts are unsupported.** Jellyfin plugins are self-contained packages: `Jellyflash.dll` plus every dependency DLL it links against (`lib/<abi>/...` from `Jellyflash.deps.json`) plus a runtime `meta.json`. Copying the raw `bin/Release/<abi>/` build output into `plugins/` causes Jellyfin to fail loading the plugin — it marks it *malfunctioning* and **recursively deletes the folder** on the next restart. Always install through the repository/Catalog.

After a valid install restart the server and proceed:

1. Add a directory of `.swf` files to your library (e.g., as an 'Other' or 'Movies' library type). Wait for the scan to finish.
2. Navigate to the item in the web interface and click **Play with Ruffle**.

## Manual JavaScript Injection (Fallback)

If the automatic `index.html` injection fails or gets wiped out by a Jellyfin update, you can manually inject the JavaScript using the community **Jellyfin-JavaScript-Injector** plugin.

1. In your Jellyfin dashboard, disable **Automatically Inject Javascript** in the Jellyflash plugin settings.
2. Install the **JavaScript Injector** plugin from the community repository.
3. Open the JavaScript Injector settings, and paste the entire contents of `Jellyflash/Web/ruffle-injector.js` into a new script block.
4. Save and refresh your web client. The Ruffle player will now be fully active!
