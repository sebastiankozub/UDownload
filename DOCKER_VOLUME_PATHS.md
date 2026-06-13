# Docker Volume Path Reference

## Current Configuration

The project uses **relative paths** in `launchSettings.json` for portable configuration.

## Path Variables

| Variable | Resolves To | Example |
|----------|-------------|---------|
| `$(ProjectDir)` | UtubeRest project directory | `D:\repos\udownload\UtubeRest\` |
| `$(ProjectDir)..` | Solution root directory | `D:\repos\udownload\` |
| `$(ProjectDir)..\\cookies` | Cookies at solution root | `D:\repos\udownload\cookies\` |

## Volume Mount Syntax

```json
"dockerRunArgs": "-v \"<host-path>:<container-path>:<mode>\""
```

- **host-path**: Where files are on your machine
- **container-path**: Where files appear in Docker
- **mode**: `ro` (read-only) or `rw` (read-write)

## Common Patterns

### Solution Root (Current Setup)
```json
"dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\""
```

**Structure:**
```
udownload\
??? cookies\           ? Host location
?   ??? youtube.txt
??? UtubeRest\
    ??? ...
```

### Project Root
```json
"dockerRunArgs": "-v \"$(ProjectDir)cookies:/app/cookies:ro\""
```

**Structure:**
```
udownload\
??? UtubeRest\
    ??? cookies\       ? Host location
    ?   ??? youtube.txt
    ??? ...
```

### Custom Location
```json
"dockerRunArgs": "-v \"$(ProjectDir)..\\..\\shared-cookies:/app/cookies:ro\""
```

**Structure:**
```
repos\
??? shared-cookies\    ? Host location
?   ??? youtube.txt
??? udownload\
    ??? UtubeRest\
        ??? ...
```

## Multiple Volumes

You can mount multiple directories:

```json
"dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\" -v \"$(ProjectDir)..\\data:/app/data:rw\""
```

This mounts:
- `cookies` folder as read-only at `/app/cookies`
- `data` folder as read-write at `/app/data`

## Read-Only vs Read-Write

| Mode | Flag | Description | Use Case |
|------|------|-------------|----------|
| Read-Only | `:ro` | Container cannot modify files | Configuration, secrets, cookies |
| Read-Write | `:rw` | Container can modify files | Logs, temp files, uploads |

**Security Best Practice**: Use `:ro` for sensitive data like cookies.

## Platform Differences

### Windows
```json
"dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\""
```
Use double backslashes: `\\`

### Linux/Mac (if needed)
```json
"dockerRunArgs": "-v \"$(ProjectDir)../cookies:/app/cookies:ro\""
```
Use forward slashes: `/`

Visual Studio handles this automatically on Windows.

## Verification Commands

### Check if path resolves correctly
In Visual Studio Output window (Docker pane), look for:
```
docker run ... -v "D:\repos\udownload\cookies:/app/cookies:ro" ...
```

### Verify inside container
```bash
docker exec -it <container-id> ls -la /app/cookies
```

Should show:
```
total 8
drwxr-xr-x 2 app app 4096 ... .
drwxr-xr-x 1 app app 4096 ... ..
-rw-r--r-- 1 app app 1234 ... youtube.txt
```

## Troubleshooting

### Path not found
- Ensure `cookies` directory exists: `D:\repos\udownload\cookies\`
- Create it: `mkdir D:\repos\udownload\cookies`

### Permission denied
- Check Docker Desktop file sharing settings
- Ensure D:\ drive is shared (Settings ? Resources ? File Sharing)

### Changes not reflected
- Read-only volumes require container restart to pick up changes
- Stop (Shift+F5) and start (F5) in Visual Studio

## Quick Setup Checklist

- [ ] Cookies directory exists at solution root
- [ ] `youtube.txt` file is in cookies directory
- [ ] `launchSettings.json` has correct `dockerRunArgs`
- [ ] Path uses `$(ProjectDir)..\\cookies` for portability
- [ ] Volume mode is `:ro` for security
- [ ] Build successful
- [ ] Container starts and mounts volume
- [ ] API can read cookies

## Further Reading

- [Docker Volumes Documentation](https://docs.docker.com/storage/volumes/)
- [Visual Studio Container Tools](https://docs.microsoft.com/visualstudio/containers/)
- [launchSettings.json Reference](https://docs.microsoft.com/aspnet/core/fundamentals/environments)
