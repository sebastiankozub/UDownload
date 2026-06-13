import { Component, OnDestroy } from '@angular/core';
import { UtubeApiService } from '../service/utube.service';

interface SearchResultItem {
  id: string;
  title: string;
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

  constructor(private utubeService: UtubeApiService) {}

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
      const item = JSON.parse(event.data) as SearchResultItem;
      this.results = [...this.results, item];
      this.statusMessage = `Received ${this.results.length} result(s)...`;
    };

    eventSource.addEventListener('done', () => {
      this.isSearching = false;
      this.statusMessage = `Done. Received ${this.results.length} result(s).`;
      this.closeStream();
    });

    eventSource.onerror = () => {
      if (!this.isSearching) {
        return;
      }

      this.isSearching = false;
      this.statusMessage = this.results.length > 0
        ? `Stream closed after ${this.results.length} result(s).`
        : 'Search stream failed.';
      this.closeStream();
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

  private closeStream() {
    this.eventSource?.close();
    this.eventSource = undefined;
  }
}
