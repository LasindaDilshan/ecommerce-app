import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { SearchService, SearchResult, SearchProduct, AutocompleteResult } from '../../services/search.service';
import { CategoryService } from '../../services/category.service';
import { StarRatingComponent } from '../shared/star-rating/star-rating.component';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, StarRatingComponent],
  template: `
    <div class="container">
      <div class="search-header">
        <h1>Search Products</h1>
        <div class="search-bar">
          <input
            type="text"
            [(ngModel)]="query"
            (ngModelChange)="onQueryChange($event)"
            (keyup.enter)="performSearch()"
            placeholder="Search for products..."
            class="search-input"
            (focus)="showSuggestions = true"
          />
          <button (click)="performSearch()" class="search-btn">Search</button>

          <!-- Autocomplete Dropdown -->
          <div class="autocomplete" *ngIf="showSuggestions && (autocomplete?.suggestions?.length || autocomplete?.products?.length)">
            <div *ngIf="autocomplete?.suggestions?.length" class="ac-section">
              <div class="ac-label">Suggestions</div>
              <div *ngFor="let s of autocomplete!.suggestions" (click)="selectSuggestion(s)" class="ac-item">{{ s }}</div>
            </div>
            <div *ngIf="autocomplete?.products?.length" class="ac-section">
              <div class="ac-label">Products</div>
              <div *ngFor="let p of autocomplete!.products" (click)="goToProduct(p.id)" class="ac-item ac-product">
                <img [src]="getImageUrl(p.imageUrl)" alt="" class="ac-img" />
                <span>{{ p.name }}</span>
                <span class="ac-price">\${{ p.price.toFixed(2) }}</span>
              </div>
            </div>
          </div>
        </div>
        <div class="popular-searches" *ngIf="popularSearches.length > 0 && !query">
          <span class="popular-label">Popular: </span>
          <button *ngFor="let term of popularSearches" (click)="selectSuggestion(term)" class="popular-tag">{{ term }}</button>
        </div>
      </div>

      <div class="search-layout" *ngIf="searched">
        <!-- Filters Sidebar -->
        <div class="filters-sidebar">
          <h3>Filters</h3>

          <div class="filter-group">
            <label>Category</label>
            <select [(ngModel)]="filters.categoryId" (change)="performSearch()" class="filter-select">
              <option [ngValue]="undefined">All Categories</option>
              <option *ngFor="let cat of categories" [ngValue]="cat.id">{{ cat.name }}</option>
            </select>
          </div>

          <div class="filter-group">
            <label>Price Range</label>
            <div class="price-inputs">
              <input type="number" [(ngModel)]="filters.minPrice" placeholder="Min" class="price-input" />
              <span>-</span>
              <input type="number" [(ngModel)]="filters.maxPrice" placeholder="Max" class="price-input" />
            </div>
            <button (click)="performSearch()" class="btn btn-sm">Apply Price</button>
          </div>

          <div class="filter-group">
            <label class="checkbox-label">
              <input type="checkbox" [(ngModel)]="filters.inStock" (change)="performSearch()" />
              In Stock Only
            </label>
          </div>

          <div class="filter-group">
            <label class="checkbox-label">
              <input type="checkbox" [(ngModel)]="filters.onSale" (change)="performSearch()" />
              On Sale
            </label>
          </div>

          <button (click)="clearFilters()" class="btn btn-secondary btn-full">Clear Filters</button>
        </div>

        <!-- Results -->
        <div class="search-results">
          <div class="results-header">
            <span class="results-count" *ngIf="results">
              {{ results.totalCount }} results for "{{ searchedQuery }}"
            </span>
            <select [(ngModel)]="filters.sortBy" (change)="performSearch()" class="sort-select">
              <option value="">Relevance</option>
              <option value="price_asc">Price: Low to High</option>
              <option value="price_desc">Price: High to Low</option>
              <option value="name_asc">Name: A-Z</option>
              <option value="rating_desc">Highest Rated</option>
              <option value="newest">Newest</option>
            </select>
          </div>

          <div class="products-grid" *ngIf="results && results.products.length > 0">
            <div *ngFor="let product of results.products" class="product-card" [routerLink]="['/products', product.id]">
              <div class="product-image">
                <img [src]="getImageUrl(product.imageUrl)" [alt]="product.name" />
                <span *ngIf="product.discountPrice" class="sale-badge">SALE</span>
              </div>
              <div class="product-info">
                <h4>{{ product.name }}</h4>
                <span class="category">{{ product.categoryName }}</span>
                <app-star-rating [rating]="product.rating" [reviewCount]="product.reviewCount" [size]="'small'"></app-star-rating>
                <div class="price-row">
                  <span *ngIf="product.discountPrice" class="original-price">\${{ product.price.toFixed(2) }}</span>
                  <span class="current-price">\${{ (product.discountPrice || product.price).toFixed(2) }}</span>
                </div>
                <span class="stock-status" [class.out]="product.stockQuantity === 0">
                  {{ product.stockQuantity > 0 ? 'In Stock' : 'Out of Stock' }}
                </span>
              </div>
            </div>
          </div>

          <div *ngIf="results && results.products.length === 0" class="no-results">
            <h3>No products found</h3>
            <p>Try adjusting your search or filters</p>
          </div>

          <!-- Pagination -->
          <div class="pagination" *ngIf="results && results.totalPages > 1">
            <button (click)="goToPage(currentPage - 1)" [disabled]="currentPage <= 1" class="page-btn">Previous</button>
            <span class="page-info">Page {{ currentPage }} of {{ results.totalPages }}</span>
            <button (click)="goToPage(currentPage + 1)" [disabled]="currentPage >= results.totalPages" class="page-btn">Next</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
    .search-header { margin-bottom: 30px; }
    .search-header h1 { color: var(--text-primary); font-size: 2rem; margin: 0 0 20px; }
    .search-bar { position: relative; display: flex; gap: 8px; margin-bottom: 12px; }
    .search-input { flex: 1; padding: 14px 18px; border: 2px solid var(--border-color); border-radius: 12px; font-size: 1.1rem; background: var(--bg-card); color: var(--text-primary); }
    .search-input:focus { outline: none; border-color: var(--primary); }
    .search-btn { padding: 14px 28px; background: var(--primary); color: white; border: none; border-radius: 12px; font-weight: 600; font-size: 1rem; cursor: pointer; }
    .search-btn:hover { opacity: 0.9; }
    .autocomplete { position: absolute; top: 100%; left: 0; right: 80px; background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; box-shadow: var(--shadow-lg); z-index: 100; max-height: 400px; overflow-y: auto; margin-top: 4px; }
    .ac-section { padding: 8px 0; border-bottom: 1px solid var(--border-color); }
    .ac-section:last-child { border-bottom: none; }
    .ac-label { padding: 4px 16px; font-size: 0.75rem; font-weight: 600; color: var(--text-tertiary); text-transform: uppercase; }
    .ac-item { padding: 10px 16px; cursor: pointer; color: var(--text-primary); font-size: 0.95rem; }
    .ac-item:hover { background: var(--bg-hover); }
    .ac-product { display: flex; align-items: center; gap: 12px; }
    .ac-img { width: 36px; height: 36px; border-radius: 6px; object-fit: cover; }
    .ac-price { margin-left: auto; font-weight: 600; color: var(--primary); }
    .popular-searches { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
    .popular-label { color: var(--text-secondary); font-size: 0.9rem; }
    .popular-tag { padding: 4px 12px; background: var(--bg-secondary); border: 1px solid var(--border-color); border-radius: 20px; cursor: pointer; font-size: 0.85rem; color: var(--text-primary); }
    .popular-tag:hover { border-color: var(--primary); color: var(--primary); }
    .search-layout { display: grid; grid-template-columns: 250px 1fr; gap: 24px; }
    .filters-sidebar { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 20px; height: fit-content; position: sticky; top: 80px; }
    .filters-sidebar h3 { margin: 0 0 16px; color: var(--text-primary); font-size: 1.1rem; }
    .filter-group { margin-bottom: 16px; }
    .filter-group label { display: block; font-size: 0.9rem; font-weight: 600; color: var(--text-primary); margin-bottom: 6px; }
    .filter-select { width: 100%; padding: 8px 10px; border: 1px solid var(--border-color); border-radius: 8px; background: var(--bg-secondary); color: var(--text-primary); font-size: 0.9rem; }
    .price-inputs { display: flex; gap: 8px; align-items: center; margin-bottom: 8px; }
    .price-input { width: 80px; padding: 6px 8px; border: 1px solid var(--border-color); border-radius: 6px; font-size: 0.9rem; background: var(--bg-secondary); color: var(--text-primary); }
    .checkbox-label { display: flex; align-items: center; gap: 6px; cursor: pointer; font-weight: normal !important; }
    .results-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .results-count { color: var(--text-secondary); font-size: 0.95rem; }
    .sort-select { padding: 8px 12px; border: 1px solid var(--border-color); border-radius: 8px; background: var(--bg-card); color: var(--text-primary); }
    .products-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
    .product-card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; overflow: hidden; cursor: pointer; transition: transform 0.2s, box-shadow 0.2s; text-decoration: none; }
    .product-card:hover { transform: translateY(-4px); box-shadow: var(--shadow-md); }
    .product-image { position: relative; height: 180px; overflow: hidden; }
    .product-image img { width: 100%; height: 100%; object-fit: cover; }
    .sale-badge { position: absolute; top: 8px; right: 8px; background: var(--danger); color: white; padding: 2px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: 700; }
    .product-info { padding: 14px; }
    .product-info h4 { margin: 0 0 4px; color: var(--text-primary); font-size: 1rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .category { font-size: 0.8rem; color: var(--text-tertiary); }
    .price-row { margin-top: 8px; display: flex; align-items: center; gap: 8px; }
    .original-price { text-decoration: line-through; color: var(--text-tertiary); font-size: 0.9rem; }
    .current-price { font-weight: 700; color: var(--primary); font-size: 1.1rem; }
    .stock-status { font-size: 0.8rem; color: var(--success); font-weight: 500; }
    .stock-status.out { color: var(--danger); }
    .no-results { text-align: center; padding: 60px; color: var(--text-secondary); }
    .no-results h3 { margin: 0 0 8px; color: var(--text-primary); }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 16px; margin-top: 24px; }
    .page-btn { padding: 8px 16px; border: 1px solid var(--border-color); border-radius: 8px; background: var(--bg-card); color: var(--text-primary); cursor: pointer; }
    .page-btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .page-info { color: var(--text-secondary); }
    .btn { padding: 8px 16px; border: none; border-radius: 8px; cursor: pointer; font-weight: 500; }
    .btn-sm { padding: 6px 12px; font-size: 0.85rem; background: var(--primary); color: white; width: 100%; }
    .btn-secondary { background: var(--bg-secondary); color: var(--text-primary); border: 1px solid var(--border-color); }
    .btn-full { width: 100%; }
    @media (max-width: 768px) {
      .search-layout { grid-template-columns: 1fr; }
      .filters-sidebar { position: static; }
      .products-grid { grid-template-columns: repeat(2, 1fr); }
    }
  `],
  host: {
    '(document:click)': 'showSuggestions = false'
  }
})
export class SearchComponent implements OnInit, OnDestroy {
  query = '';
  searchedQuery = '';
  searched = false;
  results: SearchResult | null = null;
  autocomplete: AutocompleteResult | null = null;
  showSuggestions = false;
  popularSearches: string[] = [];
  categories: any[] = [];
  currentPage = 1;

  filters: {
    categoryId?: number;
    minPrice?: number;
    maxPrice?: number;
    inStock?: boolean;
    onSale?: boolean;
    sortBy?: string;
  } = {};

  private querySubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private searchService: SearchService,
    private categoryService: CategoryService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.categoryService.getCategories().pipe(takeUntil(this.destroy$)).subscribe(cats => this.categories = cats);
    this.searchService.getPopularSearches().pipe(takeUntil(this.destroy$)).subscribe({
      next: (searches) => this.popularSearches = searches,
      error: () => {}
    });

    this.querySubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(q => {
      if (q.length >= 2) {
        this.searchService.autocomplete(q).subscribe({
          next: (result) => this.autocomplete = result,
          error: () => {}
        });
      } else {
        this.autocomplete = null;
      }
    });

    const q = this.route.snapshot.queryParamMap.get('q');
    if (q) {
      this.query = q;
      this.performSearch();
    }
  }

  onQueryChange(value: string): void {
    this.querySubject.next(value);
  }

  performSearch(): void {
    if (!this.query.trim()) return;
    this.showSuggestions = false;
    this.searched = true;
    this.searchedQuery = this.query;

    this.searchService.search(this.query, { ...this.filters, page: this.currentPage, pageSize: 20 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (results) => this.results = results,
        error: () => this.results = { products: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 }
      });
  }

  selectSuggestion(term: string): void {
    this.query = term;
    this.performSearch();
  }

  goToProduct(id: number): void {
    this.showSuggestions = false;
    this.router.navigate(['/products', id]);
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.performSearch();
  }

  clearFilters(): void {
    this.filters = {};
    this.performSearch();
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) return 'https://placehold.co/300x200/CCCCCC/FFFFFF?text=No+Image';
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;
    return `${environment.apiUrl.replace('/api', '')}${imageUrl}`;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
