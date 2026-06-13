import { Component, NgZone, OnDestroy } from '@angular/core';
import { UtubeApiService } from '../service/utube.service';

interface SearchResultItem {
  id: string;
  title: string;
  duration?: number;
  uploader?: string;
  channel?: string;
  viewCount?: number;
  webpageUrl?: string;
  thumbnail?: string;
}

@Component({
  selector: 'app-search-stream',
  templateUrl: './search-stream.component.html',
  standalone: false
})
export class SearchStreamComponent implements OnDestroy {
  query = `can't touch this`;
  count = 20;
  results: SearchResultItem[] = [];
  statusMessage = 'Ready to search.';
  isSearching = false;

  private eventSource?: EventSource;

  constructor(
    private utubeService: UtubeApiService,
    private ngZone: NgZone) {}

  startSearch() {
    const trimmedQuery = this.query.trim();
    if (!trimmedQuery) {
      this.statusMessage = 'Enter a search phrase first.';
      return;
    }

    this.stopSearch(false);
    this.results = [];
    this.isSearching = true;
    this.statusMessage = 'Searching...';

    const streamUrl = this.utubeService.getSearchStreamUrl(trimmedQuery, this.count);
    const eventSource = new EventSource(streamUrl);
    this.eventSource = eventSource;

    eventSource.onmessage = (event) => {
      this.ngZone.run(() => {
        const rawItem = JSON.parse(event.data) as Partial<SearchResultItem> & {
          Id?: string;
          Title?: string;
          ViewCount?: number;
          view_count?: number;
          webpage_url?: string;
        };
        const item: SearchResultItem = {
          id: rawItem.id ?? rawItem.Id ?? '',
          title: rawItem.title ?? rawItem.Title ?? '',
          duration: rawItem.duration,
          uploader: rawItem.uploader,
          channel: rawItem.channel,
          viewCount: rawItem.viewCount ?? rawItem.ViewCount ?? rawItem.view_count,
          webpageUrl: rawItem.webpageUrl ?? rawItem.webpage_url,
          thumbnail: rawItem.thumbnail,
        };
        this.results = [...this.results, item];
        this.statusMessage = `Received ${this.results.length} result(s)...`;
      });
    };

    eventSource.addEventListener('done', () => {
      this.ngZone.run(() => {
        this.isSearching = false;
        this.statusMessage = `Done. Received ${this.results.length} result(s).`;
        this.closeStream();
      });
    });

    eventSource.onerror = () => {
      this.ngZone.run(() => {
        if (!this.isSearching) {
          return;
        }

        this.isSearching = false;
        this.statusMessage = this.results.length > 0
          ? `Stream closed after ${this.results.length} result(s).`
          : 'Search stream failed.';
        this.closeStream();
      });
    };
  }

  stopSearch(updateStatus = true) {
    if (updateStatus && this.isSearching) {
      this.statusMessage = `Stopped after ${this.results.length} result(s).`;
    }

    this.isSearching = false;
    this.closeStream();
  }

  ngOnDestroy(): void {
    this.closeStream();
  }

  formatDuration(seconds?: number): string {
    if (!seconds || seconds <= 0) {
      return '-';
    }

    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const remainingSeconds = seconds % 60;

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`;
    }

    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  formatViews(viewCount?: number): string {
    if (!viewCount || viewCount <= 0) {
      return '-';
    }

    return viewCount.toLocaleString();
  }

  private closeStream() {
    this.eventSource?.close();
    this.eventSource = undefined;
  }
}
