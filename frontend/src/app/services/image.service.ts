import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ImageUploadResult {
  imageId: string;
  originalUrl: string;
  largeUrl: string;
  mediumUrl: string;
  thumbnailUrl: string;
}

export interface MultipleUploadResponse {
  results: ImageUploadResult[];
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ImageService {
  private apiUrl = `${environment.apiUrl}/image`;

  constructor(private http: HttpClient) {}

  uploadImage(file: File): Observable<ImageUploadResult> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ImageUploadResult>(`${this.apiUrl}/upload`, formData);
  }

  uploadMultipleImages(files: File[]): Observable<MultipleUploadResponse> {
    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file);
    });

    return this.http.post<MultipleUploadResponse>(`${this.apiUrl}/upload-multiple`, formData);
  }

  deleteImage(imageId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${imageId}`);
  }

  // Helper to get the full URL for an image
  getImageUrl(path: string | null | undefined): string {
    if (!path) {
      return 'https://placehold.co/600x400/CCCCCC/FFFFFF?text=No+Image';
    }

    // If it's already a full URL, return as-is
    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }

    // Otherwise, prepend the API base URL
    return `${environment.apiUrl.replace('/api', '')}${path}`;
  }

}
