import { Component, OnInit } from '@angular/core';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-video-display',
  templateUrl: './video-display.component.html',
  styleUrls: ['./video-display.component.css'],
  standalone: false
})
export class VideoDisplayComponent implements OnInit {

  videoUrl: string | null = null;
  loading: boolean = true;
  error: string | null = null;

  constructor() { }

  ngOnInit(): void {
    this.fetchVideo();
  }

  fetchVideo(): void {
    const videoPath = 'Подиум - Танцуй пока молодая.KX0QdxVdeFg.video.mp4';
    // Direct URL - browser will handle range requests automatically
    //this.videoUrl = `${environment.apiUrl}/Media/stream?path=${encodeURIComponent(videoPath)}`;
    this.videoUrl = `${environment.apiUrl}/Media/stream?path=${videoPath}`;
    this.loading = false;
  }

  onVideoCanPlay(): void {
    this.loading = false;
  }

  onVideoError(event: any): void {
    this.error = 'Failed to load video';
    this.loading = false;
    console.error('Video error:', event);
  }
}