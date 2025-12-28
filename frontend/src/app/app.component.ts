import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/shared/header/header.component';
import { FooterComponent } from './components/shared/footer/footer.component';
import { ExitIntentPopupComponent } from './components/shared/exit-intent-popup/exit-intent-popup.component';
import { PurchaseNotificationComponent } from './components/shared/purchase-notification/purchase-notification.component';
import { ComparisonBarComponent } from './components/shared/comparison-bar/comparison-bar.component';
import { PwaPromptComponent } from './components/shared/pwa-prompt/pwa-prompt.component';
import { OnlineStatusComponent } from './components/shared/online-status/online-status.component';
import { ToastContainerComponent } from './components/shared/toast-container/toast-container.component';
import { ConfirmationModalComponent } from './components/shared/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, ExitIntentPopupComponent, PurchaseNotificationComponent, ComparisonBarComponent, PwaPromptComponent, OnlineStatusComponent, ToastContainerComponent, ConfirmationModalComponent],
  template: `
    <div class="app-container">
      <app-online-status></app-online-status>
      <app-header></app-header>
      <main class="main-content">
        <router-outlet></router-outlet>
      </main>
      <app-footer></app-footer>
      <app-exit-intent-popup></app-exit-intent-popup>
      <app-purchase-notification></app-purchase-notification>
      <app-comparison-bar></app-comparison-bar>
      <app-pwa-prompt></app-pwa-prompt>
      <app-toast-container></app-toast-container>
      <app-confirmation-modal></app-confirmation-modal>
    </div>
  `,
  styles: [`
    .app-container {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
      position: relative;
    }

    .main-content {
      flex: 1 0 auto;
      padding-top: 60px;
      padding-bottom: 20px;
      min-height: calc(100vh - 120px);
    }

    app-footer {
      flex-shrink: 0;
      width: 100%;
    }
  `]
})
export class AppComponent {
  title = 'E-Commerce App';
}
