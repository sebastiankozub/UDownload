# YouTube Cookies Setup for Docker

This guide explains how to use YouTube cookies with yt-dlp in your Dockerized application.

## What is the correct cookie file format?

The cookie file must be in **Netscape HTTP Cookie File format** (also known as cookies.txt format).

Example format:
```
# Netscape HTTP Cookie File
# This is a generated file! Do not edit.
.youtube.com	TRUE	/	FALSE	1234567890	CONSENT	YES+
.youtube.com	TRUE	/	TRUE	1234567890	__Secure-1PAPISID	your_value_here
.youtube.com	TRUE	/	TRUE	1234567890	__Secure-1PSID	your_value_here
.youtube.com	TRUE	/	TRUE	1234567890	__Secure-3PAPISID	your_value_here
```

## How to export cookies from your browser

### Option 1: Browser Extension (Recommended)

1. **Chrome/Edge**: Install "Get cookies.txt LOCALLY" extension
2. **Firefox**: Install "cookies.txt" extension
3. Navigate to YouTube and make sure you're logged in
4. Click the extension icon and export cookies
5. Save the file as `youtube.txt`

### Option 2: Manual Export from Browser DevTools

1. Open YouTube in your browser (logged in)
2. Press F12 to open DevTools
3. Go to Application > Storage > Cookies > https://www.youtube.com
4. Copy the cookies (especially `__Secure-*PSID` and `__Secure-*PAPISID` cookies)
5. Format them according to Netscape format shown above

## Docker Setup

### Directory Structure

Your project should have this structure:
```
D:\repos\udownload\
??? cookies/
?   ??? youtube.txt          ? Your cookie file goes here
??? docker-compose.yml
??? docker-compose.override.yml
??? UtubeRest/
?   ??? Dockerfile
?   ??? appsettings.json
?   ??? ...
```

### Step 1: Create cookies directory

In your project root directory (`D:\repos\udownload\`), create a `cookies` folder:

```powershell
mkdir cookies
```

### Step 2: Add your cookie file

Place your exported `youtube.txt` file in the `cookies` folder:
```
D:\repos\udownload\cookies\youtube.txt
```

### Step 3: Docker Volume Configuration

The following files have been updated to support cookies:

#### `docker-compose.override.yml`
```yaml
volumes:
  - ./cookies:/app/cookies:ro
```

This mounts your local `cookies` folder to `/app/cookies` in the container (read-only).

#### `Dockerfile`
```dockerfile
RUN mkdir -p /app/cookies && chown -R $APP_UID:$APP_UID /app/cookies
```

This creates the cookies directory with proper permissions.

### Step 4: Configuration

The application is configured in `appsettings.json`:

```json
"YtDlp": {
  "CookiesFilePath": "/app/cookies/youtube.txt",
  "UseCookies": true
}
```

You can:
- Set `UseCookies` to `false` to disable cookie usage
- Change `CookiesFilePath` to use a different cookie file name

## Running with Docker

### Build and Run

```powershell
# Build the container
docker-compose build

# Start the application
docker-compose up
```

### Verify cookies are working

1. Check the container logs for any cookie-related warnings
2. The application will log: `Warning: Cookies file not found at /app/cookies/youtube.txt` if the file is missing
3. Test with a video URL that requires authentication

## Troubleshooting

### "Cookies file not found" warning

- Verify the file exists: `ls cookies/youtube.txt`
- Make sure the file is named exactly `youtube.txt` (or match the name in config)
- Check file permissions (should be readable)

### yt-dlp still fails to download

- Ensure cookies are up-to-date (they expire!)
- Re-export cookies from your browser
- Make sure you're logged into YouTube when exporting
- Verify the cookie file format is correct

### Permission issues in container

- The Dockerfile creates the directory with proper permissions
- The volume is mounted as read-only (`:ro`) for security
- If issues persist, check the container logs: `docker-compose logs`

## Security Notes

?? **Important**: Cookie files contain sensitive authentication data!

- Add `cookies/` to your `.gitignore` file
- Never commit cookie files to version control
- Cookies expire and need to be refreshed periodically
- Keep your cookie files secure

## Configuration Options

### Disable cookies temporarily

In `appsettings.Development.json`, you can override the setting:

```json
{
  "YtDlp": {
    "UseCookies": false
  }
}
```

### Use a different cookie file

Change the path in configuration:

```json
{
  "YtDlp": {
    "CookiesFilePath": "/app/cookies/my-cookies.txt"
  }
}
```

## How it works

1. The `YtService` class reads configuration from `appsettings.json`
2. When calling yt-dlp, it automatically adds `--cookies /app/cookies/youtube.txt`
3. yt-dlp uses these cookies to authenticate with YouTube
4. This allows downloading age-restricted, private, or region-locked content

## Example yt-dlp command

Without cookies:
```bash
yt-dlp https://www.youtube.com/watch?v=VIDEO_ID --dump-json
```

With cookies (automatic):
```bash
yt-dlp https://www.youtube.com/watch?v=VIDEO_ID --cookies /app/cookies/youtube.txt --dump-json
```
