# Running with Visual Studio Docker Profile

This guide shows how to run the UtubeRest project with cookies support using Visual Studio's "Container (Dockerfile)" launch profile.

## Prerequisites

1. **Create cookies directory** in your solution root (not in UtubeRest project folder):
   ```
   <solution-root>\cookies\
   ```
   
   For this project:
   ```
   D:\repos\udownload\cookies\
   ```

2. **Add your YouTube cookie file** as `youtube.txt`:
   ```
   <solution-root>\cookies\youtube.txt
   ```

   See [COOKIES_SETUP.md](COOKIES_SETUP.md) for how to export cookies from your browser.

### Directory Structure:
```
D:\repos\udownload\
??? cookies\
?   ??? youtube.txt          ? Your cookie file
??? UtubeRest\
?   ??? Properties\
?   ?   ??? launchSettings.json
?   ??? ...
??? ...
```

## Configuration

The `launchSettings.json` has been configured with a relative volume mount:

```json
"Container (Dockerfile)": {
  "commandName": "Docker",
  "dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\"",
  // ... other settings
}
```

### Volume Mount Explanation:
- **Host Path**: `$(ProjectDir)..\\cookies` - Relative to the UtubeRest project directory (solution root/cookies)
- **Container Path**: `/app/cookies` - Where cookies appear inside the container
- **Mode**: `:ro` (read-only) - Container cannot modify your cookie files

### Path Resolution:
- `$(ProjectDir)` = `D:\repos\udownload\UtubeRest\`
- `$(ProjectDir)..` = `D:\repos\udownload\`
- `$(ProjectDir)..\\cookies` = `D:\repos\udownload\cookies\`

This makes the configuration portable across different machines and user directories.

## Running from Visual Studio

### Method 1: Using the Toolbar
1. Open Visual Studio
2. Open `UtubeRest.sln`
3. In the toolbar, select **"Container (Dockerfile)"** from the debug target dropdown
4. Press **F5** (or click the green play button)
5. Visual Studio will:
   - Build the Docker image
   - Start the container with volume mounted
   - Open your browser to Swagger UI

### Method 2: Using Debug Menu
1. **Debug** ? **Start Debugging**
2. Ensure "Container (Dockerfile)" profile is selected

## Verify Cookies are Mounted

### Check in Docker Desktop
1. Open Docker Desktop
2. Navigate to **Containers**
3. Click on the running **UtubeRest** container
4. Go to the **Bind mounts** tab
5. You should see something like:
   ```
   D:\repos\udownload\cookies ? /app/cookies
   ```
   (The exact host path will match your solution location)

### Check in Container Logs
Look for warnings in the Visual Studio Output window (Docker pane):
- ? No warning = Cookie file found and loaded
- ?? "Warning: Cookies file not found" = File is missing or path is wrong

### Manually Verify
You can also inspect the running container:

```powershell
# Find your container ID
docker ps

# Execute bash in the container
docker exec -it <container-id> bash

# Check if cookies directory exists
ls -la /app/cookies

# Check if your cookie file is there
cat /app/cookies/youtube.txt
```

## Troubleshooting

### Issue: "Volume mount failed" or "Path not found"

**Solution**: The path is now relative and should work automatically. Visual Studio will resolve `$(ProjectDir)..\\cookies` to your solution root.

If you still have issues:
1. Verify the cookies directory exists in your **solution root** (not project root)
2. Expected location: `D:\repos\udownload\cookies\` (same level as the solution file)
3. Check Visual Studio Output window (Docker pane) for exact error messages

### Issue: Using a different directory structure

If your cookies are in a different location, you can adjust the path:

```json
// Cookies at solution root (default)
"dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\""

// Cookies inside project folder
"dockerRunArgs": "-v \"$(ProjectDir)cookies:/app/cookies:ro\""

// Cookies in a custom location
"dockerRunArgs": "-v \"$(ProjectDir)..\\..\\my-cookies:/app/cookies:ro\""
```

### Issue: Container starts but cookies not working

**Checklist**:
- ? Cookie file exists at `D:\repos\udownload\cookies\youtube.txt`
- ? Cookie file is in Netscape format (not JSON or other format)
- ? Cookies are fresh (not expired)
- ? You're logged into YouTube when exporting cookies

### Issue: Permission denied

On Windows with Docker Desktop, this is rare, but if it happens:
1. Check Docker Desktop settings
2. Ensure "D:\" drive is shared with Docker (Settings ? Resources ? File Sharing)
3. Remove `:ro` (read-only) flag temporarily to test

### Issue: Changes to cookies not reflected

**Solution**: The volume mount is read-only. To update cookies:
1. Stop the container (Shift+F5 in Visual Studio)
2. Replace the cookie file on your host machine
3. Restart the container (F5)

## Alternative: Dynamic Path (Optional)

**Current Setup (Recommended)**: The configuration already uses a relative path with `$(ProjectDir)..\\cookies`

This makes the path portable and works on any machine where the project is cloned.

### Other Path Options:

```json
// Solution root cookies folder (current setup)
"dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\""

// Project-specific cookies folder
"dockerRunArgs": "-v \"$(ProjectDir)cookies:/app/cookies:ro\""

// Two levels up (if solution is nested deeper)
"dockerRunArgs": "-v \"$(ProjectDir)..\\..\\cookies:/app/cookies:ro\""

// Multiple volumes (advanced)
"dockerRunArgs": "-v \"$(ProjectDir)..\\cookies:/app/cookies:ro\" -v \"$(ProjectDir)..\\data:/app/data\""
```

### Benefits of Relative Paths:
- ? Works on any developer machine
- ? Works in CI/CD pipelines  
- ? No need to update paths when cloning repo
- ? Compatible with team collaboration

## Testing the API with Cookies

Once running, test your API:

### Get Video Manifest (No Auth Required)
```http
GET https://localhost:7101/api/Values
```

This should successfully fetch video metadata using your YouTube cookies.

### Check YtDlp Version
The service logs should show yt-dlp being called with cookies:
```
Launch command
yt-dlp https://www.youtube.com/watch?v=VIDEO_ID --cookies /app/cookies/youtube.txt --dump-json
```

## Port Configuration

The container exposes:
- **HTTP**: `localhost:5101`
- **HTTPS**: `localhost:7101`
- **Swagger UI**: Available at both ports with `/swagger` path

## Next Steps

1. ? Verify container is running in Docker Desktop
2. ? Check cookies are mounted via "Bind mounts" tab
3. ? Test API endpoints via Swagger
4. ? Monitor logs for any cookie-related warnings

## Security Reminder

?? The cookies directory is in `.gitignore` and won't be committed to Git. Keep your cookie files secure and refresh them regularly as they expire.
