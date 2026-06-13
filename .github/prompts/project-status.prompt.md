# Project status and MVP prompt

Use this prompt when you need a quick project-aware summary before coding.

Review the current UDownload repository and answer using these rules:

1. Treat `UtubeRest` as the main backend and `UtubeFront` as the main frontend.
2. Treat `yt-dlp` as the preferred YouTube integration path.
3. Treat `YoutubeExplode` code as legacy but do not delete it and avoid editing. There will be no need for that
4. Summarize:
   - implemented endpoints and its usage by frontend,
   - incomplete or placeholder endpoints,
   - frontend screens and what they currently call,
   - the current authentication approach for YouTube access (few words about cookies only if not changed last commits),
   - the next missing pieces required for the MVP.
   - used versions of dotnet, Angular, yt-dlp and other tools.
5. Keep the answer practical and implementation-oriented.


The intended MVP is:

- search YouTube for audio & video streams,
- inspect or choose media options,
- download chosen video or audio or suggest merging,
- manage video, audio & merged av streams on the mounted volume,
- list downloaded files from the volume,
- play downloaded media from the backend in the frontend.
