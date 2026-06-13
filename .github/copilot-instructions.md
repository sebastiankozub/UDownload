# Copilot instructions for UDownload

## Project purpose

UDownload is a YouTube helper application with:

- **`UtubeRest`**: ASP.NET Core 8 backend API.
- **`UtubeFront`**: Angular 19 frontend.
- **`UtubeFunctionApp`** and **`NewFunctionAppCsproj`**: experimental or legacy Azure Functions projects, not the main product path.

The intended MVP is:

1. Search YouTube videos.
2. Inspect downloadable media options.
3. Download chosen video or audio with `yt-dlp`.
4. Store downloaded files in the mounted download volume.
5. List downloaded files from that volume.
6. Play video or audio from the backend through the frontend.

## Current technical direction

- **Use `yt-dlp` for YouTube integration.**
- `yt-dlp` commands (print --help) in path: (solution folder)/yt-dlp-full-help.txt, interesting commands in (solution folder)/yt-dlp-interesting-commands.txt
- Do **not** add new `YoutubeExplode`-based flows unless explicitly asked.
- Existing `YoutubeExplode` endpoints are legacy and should be considered migration targets.
- Prefer extending `YtService` for new `yt-dlp` functionality instead of building shell commands directly in controllers.

## Authentication / YouTube access

Authentication for restricted or rate-limited YouTube access is handled through a browser-exported cookies file:

- Export cookies from a browser while logged into YouTube.
- Save them in **Netscape cookies.txt format**.
- Place the file at `cookies\youtube.txt` in the repository root.
- In Docker, the file is mounted to `/home/app/cookies/youtube.txt`.
- `YtService` appends `--cookies <path>` when cookies are enabled and the file exists.

This is the current approach for accessing content that may require logged-in browser state.

## Active backend endpoints and status

### Working or mostly working

- **`GET /api/Search?q=...&count=...`**
  - Uses `yt-dlp` search.
  - Returns lightweight `{ id, title }` style results.

- **`GET /api/Values`**
  - Diagnostic endpoint.
  - Checks `ffmpeg`, `yt-dlp`, and returns a sample manifest.
  - Treat as a debug/smoke-test endpoint, not final product API.

- **`POST /api/Values/download`**
  - Uses `yt-dlp`.
  - Downloads by URL.
  - Supports merged output and separate audio/video output.

- **`POST /api/Values/download/video`**
  - Uses `yt-dlp`.
  - Downloads a single URL or video ID using the backend download volume.

- **`GET /api/Media/stream?path=...`**
  - Streams local files from the mounted download directory.
  - Supports HTTP range requests, so browsers can play media.

### Working but legacy / migration target

- **`GET /api/AvManifest/{avResourceId}`**
  - Currently used by the Angular stream picker.
  - Uses `YoutubeExplode`, not `yt-dlp`.
  - This endpoint should be replaced or refactored to a `yt-dlp`-based manifest/details flow.

### Stubbed, placeholder, or incomplete

- **`GET /api/AvManifest`**
  - Throws `NotImplementedException`.

- **`POST /api/StreamStorage/Import`**
  - Intended to accept selected stream hash IDs and trigger queued downloads/import.
  - Currently only returns `202 Accepted` and does not persist or execute the workflow.

- **`/api/AvManifestEndpoint`**
  - Minimal API placeholder with dummy data.

- **`/api/OldAvManifest`**
  - Legacy duplicate controller using `YoutubeExplode`.

- Boilerplate/template endpoints such as WeatherForecast and generic Values CRUD methods
  - Not part of the intended product flow.

## Frontend status

### Existing screens/components

- **File quality picker**
  - Calls `GET /api/AvManifest/{id}`.
  - Displays audio and video streams.
  - Sends selected stream hash IDs to `POST /api/StreamStorage/Import`.
  - Current blocker: the backend import/download pipeline is not implemented.

- **Video display**
  - Uses `GET /api/Media/stream?path=...`.
  - Can play a hardcoded downloaded media file.

### Missing frontend features for MVP

- Search UI wired to `GET /api/Search`.
- Download-by-URL UI for the existing `yt-dlp` endpoints.
- File list UI for downloaded items in the volume.
- Audio-focused playback/listening flow.
- Frontend integration with a future `yt-dlp`-based manifest/details endpoint.

## Important implementation gaps

The most important missing backend work is:

1. Replace the `YoutubeExplode` manifest flow with `yt-dlp`.
2. Implement a real selected-stream download/import flow behind `POST /api/StreamStorage/Import`.
3. Add an endpoint to list downloaded files from the mounted download volume.
4. Add playback support for both video and audio files through the frontend.

## Infrastructure notes

- Backend paths currently assume Linux-style container paths such as `/home/app/downloads` and `/home/app/cookies/youtube.txt`.
- The repository uses Docker volume mounts for `cookies` and `download`.
- Be careful with the Docker volume mode for `download`: if mounted read-only, `yt-dlp` downloads inside the container will fail.

## Guidance for future Copilot changes

- Prefer surgical changes inside `UtubeRest` and `UtubeFront`.
- Reuse existing service patterns before adding new helpers.
- Keep new product work aligned to the MVP above.
- If implementing YouTube metadata/manifest functionality, use or update existing or create new `yt-dlp` output models rather than expanding `YoutubeExplode` usage.
