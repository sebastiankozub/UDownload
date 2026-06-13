# yt-dlp search, filter, and sort notes

## Safe metadata-only commands

- Use `--dump-json` for JSON metadata without downloading.
- `--dump-json` simulates unless `--no-simulate` is used.
- `--print-json` don't simulate unless using `--simulate`.
- The accidental downloads in this repo came from using `--print-json`,  `--dump-json` is the correct one.

## Best search approaches

### 1. Simple search

Use `ytsearch:` when all you need is plain keyword search.

```bash
yt-dlp --dump-json "ytsearch20:sha la la la"
```

### 2. Advanced search

Prefer real YouTube search URLs for anything involving sort/filter combinations.

Why:

- current yt-dlp clearly supports `youtube:search_url`
- browser search URLs carry YouTube-side sort/filter state
- this path is better documented than `ytsearchdate`

Examples:

```bash
yt-dlp --dump-json "https://www.youtube.com/results?search_query=rammstein+concert"
```

```bash
yt-dlp --dump-json "https://www.youtube.com/results?search_query=python&sp=EgIQAg%253D%253D"
```

```bash
yt-dlp --dump-json "https://www.youtube.com/@AsapSCIENCE/search?query=relativity"
```

## Filtering options that matter most

### Date filters

```bash
--date 20240101
--dateafter now-3years
--datebefore today-2weeks
```

Accepted formats include `YYYYMMDD` and relative forms like `now-3years`, `today-14days`, `yesterday-2weeks`.

### Generic filters

Use `--match-filters` for most advanced filtering.

Examples:

```bash
--match-filters "duration >= 1800"
--match-filters "view_count >=? 100000"
--match-filters "like_count >=? 5000"
--match-filters "average_rating >=? 4.5"
--match-filters "title ~= '(?i)\brammstein concert\b'"
--match-filters "description ~= '(?i)live'"
--match-filters "!is_live"
```

Notes:

- regex-like matching is available inside `--match-filters`
- multiple conditions can be combined with `&`
- multiple `--match-filters` options are OR-ed together

### Break/stop behavior

Useful for long result sets:

```bash
--break-match-filters "upload_date < 20240101"
--break-on-existing
--max-downloads 100
--break-per-input
```

### Result slicing

```bash
-I 1:20
-I ::-1
--playlist-end 100
```

## Sorting / ordering

### Search result ordering

- `--playlist-reverse` reverses the received result order
- for date-oriented queries, reversing can be used to get oldest-first behavior if the source order is newest-first
- `--lazy-playlist` processes entries as they arrive, but disables `--playlist-reverse`

### Format sorting is different

- `-S` / `--format-sort` sorts **media formats**, not search results
- good for choosing download formats, not for ranking search hits

## What seems supported vs uncertain

### Clearly supported now

- `ytsearch:`
- YouTube search URLs (`youtube:search_url`)
- YouTube Music search URLs
- channel search URLs
- `--date*` filters
- `--match-filters`
- `--playlist-reverse`

### Uncertain / weakly documented

`ytsearchdate` appeared in:

- local example notes in this repo
- older yt-dlp issue discussions

But it was **not found** in the current bundled:

- `yt-dlp-help.txt`
- `yt-dlp-README.md`
- current `supportedsites.md` snapshot searched during this session
- current yt-dlp code snapshot searched during this session

Practical recommendation: prefer **search URLs** over `ytsearchdate`.

## Good practical recipes

### Simple search

```bash
yt-dlp --dump-json "ytsearch20:vengaboys"
```

### Search and keep only long videos from recent years

```bash
yt-dlp --js-runtime node  --cookies '/home/app/cookies/youtube.txt'  --dump-json --dateafter now-10years --match-filters "duration >= 1200" "ytsearch200:rammstein concert"
```

### Use browser-generated URL with YouTube-side sort/filter state

```bash
yt-dlp --dump-json "https://www.youtube.com/results?search_query=rammstein+concert&sp=..."
```

### Oldest-first exploration

```bash
yt-dlp --dump-json --playlist-reverse "https://www.youtube.com/results?search_query=rammstein+concert&sp=..."
```

## Recommended direction for this solution

1. Keep using `--dump-json` for metadata/search flows.
2. Keep `ytsearch:` for simple free-text search.
3. Add optional `--dateafter`, `--datebefore`, `--match-filters`, and result slicing options.
4. For advanced sort/filter UX, prefer passing real YouTube search URLs instead of relying on `ytsearchdate`.



### Other sample queries: 

 - https://www.youtube.com/results?baz=bar&search_query=youtube-dl+test+video&filters=video&lclk=video
 - https://www.youtube.com/results?search_query=python&sp=EgIQAg%253D%253D
 - https://www.youtube.com/results?q=test&sp=EgQIBBgB

 yt-dlp --dump-json "ytsearch20:sha la la la"
 yt-dlp --dump-json --dateafter now-10years "ytsearchdate100:intitle:\"vengaboys\""
 yt-dlp --dump-json --dateafter now-20years --playlist-reverse "ytsearchdate200:intitle:\"rammstein concert\""
 yt-dlp --dump-json --match-filters "duration >= 1200 & title ~= '(?i)concert'" "ytsearch100:rammstein"
 yt-dlp --dump-json --match-filters "average_rating >=? 4.5 & like_count >=? 5000" "ytsearch50:live performance"


### What the official docs do clearly support is:

 1. ytsearchN:query
 2. --dateafter / --datebefore
 3. --match-filters
 4. --playlist-reverse
Best “interesting” options for your app

 - ytsearchN: — normal search
 - ytsearchdateN: — date-driven search
 - --playlist-reverse — invert date/result order
 - --dateafter / --datebefore — time windows
 - --match-filters — strongest option overall; lets you combine: - title regex
 - description regex
 - duration
 - views
 - likes
 - average rating
 - live/non-live flags
 - -I / --playlist-items — page/slice results
 - --flat-playlist — faster/lighter search result extraction when you only need summary fields

Most useful pieces from the repo docs:

┌────────────────────────┬───────────────────────────────────────────────────────────────────┬─────────────────────────────────────────────────────────────────────────────────────────────┐
│ Need                   │ Best option                                                       │ Notes                                                                                       │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ Plain YouTube search   │ ytsearchN:query                                                   │ Search prefix support is called out in the README (yt-dlp-README.md:2265)                   │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ Date-sorted search     │ ytsearchdateN:query                                               │ Present in your repo examples (yt-dlp-interesting-commands.txt:22-30)                       │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ Oldest first           │ ytsearchdateN:query + --playlist-reverse                          │ --playlist-reverse is supported (yt-dlp-help.txt:130-132, yt-dlp-README.md:2396-2399)       │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ Regex-like filtering   │ --match-filters                                                   │ Supports regex matches on fields like title/description (yt-dlp-help.txt:90-95)             │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ Exact-ish title search │ query syntax like intitle:"rammstein concert"                     │ Shown in your examples (yt-dlp-interesting-commands.txt:30, 38, 49)                         │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ Date filtering         │ --date, --datebefore, --dateafter                                 │ Built-in (yt-dlp-help.txt:86-89)                                                            │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ View/rating thresholds │ --match-filters on view_count, like_count, average_rating         │ Fields exist in metadata (yt-dlp-README.md:1349-1354)                                       │
├────────────────────────┼───────────────────────────────────────────────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────┤
│ Limit result count     │ ytsearchN: or playlist slicing -I                                 │ -I/--playlist-items is supported (yt-dlp-help.txt:80-83)                                    │
└────────────────────────┴───────────────────────────────────────────────────────────────────┴─────────────────────────────────────────────────────────────────────────────────────────────┘

What works well

 1. Regex / pattern matching
 - Yes. The main tool is --match-filters.
 - Examples: - --match-filters "title ~= '(?i)\\bvan damme\\b'"
 - --match-filters "description ~= '(?i)concert'"
 2. Sort by date
 - Yes.
 - Use ytsearchdateN:... for date-oriented results.
 - Add --playlist-reverse if you want oldest first.
 - Your own examples already show this pattern.
 3. Filter by date
 - Yes.
 - --dateafter now-3years
 - --datebefore 20220101
 - --date today-2weeks
 4. Filter by duration / views / rating
 - Yes, via --match-filters.
 - Examples: - --match-filters "duration >= 1800"
 - --match-filters "view_count >=? 100000"
 - --match-filters "like_count >=? 5000"
 - --match-filters "average_rating >=? 4.5"

Important limitations

 - Sort by rating: I do not see a generic yt-dlp search-result sort switch for rating.
 You can filter by average_rating / like_count, but not tell yt-dlp “return results sorted by rating”.
 - Sort by size: -S/--format-sort is for media formats, not search results (yt-dlp-help.txt:250-253, yt-dlp-README.md:1608-1652).
 - Filter by size: --min-filesize / --max-filesize exist (yt-dlp-help.txt:84-85), but they are really download selection guards. They are less useful for search ranking, and filesize metadata may be incomplete or format-specific.

Best “interesting” options for your app

 - ytsearchN: — normal search
 - ytsearchdateN: — date-driven search
 - --playlist-reverse — invert date/result order
 - --dateafter / --datebefore — time windows
 - --match-filters — strongest option overall; lets you combine: - title regex
 - description regex
 - duration
 - views
 - likes
 - average rating
 - live/non-live flags
 - -I / --playlist-items — page/slice results
 - --flat-playlist — faster/lighter search result extraction when you only need summary fields

Good candidate commands

 yt-dlp --dump-json "ytsearch20:sha la la la"

 yt-dlp --dump-json --dateafter now-10years "ytsearchdate100:intitle:\"vengaboys\""

 yt-dlp --dump-json --dateafter now-20years --playlist-reverse "ytsearchdate200:intitle:\"rammstein concert\""

 yt-dlp --dump-json --match-filters "duration >= 1200 & title ~= '(?i)concert'" "ytsearch100:rammstein"

 yt-dlp --dump-json --match-filters "average_rating >=? 4.5 & like_count >=? 5000" "ytsearch50:live performance"

Practical recommendation: for your app, the best upgrade path is probably:

 1. keep --dump-json
 2. add optional ytsearchdateN: mode
 3. expose dateafter, datebefore, duration, title regex, and min views
 4. treat rating sort and size sort as app-side features after collecting JSON, not yt-dlp-side sorting

That will give you the most useful “advanced search” behavior with the least surprise.


