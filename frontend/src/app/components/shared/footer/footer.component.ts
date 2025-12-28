import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <footer class="footer">
      <div class="container">
        <p>&copy; {{ currentYear }} E-Commerce. All rights reserved.</p>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      background: var(--bg-secondary);
      color: var(--text-primary);
      padding: 20px 0;
      text-align: center;
      margin-top: auto;
      border-top: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
    }

    .footer p {
      margin: 0;
      color: var(--text-secondary);
    }
  `]
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}
