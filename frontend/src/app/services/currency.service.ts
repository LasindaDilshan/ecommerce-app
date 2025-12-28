import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface Currency {
  id: number;
  code: string;
  symbol: string;
  name: string;
  exchangeRate: number;
  isActive: boolean;
  updatedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class CurrencyService {
  private readonly API_URL = `${environment.apiUrl}/currency`;
  private readonly STORAGE_KEY = 'selected-currency';
  private readonly DEFAULT_CURRENCY = 'USD';

  private currenciesSubject = new BehaviorSubject<Currency[]>([]);
  private selectedCurrencySubject: BehaviorSubject<Currency | null>;

  currencies$: Observable<Currency[]> = this.currenciesSubject.asObservable();
  selectedCurrency$: Observable<Currency | null>;

  constructor(private http: HttpClient) {
    const savedCurrency = this.loadSavedCurrency();
    this.selectedCurrencySubject = new BehaviorSubject<Currency | null>(savedCurrency);
    this.selectedCurrency$ = this.selectedCurrencySubject.asObservable();

    this.loadCurrencies();
  }

  loadCurrencies(): void {
    this.http.get<Currency[]>(this.API_URL)
      .subscribe({
        next: (currencies) => {
          this.currenciesSubject.next(currencies);

          // If no currency is selected, set default
          if (!this.selectedCurrencySubject.value && currencies.length > 0) {
            const defaultCurrency = currencies.find(c => c.code === this.DEFAULT_CURRENCY) || currencies[0];
            this.setSelectedCurrency(defaultCurrency);
          }
        },
        error: (error) => {
          console.error('Error loading currencies:', error);
        }
      });
  }

  setSelectedCurrency(currency: Currency): void {
    this.selectedCurrencySubject.next(currency);
    this.saveCurrency(currency);
  }

  getSelectedCurrency(): Currency | null {
    return this.selectedCurrencySubject.value;
  }

  convertPrice(price: number, fromCode?: string): number {
    const selectedCurrency = this.selectedCurrencySubject.value;
    if (!selectedCurrency) {
      return price;
    }

    const from = fromCode || this.DEFAULT_CURRENCY;

    if (from === selectedCurrency.code) {
      return price;
    }

    // Get the currency objects
    const currencies = this.currenciesSubject.value;
    const fromCurrency = currencies.find(c => c.code === from);
    const toCurrency = selectedCurrency;

    if (!fromCurrency || !toCurrency) {
      return price;
    }

    // Convert to USD first (base currency), then to target currency
    const priceInUSD = price / fromCurrency.exchangeRate;
    return priceInUSD * toCurrency.exchangeRate;
  }

  formatPrice(price: number, fromCode?: string): string {
    const selectedCurrency = this.selectedCurrencySubject.value;
    if (!selectedCurrency) {
      return `$${price.toFixed(2)}`;
    }

    const convertedPrice = this.convertPrice(price, fromCode);
    return `${selectedCurrency.symbol}${convertedPrice.toFixed(2)}`;
  }

  private saveCurrency(currency: Currency): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(currency));
    } catch (error) {
      console.error('Error saving currency:', error);
    }
  }

  private loadSavedCurrency(): Currency | null {
    try {
      const saved = localStorage.getItem(this.STORAGE_KEY);
      return saved ? JSON.parse(saved) : null;
    } catch (error) {
      console.error('Error loading saved currency:', error);
      return null;
    }
  }

}
