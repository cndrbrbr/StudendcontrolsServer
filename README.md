# Minecraft SSH Client for Windows

Kleine Windows-Desktop-App für deine drei Server-Kommandos:

- `start`
- `stop`
- `version <minecraftversion>`

## Voraussetzungen

- Windows 10/11
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

- Server: Hostname oder IP
- User: SSH-User
- Private Key: Pfad zur privaten Key-Datei
- `Start` sendet `start`
- `Stop` sendet `stop`
- `Set Version` sendet `version <Wert im Feld>`

## Sicherheit

- Die App nutzt den Windows-OpenSSH-Client.
- Der Server sollte den Key weiter auf deinen Wrapper begrenzen.
- Private Keys nicht unverschlüsselt verteilen.
