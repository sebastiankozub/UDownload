# ?? Fixing Docker Volume Mount Issues

## Problem
After rebuilding with Dockerfile, the `/app/cookies` folder is not visible or not mounted in the container.

## Root Causes

### 1. Visual Studio Container Tools Caching
Visual Studio caches Docker images and may not rebuild the base stage where `/app/cookies` is created.

### 2. Fast Mode vs Regular Mode
- **Fast Mode (Debug)**: Uses the `base` stage
- **Regular Mode (Release)**: Uses the `final` stage
- Both stages now create `/app/cookies` directory

### 3. Volume Mount Not Applied
The `dockerRunArgs` in `launchSettings.json` might not be processed correctly.

## ? Solution Steps

### Step 1: Clean Docker Environment
```powershell
# Stop all running containers
docker stop $(docker ps -aq)

# Remove stopped containers and unused data
docker system prune -f

# Optional: Remove all Docker images (forces complete rebuild)
docker rmi $(docker images -q) -f
```

### Step 2: Clean Visual Studio Cache
In Visual Studio:
1. **Build** ? **Clean Solution**
2. **Build** ? **Rebuild Solution**
3. Close and reopen Visual Studio (if issues persist)

### Step 3: Verify Configuration Files

#### Check `launchSettings.json`:
```json
"Container (Dockerfile)": {
  "commandName": "Docker",
  "dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\"",
  // ... other settings
}
```

#### Check `Dockerfile` has both stages:
```dockerfile
# base stage (line ~19)
RUN mkdir -p /app/cookies && chown -R $APP_UID:$APP_UID /app/cookies

# final stage (line ~46)
RUN mkdir -p /app/cookies
```

### Step 4: Create Cookie File (Not Template)
```powershell
# Navigate to cookies directory
cd D:\repos\udownload\cookies

# Rename template or create actual cookie file
# DO NOT use youtube.txt.template - create youtube.txt
Copy-Item youtube.txt.template youtube.txt
# Or export real cookies from browser
```

### Step 5: Rebuild and Run from Visual Studio

1. **Select Profile**: Ensure "**Container (Dockerfile)**" is selected in debug dropdown
2. **Rebuild**: Press **Ctrl+Shift+B**
3. **Start**: Press **F5**
4. **Wait**: Let Visual Studio build the image (first time takes longer)

### Step 6: Verify Volume Mount

#### Option A: Docker Desktop GUI
1. Open Docker Desktop
2. Click **Containers** in left sidebar
3. Find **UtubeRest** container
4. Click on the container name
5. Go to **Bind mounts** tab
6. Should see: `D:\repos\udownload\cookies ? /app/cookies`

#### Option B: PowerShell Script
```powershell
# Run the verification script
.\verify-docker-volume.ps1
```

#### Option C: Manual Docker Command
```powershell
# Find container ID
$containerId = docker ps --filter "name=utuberest" --format "{{.ID}}"

# Check if /app/cookies exists
docker exec $containerId ls -la /app/cookies

# Check volume mounts
docker inspect $containerId | Select-String "cookies"
```

## ?? Common Issues and Fixes

### Issue 1: "Directory /app/cookies not found"

**Cause**: Dockerfile didn't create the directory in the stage being used.

**Fix**:
```dockerfile
# Add to FINAL stage (after line 45)
RUN mkdir -p /app/cookies
```

Then rebuild:
```powershell
docker rmi utuberest:dev -f
# Press F5 in Visual Studio
```

### Issue 2: "Volume mount path not found"

**Cause**: Host directory doesn't exist or path is wrong.

**Fix**:
```powershell
# Create directory
mkdir D:\repos\udownload\cookies

# Verify path
Get-Item D:\repos\udownload\cookies

# Check launchSettings.json uses correct path
# Should be: $(ProjectDir)..\\cookies
```

### Issue 3: "Permission denied" in container

**Cause**: User permissions mismatch.

**Fix**: The Dockerfile already handles this with:
```dockerfile
RUN mkdir -p /app/cookies && chown -R $APP_UID:$APP_UID /app/cookies
```

If still having issues:
```powershell
# Run as administrator
# Or check Docker Desktop file sharing settings
```

### Issue 4: "Mount point is empty" in container

**Cause**: Volume mount syntax error or Docker Desktop file sharing not enabled.

**Fix**:
1. Check Docker Desktop: **Settings** ? **Resources** ? **File Sharing**
2. Ensure **D:\\** drive is in the list
3. If not, click **+** and add it
4. Apply & Restart Docker Desktop

### Issue 5: Visual Studio shows old image

**Cause**: VS is using cached image.

**Fix**:
```powershell
# Force remove all utuberest images
docker images --filter "reference=*utuberest*" -q | ForEach-Object { docker rmi $_ -f }

# Rebuild in VS
# Build > Rebuild Solution
# Then F5
```

## ?? Testing After Fix

### Test 1: Check Directory Exists in Container
```powershell
$containerId = docker ps --filter "name=utuberest" --format "{{.ID}}"
docker exec $containerId ls -la /app
```

Expected output should include:
```
drwxr-xr-x 2 app app 4096 ... cookies
```

### Test 2: Check Files in Mounted Directory
```powershell
docker exec $containerId ls -la /app/cookies
```

Expected output:
```
total 8
drwxr-xr-x 2 app app 4096 ... .
drwxr-xr-x 1 app app 4096 ... ..
-rw-r--r-- 1 app app 1234 ... youtube.txt
```

### Test 3: Verify Application Can Read Cookies
Check Visual Studio Output window (Docker pane) when API is called.

Should see:
```
Launch command
yt-dlp https://www.youtube.com/watch?v=... --cookies /app/cookies/youtube.txt --dump-json
```

Should NOT see:
```
Warning: Cookies file not found at /app/cookies/youtube.txt
```

## ?? Quick Fix Checklist

- [ ] Cookies directory exists: `D:\repos\udownload\cookies\`
- [ ] Cookie file exists: `D:\repos\udownload\cookies\youtube.txt` (not .template)
- [ ] Docker cache cleaned: `docker system prune -f`
- [ ] Visual Studio solution rebuilt: **Build** ? **Rebuild Solution**
- [ ] Correct profile selected: "**Container (Dockerfile)**"
- [ ] Container is running: `docker ps` shows utuberest
- [ ] Volume is mounted: Check Docker Desktop or `docker inspect`
- [ ] Files visible in container: `docker exec <id> ls /app/cookies`

## ?? Full Clean Rebuild Process

If all else fails, do a complete clean rebuild:

```powershell
# 1. Stop Visual Studio

# 2. Clean everything
docker stop $(docker ps -aq)
docker rm $(docker ps -aq)
docker rmi $(docker images -q) -f
docker system prune -af --volumes

# 3. Ensure cookies directory and file exist
New-Item -Path "D:\repos\udownload\cookies" -ItemType Directory -Force
# Add your youtube.txt file here

# 4. Open Visual Studio

# 5. Clean Solution
# Build > Clean Solution

# 6. Rebuild Solution  
# Build > Rebuild Solution

# 7. Select "Container (Dockerfile)" profile

# 8. Press F5

# 9. Wait for build (will take several minutes)

# 10. Verify in Docker Desktop
```

## ?? Advanced Debugging

### View Docker Container Startup Command
Check Visual Studio Output window (**View** ? **Output**, select "Container Tools"):

Look for line starting with `docker run` - should include:
```
-v "D:\repos\udownload\cookies:/app/cookies:ro"
```

### Manual Docker Run (Outside VS)
Test if Docker volume works independently:

```powershell
# Build image manually
cd D:\repos\udownload
docker build -t utuberest-test -f UtubeRest/Dockerfile .

# Run with volume mount
docker run -it --rm `
  -v "D:\repos\udownload\cookies:/app/cookies:ro" `
  -p 5101:8080 `
  utuberest-test

# In another terminal, check files
docker exec -it $(docker ps -q --filter ancestor=utuberest-test) ls -la /app/cookies
```

If this works but VS doesn't, the issue is with Visual Studio configuration.

## ?? Still Not Working?

1. Run: `.\verify-docker-volume.ps1` and share output
2. Check: Visual Studio Output window (Container Tools pane)
3. Check: Docker Desktop containers ? UtubeRest ? Logs
4. Verify: Docker Desktop Settings ? Resources ? File Sharing includes D:\

## ? Success Indicators

When everything is working correctly:

1. ? Container starts without errors
2. ? Docker Desktop shows volume under "Bind mounts"
3. ? `docker exec <id> ls /app/cookies` shows your files
4. ? No "Warning: Cookies file not found" in logs
5. ? API calls to yt-dlp include `--cookies` parameter
6. ? YouTube downloads work with authentication
