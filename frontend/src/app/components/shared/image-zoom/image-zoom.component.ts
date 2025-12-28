import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-image-zoom',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-zoom.component.html',
  styleUrls: ['./image-zoom.component.scss']
})
export class ImageZoomComponent {
  @Input() images: string[] = [];
  @Input() productName: string = '';

  selectedImageIndex = 0;
  showLightbox = false;
  zoomLevel = 1;

  get selectedImage(): string {
    return this.images[this.selectedImageIndex] || '';
  }

  selectImage(index: number): void {
    this.selectedImageIndex = index;
  }

  openLightbox(index: number): void {
    this.selectedImageIndex = index;
    this.showLightbox = true;
    document.body.style.overflow = 'hidden';
  }

  closeLightbox(): void {
    this.showLightbox = false;
    this.zoomLevel = 1;
    document.body.style.overflow = 'auto';
  }

  nextImage(): void {
    this.selectedImageIndex = (this.selectedImageIndex + 1) % this.images.length;
  }

  previousImage(): void {
    this.selectedImageIndex = (this.selectedImageIndex - 1 + this.images.length) % this.images.length;
  }

  zoomIn(): void {
    this.zoomLevel = Math.min(this.zoomLevel + 0.25, 3);
  }

  zoomOut(): void {
    this.zoomLevel = Math.max(this.zoomLevel - 0.25, 1);
  }
}
