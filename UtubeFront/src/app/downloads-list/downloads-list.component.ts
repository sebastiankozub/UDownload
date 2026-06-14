import { Component, OnInit } from '@angular/core';
import { DownloadedFileItem, UtubeApiService } from '../service/utube.service';

@Component({
  selector: 'app-downloads-list',
  standalone: false,
  templateUrl: './downloads-list.component.html',
  styleUrl: './downloads-list.component.css'
})
export class DownloadsListComponent implements OnInit {
  files: DownloadedFileItem[] = [];
  loading = true;
  error: string | null = null;

  constructor(private utubeService: UtubeApiService) { }

  ngOnInit(): void {
    this.loadFiles();
  }

  loadFiles(): void {
    this.loading = true;
    this.error = null;

    this.utubeService.getDownloadedFiles().subscribe({
      next: files => {
        this.files = files;
        this.loading = false;
      },
      error: error => {
        console.error('Failed to load downloaded files:', error);
        this.error = error?.error?.detail ?? error?.error?.title ?? 'Failed to load downloaded files.';
        this.loading = false;
      }
    });
  }

  formatFileSize(sizeBytes: number): string {
    if (!sizeBytes || sizeBytes <= 0) {
      return '-';
    }

    const units = ['B', 'KB', 'MB', 'GB', 'TB'];
    let size = sizeBytes;
    let unitIndex = 0;

    while (size >= 1024 && unitIndex < units.length - 1) {
      size /= 1024;
      unitIndex++;
    }

    return `${size.toFixed(size >= 10 || unitIndex === 0 ? 0 : 1)} ${units[unitIndex]}`;
  }

  formatModifiedAt(value: string): string {
    if (!value) {
      return '-';
    }

    return new Date(value).toLocaleString();
  }
}
