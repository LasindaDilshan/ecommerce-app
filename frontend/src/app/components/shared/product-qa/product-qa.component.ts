import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ProductQAService } from '../../../services/product-qa.service';
import { AuthService } from '../../../services/auth.service';
import { ToastService } from '../../../services/toast.service';
import { ProductQuestion, ProductAnswer } from '../../../models/product-qa.models';

@Component({
  selector: 'app-product-qa',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="qa-section">
      <h2>Questions & Answers</h2>

      <!-- Ask Question -->
      <div class="ask-section" *ngIf="isLoggedIn">
        <div class="ask-form" *ngIf="!showQuestionForm">
          <button (click)="showQuestionForm = true" class="btn btn-primary">Ask a Question</button>
        </div>
        <div class="question-form" *ngIf="showQuestionForm">
          <textarea
            [(ngModel)]="newQuestionText"
            placeholder="What would you like to know about this product?"
            class="form-control"
            rows="3"
          ></textarea>
          <div class="form-actions">
            <button (click)="submitQuestion()" [disabled]="!newQuestionText.trim()" class="btn btn-primary">Submit Question</button>
            <button (click)="showQuestionForm = false; newQuestionText = ''" class="btn btn-secondary">Cancel</button>
          </div>
        </div>
      </div>

      <!-- Questions List -->
      <div class="questions-list">
        <div *ngFor="let question of questions" class="question-card">
          <div class="question-header">
            <div class="q-badge">Q</div>
            <div class="question-content">
              <p class="question-text">{{ question.questionText }}</p>
              <div class="question-meta">
                <span class="asker">{{ question.userName }}</span>
                <span class="date">{{ question.createdAt | date:'mediumDate' }}</span>
                <button (click)="voteQuestion(question)" class="vote-btn" [class.voted]="question.hasUserVoted">
                  Helpful ({{ question.upvoteCount }})
                </button>
              </div>
            </div>
          </div>

          <!-- Answers -->
          <div class="answers" *ngIf="question.answers && question.answers.length > 0">
            <div *ngFor="let answer of question.answers" class="answer-card">
              <div class="a-badge" [class.seller]="answer.isSellerAnswer">{{ answer.isSellerAnswer ? 'S' : 'A' }}</div>
              <div class="answer-content">
                <p class="answer-text">{{ answer.answerText }}</p>
                <div class="answer-meta">
                  <span class="answerer">
                    {{ answer.userName }}
                    <span *ngIf="answer.isSellerAnswer" class="seller-badge">Seller</span>
                    <span *ngIf="answer.isVerifiedPurchase" class="verified-badge">Verified Purchase</span>
                  </span>
                  <span class="date">{{ answer.createdAt | date:'mediumDate' }}</span>
                  <button (click)="voteAnswer(answer, true)" class="vote-btn" [class.voted]="answer.hasUserVoted">
                    Helpful ({{ answer.helpfulCount }})
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Answer Form -->
          <div class="answer-form-toggle" *ngIf="isLoggedIn">
            <button *ngIf="answeringQuestionId !== question.id" (click)="answeringQuestionId = question.id" class="btn-link">
              Answer this question
            </button>
            <div *ngIf="answeringQuestionId === question.id" class="answer-form">
              <textarea [(ngModel)]="newAnswerText" placeholder="Write your answer..." class="form-control" rows="2"></textarea>
              <div class="form-actions">
                <button (click)="submitAnswer(question.id)" [disabled]="!newAnswerText.trim()" class="btn btn-primary btn-sm">Submit Answer</button>
                <button (click)="answeringQuestionId = 0; newAnswerText = ''" class="btn btn-secondary btn-sm">Cancel</button>
              </div>
            </div>
          </div>
        </div>

        <div *ngIf="questions.length === 0" class="no-questions">
          <p>No questions yet. Be the first to ask!</p>
        </div>

        <!-- Pagination -->
        <div class="pagination" *ngIf="totalPages > 1">
          <button (click)="loadQuestions(currentPage - 1)" [disabled]="currentPage <= 1" class="page-btn">Previous</button>
          <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
          <button (click)="loadQuestions(currentPage + 1)" [disabled]="currentPage >= totalPages" class="page-btn">Next</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .qa-section { padding: 40px 0; border-top: 1px solid var(--border-color); margin-top: 20px; }
    .qa-section h2 { font-size: 1.5rem; margin-bottom: 24px; color: var(--text-primary); }
    .ask-section { margin-bottom: 24px; }
    .question-form, .answer-form { margin-top: 12px; }
    .form-control { width: 100%; padding: 10px 14px; border: 1px solid var(--border-color); border-radius: 8px; font-size: 1rem; background: var(--bg-secondary); color: var(--text-primary); box-sizing: border-box; font-family: inherit; resize: vertical; }
    .form-control:focus { outline: none; border-color: var(--primary); }
    .form-actions { display: flex; gap: 8px; margin-top: 8px; }
    .questions-list { display: flex; flex-direction: column; gap: 20px; }
    .question-card { background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 12px; padding: 20px; }
    .question-header { display: flex; gap: 12px; }
    .q-badge, .a-badge { width: 32px; height: 32px; border-radius: 50%; background: var(--primary); color: white; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 0.9rem; flex-shrink: 0; }
    .a-badge { background: #6b7280; }
    .a-badge.seller { background: #059669; }
    .question-content, .answer-content { flex: 1; }
    .question-text { color: var(--text-primary); font-weight: 600; margin: 0 0 8px; }
    .answer-text { color: var(--text-secondary); margin: 0 0 8px; line-height: 1.5; }
    .question-meta, .answer-meta { display: flex; gap: 12px; align-items: center; flex-wrap: wrap; font-size: 0.85rem; }
    .asker, .answerer { color: var(--text-primary); font-weight: 500; }
    .date { color: var(--text-tertiary); }
    .seller-badge { background: #d1fae5; color: #065f46; padding: 1px 6px; border-radius: 4px; font-size: 0.7rem; font-weight: 600; margin-left: 4px; }
    .verified-badge { background: #dbeafe; color: #1e40af; padding: 1px 6px; border-radius: 4px; font-size: 0.7rem; font-weight: 600; margin-left: 4px; }
    .vote-btn { padding: 2px 10px; border: 1px solid var(--border-color); border-radius: 4px; background: var(--bg-secondary); color: var(--text-secondary); cursor: pointer; font-size: 0.8rem; }
    .vote-btn:hover { border-color: var(--primary); color: var(--primary); }
    .vote-btn.voted { background: var(--primary); color: white; border-color: var(--primary); }
    .answers { margin: 16px 0 8px 44px; display: flex; flex-direction: column; gap: 12px; }
    .answer-card { display: flex; gap: 12px; padding: 12px; background: var(--bg-secondary); border-radius: 8px; }
    .answer-form-toggle { margin-left: 44px; margin-top: 8px; }
    .btn-link { background: none; border: none; color: var(--primary); cursor: pointer; font-size: 0.9rem; padding: 0; text-decoration: underline; }
    .btn-link:hover { color: var(--primary-dark); }
    .no-questions { text-align: center; padding: 40px; color: var(--text-secondary); }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 16px; margin-top: 24px; }
    .page-btn { padding: 8px 16px; border: 1px solid var(--border-color); border-radius: 8px; background: var(--bg-card); color: var(--text-primary); cursor: pointer; }
    .page-btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .page-info { color: var(--text-secondary); font-size: 0.9rem; }
    .btn { padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.95rem; }
    .btn-sm { padding: 6px 14px; font-size: 0.85rem; }
    .btn-primary { background: var(--primary); color: white; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary { background: var(--bg-secondary); color: var(--text-primary); border: 1px solid var(--border-color); }
    @media (max-width: 768px) {
      .answers { margin-left: 20px; }
      .answer-form-toggle { margin-left: 20px; }
    }
  `]
})
export class ProductQAComponent implements OnInit, OnDestroy {
  @Input() productId!: number;

  questions: ProductQuestion[] = [];
  isLoggedIn = false;
  showQuestionForm = false;
  newQuestionText = '';
  answeringQuestionId = 0;
  newAnswerText = '';
  currentPage = 1;
  totalPages = 1;

  private destroy$ = new Subject<void>();

  constructor(
    private qaService: ProductQAService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.loadQuestions();
  }

  loadQuestions(page: number = 1): void {
    this.currentPage = page;
    this.qaService.getProductQuestions(this.productId, page, 10)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.questions = response.questions;
          this.totalPages = response.totalPages;
        },
        error: () => {}
      });
  }

  submitQuestion(): void {
    if (!this.newQuestionText.trim()) return;

    this.qaService.createQuestion({ productId: this.productId, questionText: this.newQuestionText.trim() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.success('Question Submitted', 'Your question has been submitted.');
          this.showQuestionForm = false;
          this.newQuestionText = '';
          this.loadQuestions();
        },
        error: (err) => {
          this.toastService.error('Error', err.error?.message || 'Failed to submit question');
        }
      });
  }

  submitAnswer(questionId: number): void {
    if (!this.newAnswerText.trim()) return;

    this.qaService.createAnswer({ questionId, answerText: this.newAnswerText.trim() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.success('Answer Submitted', 'Your answer has been submitted.');
          this.answeringQuestionId = 0;
          this.newAnswerText = '';
          this.loadQuestions(this.currentPage);
        },
        error: (err) => {
          this.toastService.error('Error', err.error?.message || 'Failed to submit answer');
        }
      });
  }

  voteQuestion(question: ProductQuestion): void {
    if (!this.isLoggedIn) return;

    if (question.hasUserVoted) {
      this.qaService.removeQuestionVote(question.id).pipe(takeUntil(this.destroy$)).subscribe({ next: () => this.loadQuestions(this.currentPage) });
    } else {
      this.qaService.voteQuestion(question.id).pipe(takeUntil(this.destroy$)).subscribe({ next: () => this.loadQuestions(this.currentPage) });
    }
  }

  voteAnswer(answer: ProductAnswer, helpful: boolean): void {
    if (!this.isLoggedIn) return;

    if (answer.hasUserVoted) {
      this.qaService.removeAnswerVote(answer.id).pipe(takeUntil(this.destroy$)).subscribe({ next: () => this.loadQuestions(this.currentPage) });
    } else {
      this.qaService.voteAnswer(answer.id, helpful).pipe(takeUntil(this.destroy$)).subscribe({ next: () => this.loadQuestions(this.currentPage) });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
