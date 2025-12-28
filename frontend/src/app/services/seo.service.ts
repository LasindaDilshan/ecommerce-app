import { Injectable, Inject } from '@angular/core';
import { Title, Meta } from '@angular/platform-browser';
import { DOCUMENT } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class SeoService {
  constructor(
    private titleService: Title,
    private metaService: Meta,
    @Inject(DOCUMENT) private document: Document
  ) {}

  updateTitle(title: string): void {
    this.titleService.setTitle(`${title} | ShopEase E-Commerce`);
  }

  updateMetaTags(config: {
    title?: string;
    description?: string;
    keywords?: string;
    image?: string;
    url?: string;
    type?: string;
  }): void {
    if (config.title) {
      this.updateTitle(config.title);
      this.metaService.updateTag({ property: 'og:title', content: config.title });
      this.metaService.updateTag({ name: 'twitter:title', content: config.title });
    }

    if (config.description) {
      this.metaService.updateTag({ name: 'description', content: config.description });
      this.metaService.updateTag({ property: 'og:description', content: config.description });
      this.metaService.updateTag({ name: 'twitter:description', content: config.description });
    }

    if (config.keywords) {
      this.metaService.updateTag({ name: 'keywords', content: config.keywords });
    }

    if (config.image) {
      this.metaService.updateTag({ property: 'og:image', content: config.image });
      this.metaService.updateTag({ name: 'twitter:image', content: config.image });
    }

    if (config.url) {
      this.metaService.updateTag({ property: 'og:url', content: config.url });
      this.metaService.updateTag({ name: 'twitter:url', content: config.url });
      this.updateCanonicalUrl(config.url);
    }

    this.metaService.updateTag({ property: 'og:type', content: config.type || 'website' });
    this.metaService.updateTag({ name: 'twitter:card', content: 'summary_large_image' });
  }

  updateCanonicalUrl(url?: string): void {
    const head = this.document.getElementsByTagName('head')[0];
    let element: HTMLLinkElement | null = this.document.querySelector(`link[rel='canonical']`) || null;

    if (!element) {
      element = this.document.createElement('link') as HTMLLinkElement;
      element.setAttribute('rel', 'canonical');
      head.appendChild(element);
    }

    element.setAttribute('href', url || this.document.URL);
  }

  addStructuredData(data: any): void {
    const script = this.document.createElement('script');
    script.type = 'application/ld+json';
    script.text = JSON.stringify(data);
    this.document.head.appendChild(script);
  }

  createProductStructuredData(product: any): any {
    return {
      '@context': 'https://schema.org/',
      '@type': 'Product',
      name: product.name,
      image: product.imageUrl,
      description: product.description,
      sku: product.sku,
      offers: {
        '@type': 'Offer',
        url: window.location.href,
        priceCurrency: 'USD',
        price: product.discountPrice || product.price,
        priceValidUntil: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        itemCondition: 'https://schema.org/NewCondition',
        availability: product.stockQuantity > 0 ? 'https://schema.org/InStock' : 'https://schema.org/OutOfStock'
      }
    };
  }

  createBreadcrumbStructuredData(breadcrumbs: Array<{name: string, url: string}>): any {
    return {
      '@context': 'https://schema.org',
      '@type': 'BreadcrumbList',
      itemListElement: breadcrumbs.map((crumb, index) => ({
        '@type': 'ListItem',
        position: index + 1,
        name: crumb.name,
        item: crumb.url
      }))
    };
  }
}
