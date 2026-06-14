import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface StreamImportRequest {
  videoId: string;
  audioFormatIds: string[];
  videoFormatIds: string[];
}

export interface DownloadedFileItem {
  name: string;
  path: string;
  sizeBytes: number;
  modifiedAtUtc: string;
  contentType: string;
  isPlayable: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UtubeApiService {

  private baseUrl: string = 'https://localhost:7101/api';

  constructor(private http: HttpClient) { }

  getData(endpoint: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/${endpoint}`);
  }

  postDownload(request: StreamImportRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/StreamStorage/Import`, request);
  }

  getDownloadedFiles(): Observable<DownloadedFileItem[]> {
    return this.http.get<DownloadedFileItem[]>(`${this.baseUrl}/StreamStorage/files`);
  }

  fetchManifest(videoId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/Search/fetchmanifest/${encodeURIComponent(videoId)}`);
  }

  getStreamUrl(path: string): string {
    return `${this.baseUrl}/StreamStorage/stream?path=${encodeURIComponent(path)}`;
  }

  getSearchStreamUrl(query: string, count: number): string {
    const params = new URLSearchParams({
      q: query,
      count: count.toString(),
    });

    return `${this.baseUrl}/Search/stream?${params.toString()}`;
  }
}

// POST
//  /api/StreamStorage/Import
