import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UtubeApiService } from '../service/utube.service';

export interface AvManifest {
  id: string;
  title: string;
  url: string;
  uploadDate: Date;
  description: string;
  keywords: string[];
  audioStreams: AudioStream[];
  videoStreams: VideoStream[];
}

export interface VideoStream extends  AvStream {  
  videoCodec: string;
  videoQuality: string;
  videoResolution: string;
}

export interface AudioStream extends  AvStream {
  audioCodec: string;
  audioLanguage?: string; // Optional
  isAudioLanguageDefault?: boolean; // Optional
}

export interface AvStream {
  url: string;
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

  resourceId: string = '';
  videoStreams: VideoStream[] = [];
  audioStreams: AudioStream[] = [];
  avManifest? : AvManifest;

  videoStreamFriendlyNames: string[] = [];
  audioStreamFriendlyNames: string[] = [];

  selectedVideoStreamsHashIds: string[] = [];
  selectedAudioStreamsHashIds: string[] = [];

  private baseUrl: string = 'https://localhost:7101/api';
  
  constructor(private utubeService: UtubeApiService, private http: HttpClient) 
  {}

  fetchAvManifest() {
    this.http.get<AvManifest>(`${this.baseUrl}/AvManifest/${this.resourceId}`)
      .subscribe(manifest => {
        this.audioStreams = manifest.audioStreams;
        this.videoStreams = manifest.videoStreams;

        this.audioStreams = this.audioStreams.map(s => 
        { 
          s.friendlyName = s.audioCodec.replaceAll(' ', '') 
          + " | "
          + s.bitrate.replaceAll(' ', '')
          + " | "
          + s.size.replaceAll(' ', '');
          return s;
        });

        this.videoStreams = this.videoStreams.map(s => 
        { 
          s.friendlyName = s.videoCodec.replaceAll(' ', '') 
          + " | "
          + s.bitrate.replaceAll(' ', '')
          + " | "
          + s.size.replaceAll(' ', '');
          return s;
        });
 
        this.audioStreamFriendlyNames = this.audioStreams.map(s => s.friendlyName);
        this.videoStreamFriendlyNames = this.videoStreams.map(s => s.friendlyName);

        manifest.url = `https://www.youtube.com/watch?v=${manifest.id}`;
        
        this.avManifest = manifest;

        // TODO cache manifest
      });
  }

  onSelectedVStreamsChange(selectedItemsHashIds: string[]) {
    this.selectedVideoStreamsHashIds = selectedItemsHashIds;
    console.log(selectedItemsHashIds );
    console.log(this.selectedVideoStreamsHashIds );
  }

  onSelectedAStreamsChange(selectedItemsHashIds: string[]) {
    this.selectedAudioStreamsHashIds = selectedItemsHashIds;
    console.log("onSelectedAStreamsChange");
    console.log(selectedItemsHashIds );
    console.log(this.selectedAudioStreamsHashIds );
  }
  
  async downloadStreams() {
    //this.downloading = true;
    //this.downloadError = null;
    //this.downloadComplete = false;

    //const downloadPromises: Promise<any>[] = [];
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
    })
    //.subscribe(
    //   response => {
    //     console.log('Download initiated:', response);
    //   },
    //   error => {
    //     console.error('Error initiating download:', error);
    //   }
    // );


    // for (const streamHashId of allSelectedStreams) {
    //   const downloadUrl = `` ; //`${process.env.AZURE_FUNCTION_URL}/download/${streamHashId}`;  // api/StreamStorage/Import
    //   downloadPromises.push(this.http.post(downloadUrl, {}).toPromise());
    // }

    // try {
    //   await Promise.all(downloadPromises);
    //   //this.triggerInternalProcessing();//not here
    //   console.log('All streams downloaded successfully.');
    // } catch (error) {
    //   console.error('Error downloading streams:', error);
    //   // Handle
    // }
  }
}

// @Pipe({
//   name: 'join'
// })
// export class JoinPipe implements PipeTransform {
//   transform(input:Array<any>, sep = ','): string {
//     return input.join(sep);
//   }
// }
// <p>{{ cardData.names|join:', ' }}</p>