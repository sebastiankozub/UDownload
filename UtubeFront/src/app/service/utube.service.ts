import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface StreamImportRequest {
  videoId: string;
  audioFormatIds: string[];
  videoFormatIds: string[];
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

  fetchManifest(videoId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/Search/fetchmanifest/${encodeURIComponent(videoId)}`);
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
