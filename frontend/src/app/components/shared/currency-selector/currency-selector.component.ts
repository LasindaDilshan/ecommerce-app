import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { CurrencyService, Currency } from '../../../services/currency.service';

@Component({
  selector: 'app-currency-selector',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="currency-selector" (click)="toggleDropdown()" [class.open]="isOpen">
      <span class="selected" *ngIf="selectedCurrency">
        {{ selectedCurrency.symbol }} {{ selectedCurrency.code }}
      </span>
      <span class="arrow">{{ isOpen ? '\u25B2' : '\u25BC' }}</span>

      <div class="dropdown" *ngIf="isOpen" (click)="$event.stopPropagation()">
        <div *ngFor="let currency of currencies"
          class="dropdown-item"
          [class.active]="selectedCurrency?.code === currency.code"
          (click)="selectCurrency(currency)">
          <span class="symbol">{{ currency.symbol }}</span>
          <span class="name">{{ currency.code }} - {{ currency.name }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .currency-selector {
      position: relative; display: inline-flex; align-items: center; gap: 4px;
      padding: 6px 12px; background: var(--bg-secondary); border: 1px solid var(--border-color);
      border-radius: 8px; cursor: pointer; font-size: 0.85rem; color: var(--text-primary);
      transition: all 0.2s; user-select: none;
    }
    .currency-selector:hover { border-color: var(--primary); }
    .currency-selector.open { border-color: var(--primary); box-shadow: 0 0 0 2px rgba(var(--primary-rgb), 0.1); }
    .arrow { font-size: 0.65rem; color: var(--text-tertiary); }
    .dropdown {
      position: absolute; top: 100%; right: 0; margin-top: 4px;
      background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 8px;
      box-shadow: var(--shadow-lg); z-index: 1000; min-width: 200px; max-height: 250px;
      overflow-y: auto; animation: fadeIn 0.15s ease;
    }
    .dropdown-item {
      display: flex; align-items: center; gap: 8px; padding: 10px 14px; cursor: pointer;
      transition: background 0.15s; font-size: 0.85rem; color: var(--text-primary);
    }
    .dropdown-item:hover { background: var(--bg-hover); }
    .dropdown-item.active { background: var(--primary); color: white; }
    .symbol { font-weight: 600; min-width: 20px; }
    .name { white-space: nowrap; }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(-4px); } to { opacity: 1; transform: translateY(0); } }
  `],
  host: {
    '(document:click)': 'closeDropdown()'
  }
})
export class CurrencySelectorComponent implements OnInit, OnDestroy {
  currencies: Currency[] = [];
  selectedCurrency: Currency | null = null;
  isOpen = false;
  private destroy$ = new Subject<void>();

  constructor(private currencyService: CurrencyService) {}

  ngOnInit(): void {
    this.currencyService.currencies$.pipe(takeUntil(this.destroy$)).subscribe(c => this.currencies = c);
    this.currencyService.selectedCurrency$.pipe(takeUntil(this.destroy$)).subscribe(c => this.selectedCurrency = c);
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  closeDropdown(): void {
    this.isOpen = false;
  }

  selectCurrency(currency: Currency): void {
    this.currencyService.setSelectedCurrency(currency);
    this.isOpen = false;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
