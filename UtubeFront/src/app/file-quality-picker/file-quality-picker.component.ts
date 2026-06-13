import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UtubeApiService } from '../service/utube.service';

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

  resourceId = '';
  videoStreams: VideoStream[] = [];
  audioStreams: AudioStream[] = [];
  avManifest?: AvManifest;

  selectedVideoStreamsHashIds: string[] = [];
  selectedAudioStreamsHashIds: string[] = [];

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

        this.selectedAudioStreamsHashIds = [];
        this.selectedVideoStreamsHashIds = [];
        this.avManifest = manifest;
      }, error => {
        this.downloadError = error?.error?.detail ?? 'Failed to fetch manifest.';
      });
  }

  toggleAudioStream(hashId: string, checked: boolean) {
    this.selectedAudioStreamsHashIds = checked
      ? [...this.selectedAudioStreamsHashIds, hashId]
      : this.selectedAudioStreamsHashIds.filter(id => id !== hashId);
  }

  toggleVideoStream(hashId: string, checked: boolean) {
    this.selectedVideoStreamsHashIds = checked
      ? [...this.selectedVideoStreamsHashIds, hashId]
      : this.selectedVideoStreamsHashIds.filter(id => id !== hashId);
  }

  isSelected(hashId: string): boolean {
    return this.selectedAudioStreamsHashIds.includes(hashId)
      || this.selectedVideoStreamsHashIds.includes(hashId);
  }

  async downloadStreams() {
    const allSelectedStreams = [...this.selectedAudioStreamsHashIds, ...this.selectedVideoStreamsHashIds];

    if (!allSelectedStreams.length) {
      console.warn('No streams selected for download.');
      return;
    }

    this.utubeService.postDownload(allSelectedStreams)
      .subscribe({
        next: (response) => {
          this.downloadInitSuccess = true;
          console.log('Download initiated:', response);
        },
        error: (error) => {
          console.error('Error initiating download:', error);
          this.downloadError = error;
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
}
