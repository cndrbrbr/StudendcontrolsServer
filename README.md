# Minecraft SSH Client for Windows

Kleine Windows-Desktop-App für diese Server-Kommandos:

- `start`
- `stop`
- `version <minecraftversion>`

## Vorgaben in dieser Version

- **Username ist fest auf `mc-ctrl`**
- **SSH-Port ist variabel**
- **Private-Key-Pfad ist variabel**

## Voraussetzungen

- Windows 10 oder 11
- OpenSSH Client installiert (`ssh.exe`)
- .NET 8 SDK zum Bauen

## Build

In PowerShell im Projektordner:

```powershell
dotnet publish .\MinecraftSshClient\MinecraftSshClient.csproj -c Release -r win-x64 --self-contained true
```

Danach liegt die EXE hier:

```text
MinecraftSshClient\bin\Release\net8.0-windows\win-x64\publish\MinecraftSshClient.exe
```

## Nutzung

- **Server**: Hostname oder IP
- **Port**: SSH-Port, z. B. `22` oder `2222`
- **User**: fest `mc-ctrl`
- **Private Key**: Pfad zur privaten Key-Datei
- **Start** sendet `start`
- **Stop** sendet `stop`
- **Set Version** sendet `version <Wert im Feld>`

## Hinweise

- Die App nutzt den Windows-OpenSSH-Client.
- Der Host-Key wird mit `StrictHostKeyChecking=accept-new` beim ersten Kontakt übernommen.
- Der Server sollte den Key weiter auf deinen Wrapper begrenzen.
- Private Keys nicht unverschlüsselt verteilen.
