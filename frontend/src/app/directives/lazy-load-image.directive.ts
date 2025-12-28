import { Directive, ElementRef, AfterViewInit } from '@angular/core';

@Directive({
  selector: 'img[appLazyLoad]',
  standalone: true
})
export class LazyLoadImageDirective implements AfterViewInit {
  constructor(private el: ElementRef<HTMLImageElement>) {}

  ngAfterViewInit() {
    if ('IntersectionObserver' in window) {
      const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            const img = entry.target as HTMLImageElement;
            const src = img.getAttribute('data-src');
            if (src) {
              img.src = src;
              img.removeAttribute('data-src');
            }
            observer.unobserve(img);
          }
        });
      });

      // Move src to data-src
      const img = this.el.nativeElement;
      const src = img.src;
      if (src) {
        img.setAttribute('data-src', src);
        img.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1 1"%3E%3C/svg%3E';
      }

      observer.observe(img);
    }
  }
}
