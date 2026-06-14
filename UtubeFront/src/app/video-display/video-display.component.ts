import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UtubeApiService } from '../service/utube.service';

@Component({
  selector: 'app-video-display',
  templateUrl: './video-display.component.html',
  styleUrls: ['./video-display.component.css'],
  standalone: false
})
export class VideoDisplayComponent implements OnInit {

  mediaUrl: string | null = null;
  mediaPath: string | null = null;
  isVideo = true;
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private utubeService: UtubeApiService) { }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.loadMedia(params.get('path'));
    });
  }

  loadMedia(path: string | null): void {
    this.loading = true;
    this.error = null;
    this.mediaUrl = null;
    this.mediaPath = path;

    if (!path) {
      this.error = 'Missing file path.';
      this.loading = false;
      return;
    }

    this.isVideo = !this.isAudioFile(path);
    this.mediaUrl = this.utubeService.getStreamUrl(path);
    this.loading = false;
  }

  onMediaCanPlay(): void {
    this.loading = false;
  }

  onMediaError(event: Event): void {
    this.error = 'Failed to load media.';
    this.loading = false;
    console.error('Media error:', event);
  }

  private isAudioFile(path: string): boolean {
    const lowerPath = path.toLowerCase();
    return lowerPath.endsWith('.mp3') || lowerPath.endsWith('.wav') || lowerPath.endsWith('.m4a');
  }
}