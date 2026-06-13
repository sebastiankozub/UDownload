import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { StreamImportRequest, UtubeApiService } from '../service/utube.service';

export interface AvManifest {
  id: string;
  title: string;
  url: string;
  channel?: string;
  uploader?: string;
  thumbnail?: string;
  viewCount?: number;
  duration?: number;
  uploadDate: Date;
  description: string;
  keywords: string[];
  audioStreams: AudioStream[];
  videoStreams: VideoStream[];
}

export interface VideoStream extends AvStream {
  videoCodec: string;
  videoQuality: string;
  videoResolution: string;
}

export interface AudioStream extends AvStream {
  audioCodec: string;
  audioLanguage?: string;
  isAudioLanguageDefault?: boolean | string;
}

export interface AvStream {
  url: string;
  formatId: string;
  formatNote?: string;
  container: string;
  size: string;
  bitrate: string;
  hashId: string;
  friendlyName: string;
}

@Component({
  selector: 'file-quality-picker',
  standalone: false,
  templateUrl: './file-quality-picker.component.html',
  styleUrl: './file-quality-picker.component.css'
})
export class FileQualityPickerComponent {
  downloading = false;
  downloadError: string | null = null;
  downloadComplete = false;
  downloadInitSuccess = false;
  private lastQueuedSelectionKey: string | null = null;

  resourceId = '';
  videoStreams: VideoStream[] = [];
  audioStreams: AudioStream[] = [];
  avManifest?: AvManifest;

  selectedVideoFormatIds: string[] = [];
  selectedAudioFormatIds: string[] = [];

  constructor(
    private utubeService: UtubeApiService,
    private route: ActivatedRoute)
  {
    this.route.paramMap.subscribe(params => {
      const videoId = params.get('videoId');
      if (videoId) {
        this.resourceId = videoId;
        this.fetchAvManifest();
      }
    });
  }

  fetchAvManifest() {
    const trimmedId = this.resourceId.trim();
    if (!trimmedId) {
      return;
    }

    this.downloadError = null;
    this.downloadInitSuccess = false;
    this.downloadComplete = false;
    this.downloading = false;
    this.lastQueuedSelectionKey = null;
    this.avManifest = undefined;
    this.audioStreams = [];
    this.videoStreams = [];

    this.utubeService.fetchManifest(trimmedId)
      .subscribe((manifest: AvManifest) => {
        this.audioStreams = manifest.audioStreams.map(stream => ({
          ...stream,
          friendlyName: this.buildAudioFriendlyName(stream),
        }));

        this.videoStreams = manifest.videoStreams.map(stream => ({
          ...stream,
          friendlyName: this.buildVideoFriendlyName(stream),
        }));

        this.selectedAudioFormatIds = [];
        this.selectedVideoFormatIds = [];
        this.avManifest = manifest;
      }, error => {
        this.downloadError = error?.error?.detail ?? 'Failed to fetch manifest.';
      });
  }

  toggleAudioStream(formatId: string, checked: boolean) {
    this.downloadError = null;
    this.downloadInitSuccess = false;
    this.selectedAudioFormatIds = checked ? [formatId] : [];
    this.tryStartDownload();
  }

  toggleVideoStream(formatId: string, checked: boolean) {
    this.downloadError = null;
    this.downloadInitSuccess = false;
    this.selectedVideoFormatIds = checked ? [formatId] : [];
    this.tryStartDownload();
  }

  isAudioSelected(formatId: string): boolean {
    return this.selectedAudioFormatIds.includes(formatId);
  }

  isVideoSelected(formatId: string): boolean {
    return this.selectedVideoFormatIds.includes(formatId);
  }

  async downloadStreams() {
    const request = this.buildDownloadRequest();
    if (!request) {
      this.downloadError = 'Select exactly one audio stream and one video stream.';
      return;
    }

    const selectionKey = this.buildSelectionKey(request);
    if (this.downloading || this.lastQueuedSelectionKey === selectionKey) {
      return;
    }

    this.downloading = true;
    this.downloadError = null;
    this.downloadInitSuccess = false;

    this.utubeService.postDownload(request)
      .subscribe({
        next: (response) => {
          this.downloading = false;
          this.downloadInitSuccess = true;
          this.lastQueuedSelectionKey = selectionKey;
          console.log('Download initiated:', response);
        },
        error: (error) => {
          this.downloading = false;
          console.error('Error initiating download:', error);
          this.downloadError = error?.error?.detail
            ?? error?.error?.title
            ?? 'Failed to queue download.';
        }
      });
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

  private buildAudioFriendlyName(stream: AudioStream): string {
    return [
      stream.formatId,
      stream.formatNote,
      stream.audioCodec,
      stream.bitrate,
      stream.size,
    ].filter(Boolean).join(' | ');
  }

  private buildVideoFriendlyName(stream: VideoStream): string {
    return [
      stream.formatId,
      stream.formatNote,
      stream.videoQuality,
      stream.videoResolution,
      stream.videoCodec,
      stream.bitrate,
      stream.size,
    ].filter(Boolean).join(' | ');
  }

  private tryStartDownload() {
    if (this.selectedAudioFormatIds.length === 1 && this.selectedVideoFormatIds.length === 1) {
      void this.downloadStreams();
    }
  }

  private buildDownloadRequest(): StreamImportRequest | null {
    const videoId = this.avManifest?.id ?? this.resourceId.trim();

    if (!videoId
      || this.selectedAudioFormatIds.length !== 1
      || this.selectedVideoFormatIds.length !== 1) {
      return null;
    }

    return {
      videoId,
      audioFormatIds: [...this.selectedAudioFormatIds],
      videoFormatIds: [...this.selectedVideoFormatIds],
    };
  }

  private buildSelectionKey(request: StreamImportRequest): string {
    return [
      request.videoId,
      request.audioFormatIds[0],
      request.videoFormatIds[0],
    ].join(':');
  }
}
