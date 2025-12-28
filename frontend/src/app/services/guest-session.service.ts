import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GuestSessionService {
  private readonly SESSION_KEY = 'guestSessionId';

  constructor() {
    this.ensureSessionId();
  }

  /**
   * Get the current guest session ID, creating one if it doesn't exist
   */
  getSessionId(): string {
    const sessionId = localStorage.getItem(this.SESSION_KEY);
    if (sessionId) {
      return sessionId;
    }
    return this.createNewSession();
  }

  /**
   * Create a new guest session ID
   */
  private createNewSession(): string {
    const sessionId = this.generateGuid();
    localStorage.setItem(this.SESSION_KEY, sessionId);
    return sessionId;
  }

  /**
   * Ensure a session ID exists
   */
  private ensureSessionId(): void {
    if (!localStorage.getItem(this.SESSION_KEY)) {
      this.createNewSession();
    }
  }

  /**
   * Clear the guest session (call when user logs in)
   */
  clearSession(): void {
    localStorage.removeItem(this.SESSION_KEY);
  }

  /**
   * Check if a guest session exists
   */
  hasSession(): boolean {
    return localStorage.getItem(this.SESSION_KEY) !== null;
  }

  /**
   * Generate a cryptographically secure UUID for session identification
   * Uses crypto.getRandomValues() which is cryptographically secure
   */
  private generateGuid(): string {
    // Use crypto.randomUUID if available (modern browsers)
    if (typeof crypto !== 'undefined' && crypto.randomUUID) {
      return crypto.randomUUID();
    }

    // Fallback using crypto.getRandomValues (still cryptographically secure)
    const bytes = new Uint8Array(16);
    crypto.getRandomValues(bytes);

    // Set version (4) and variant bits per RFC 4122
    bytes[6] = (bytes[6] & 0x0f) | 0x40; // Version 4
    bytes[8] = (bytes[8] & 0x3f) | 0x80; // Variant 10

    const hex = Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('');
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
  }
}
