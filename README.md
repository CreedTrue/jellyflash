# Jellyflash

A Jellyfin plugin that enables support for playing `.swf` Flash files natively in the web client using the open-source [Ruffle](https://ruffle.rs/) emulator.

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

Once built, the output will be written to `Jellyflash/bin/Release/<abi>/` (for example `net8.0` or `net9.0`).

## Installation

Jellyfin plugins are self-contained packages: a plugin folder must contain the plugin DLL **plus** every dependency DLL it links against (the `lib/<abi>/...` tree declared in `Jellyflash.deps.json`) **plus** a runtime `meta.json`. The raw build output is **not** a valid install — copying `bin/Release/<abi>/` straight into the plugins folder causes Jellyfin to fail loading the plugin, mark it *malfunctioning*, and then **recursively delete the folder** on the next restart.

Two supported installation methods:

### 1. Plugin repository / Catalog (recommended)

Publish the packaged plugin (using `Jellyflash/meta.json` as the manifest and `Jellyflash.deps.json` as the dependency list) to a Jellyfin plugin repository, then install it from the Jellyfin **Dashboard → Plugins → Catalog**. This resolves the `lib/` dependency DLLs and writes a correct runtime `meta.json` automatically.

### 2. Manual complete layout

If you must install manually, the `/config/plugins/Jellyflash/` folder must be fully self-contained and include every dependency DLL, for example:

```
Jellyflash/
├─ meta.json                         # runtime manifest (id, version, targetAbi, assemblies, ...)
└─ lib/net8.0/  (or the matching ABI)
   ├─ MediaBrowser.Controller.dll
   ├─ MediaBrowser.Model.dll
   ├─ Jellyfin.Common.dll
   └─ ... (every DLL referenced in Jellyflash.deps.json)
```

The bare build output only contains `Jellyflash.dll`/`.pdb`/`.deps.json`, so it must first be assembled into this complete layout. Deploying an incomplete folder is not supported and results in the fault-and-delete behavior described above.

If a previous broken install has already been recorded as disabled on the server:

1. With Jellyfin **stopped**, delete `/config/plugins/Jellyflash` (stopping the server avoids Windows file-lock errors).
2. Restart once to clear the poisoned state.
3. Then install a complete, self-contained folder (or the catalog package).

After a valid install restart the server and proceed:

1. Add a directory of `.swf` files to your library (e.g., as an 'Other' or 'Movies' library type). Wait for the scan to finish.
2. Navigate to the item in the web interface and click **Play with Ruffle**.

## Manual JavaScript Injection (Fallback)

If the automatic `index.html` injection fails or gets wiped out by a Jellyfin update, you can manually inject the JavaScript using the community **Jellyfin-JavaScript-Injector** plugin.

1. In your Jellyfin dashboard, disable **Automatically Inject Javascript** in the Jellyflash plugin settings.
2. Install the **JavaScript Injector** plugin from the community repository.
3. Open the JavaScript Injector settings, and paste the entire contents of `Jellyflash/Web/ruffle-injector.js` into a new script block.
4. Save and refresh your web client. The Ruffle player will now be fully active!
