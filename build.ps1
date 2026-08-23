$ErrorActionPreference = "Stop"

Write-Host "Building Jellyflash Plugin using Docker..."
docker run --rm -v "$($PWD.Path)/Jellyflash:/app" -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host "Build output: Jellyflash/bin/Release/net9.0/" -ForegroundColor Green
    Write-Host "NOTE: This is raw build output, not a self-contained install." -ForegroundColor Yellow
    Write-Host "Install via a plugin repo/Catalog, or assemble Jellyflash.dll + all dependency DLLs" -ForegroundColor Yellow
    Write-Host "(lib/<abi>/ tree from Jellyflash.deps.json) + runtime meta.json into the plugins folder." -ForegroundColor Yellow
} else {
    Write-Host "Build failed." -ForegroundColor Red
}
