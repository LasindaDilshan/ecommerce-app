import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkMode = new BehaviorSubject<boolean>(false);
  isDarkMode$ = this.darkMode.asObservable();

  constructor() {
    // Check for saved theme preference or default to light mode
    const savedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (savedTheme) {
      this.setDarkMode(savedTheme === 'dark');
    } else if (prefersDark) {
      this.setDarkMode(true);
    }

    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
      if (!localStorage.getItem('theme')) {
        this.setDarkMode(e.matches);
      }
    });
  }

  toggleDarkMode(): void {
    this.setDarkMode(!this.darkMode.value);
  }

  setDarkMode(isDark: boolean): void {
    this.darkMode.next(isDark);

    if (isDark) {
      document.documentElement.classList.add('dark');
      localStorage.setItem('theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark');
      localStorage.setItem('theme', 'light');
    }
  }

  isDarkModeEnabled(): boolean {
    return this.darkMode.value;
  }
}