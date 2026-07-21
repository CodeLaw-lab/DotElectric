---
name: devops-engineering
description: DevOps Engineer — configures CI/CD (GitHub Actions), builds installer (WiX), manages releases, versioning, changelog, code signing, and auto-update.
---

# DevOps Engineering — CI/CD и релизы

Ты — DevOps Engineer для .NET/WPF. Используй этот skill для настройки CI/CD, сборки инсталляторов и управления релизами.

## GitHub Actions — шаблоны

### CI: build + test + coverage

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
      - run: dotnet build src/DotElectric.TemplateEditor.slnx --configuration Release
      - run: dotnet test src/DotElectric.TemplateEditor.Tests --configuration Release --collect:"XPlat Code Coverage"
      - run: dotnet tool install -g dotnet-reportgenerator-globaltool
      - run: reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:TextSummary
```

### Release workflow

```yaml
name: Release

on:
  push:
    tags: ['v*.*.*']

jobs:
  release:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet publish src/DotElectric.TemplateEditor --configuration Release
      - name: Build installer
        run: |
          & "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" installer.wxs
          & "C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" installer.wixobj
      - uses: softprops/action-gh-release@v2
        with:
          files: |
            *.msi
            publish/*.zip
          generate_release_notes: true
```

## WiX installer — шаблон

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="DotElectric TemplateEditor"
           Language="1033" Version="!(bind.FileVersion.MainExe)"
           Manufacturer="DotElectric">
    <Package InstallerVersion="200" Compressed="yes"/>
    <Media Id="1" Cabinet="product.cab" EmbedCab="yes"/>
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFiles64Folder">
        <Directory Id="INSTALLDIR" Name="DotElectric">
          <Component Id="MainExe" Guid="*">
            <File Id="MainExe" Name="DotElectric.TemplateEditor.exe"
                  KeyPath="yes" DiskId="1"/>
          </Component>
        </Directory>
      </Directory>
    </Directory>
  </Product>
</Wix>
```

## Version bump (PowerShell)

```powershell
# bump-version.ps1
param([string]$newVersion)
$csproj = "src/DotElectric.TemplateEditor/DotElectric.TemplateEditor.csproj"
(Get-Content $csproj) -replace '<Version>.*?</Version>', "<Version>$newVersion</Version>" | Set-Content $csproj
```

## Code signing

```powershell
# sign.ps1 (запускать только в CI с секретами)
$cert = ConvertFrom-SecureString -SecureString $env:CODE_SIGN_CERT
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe" sign /fd SHA256 /f $cert /p $env:CODE_SIGN_PASSWORD /t http://timestamp.digicert.com /v output.exe
```

## Auto-update (Velopack)

```csharp
// App.xaml.cs — при старте
using Velopack;

protected override async void OnStartup(StartupEventArgs e)
{
    VelopackApp.Build()
        .SetAutoApplyOnStartup(true)
        .Run();
    // ... остальной код
}
```

## Local dev setup

```yaml
# docker-compose.yml (future DB)
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: dotelectric
      POSTGRES_PASSWORD: devpassword
    ports:
      - "5432:5432"
```

## Troubleshooting

| Проблема | Решение |
|----------|---------|
| NuGet cache miss в CI | Проверь `hashFiles('**/*.csproj')` — если lock файл не включён, cache сбрасывается |
| CodeQL не находит .NET 10 | Используй `runner: windows-latest` и `setup-dotnet@v4` |
| WiX candle не найден | Установи WiX Toolset через `choco install wixtoolset` |
| SignTool Access denied | Сертификат должен быть в Base64, не PFX с паролем |
