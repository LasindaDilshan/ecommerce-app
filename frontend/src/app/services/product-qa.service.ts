import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ProductQuestion,
  ProductAnswer,
  CreateQuestionRequest,
  CreateAnswerRequest,
  ModerateQuestionRequest,
  ModerateAnswerRequest,
  QuestionListResponse,
  VoteResponse
} from '../models/product-qa.models';

@Injectable({
  providedIn: 'root'
})
export class ProductQAService {
  private apiUrl = `${environment.apiUrl}/productqa`;

  constructor(private http: HttpClient) {}

  // ==================== Questions ====================

  getProductQuestions(productId: number, page: number = 1, pageSize: number = 10): Observable<QuestionListResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<QuestionListResponse>(`${this.apiUrl}/products/${productId}/questions`, { params });
  }

  getQuestionById(questionId: number): Observable<ProductQuestion> {
    return this.http.get<ProductQuestion>(`${this.apiUrl}/questions/${questionId}`);
  }

  createQuestion(request: CreateQuestionRequest): Observable<ProductQuestion> {
    return this.http.post<ProductQuestion>(`${this.apiUrl}/questions`, request);
  }

  updateQuestion(questionId: number, newText: string): Observable<ProductQuestion> {
    return this.http.put<ProductQuestion>(`${this.apiUrl}/questions/${questionId}`, JSON.stringify(newText), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  deleteQuestion(questionId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/questions/${questionId}`);
  }

  // ==================== Answers ====================

  createAnswer(request: CreateAnswerRequest): Observable<ProductAnswer> {
    return this.http.post<ProductAnswer>(`${this.apiUrl}/answers`, request);
  }

  updateAnswer(answerId: number, newText: string): Observable<ProductAnswer> {
    return this.http.put<ProductAnswer>(`${this.apiUrl}/answers/${answerId}`, JSON.stringify(newText), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  deleteAnswer(answerId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/answers/${answerId}`);
  }

  // ==================== Voting ====================

  voteQuestion(questionId: number): Observable<VoteResponse> {
    return this.http.post<VoteResponse>(`${this.apiUrl}/questions/${questionId}/vote`, {});
  }

  removeQuestionVote(questionId: number): Observable<VoteResponse> {
    return this.http.delete<VoteResponse>(`${this.apiUrl}/questions/${questionId}/vote`);
  }

  voteAnswer(answerId: number, helpful: boolean = true): Observable<VoteResponse> {
    const params = new HttpParams().set('helpful', helpful.toString());
    return this.http.post<VoteResponse>(`${this.apiUrl}/answers/${answerId}/vote`, {}, { params });
  }

  removeAnswerVote(answerId: number): Observable<VoteResponse> {
    return this.http.delete<VoteResponse>(`${this.apiUrl}/answers/${answerId}/vote`);
  }

  // ==================== Moderation (Admin) ====================

  getPendingQuestions(page: number = 1, pageSize: number = 20): Observable<ProductQuestion[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ProductQuestion[]>(`${this.apiUrl}/admin/questions/pending`, { params });
  }

  getPendingAnswers(page: number = 1, pageSize: number = 20): Observable<ProductAnswer[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ProductAnswer[]>(`${this.apiUrl}/admin/answers/pending`, { params });
  }

  moderateQuestion(questionId: number, request: ModerateQuestionRequest): Observable<ProductQuestion> {
    return this.http.put<ProductQuestion>(`${this.apiUrl}/admin/questions/${questionId}/moderate`, request);
  }

  moderateAnswer(answerId: number, request: ModerateAnswerRequest): Observable<ProductAnswer> {
    return this.http.put<ProductAnswer>(`${this.apiUrl}/admin/answers/${answerId}/moderate`, request);
  }

  // ==================== User's Q&A ====================

  getMyQuestions(page: number = 1, pageSize: number = 10): Observable<ProductQuestion[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ProductQuestion[]>(`${this.apiUrl}/my/questions`, { params });
  }

  getMyAnswers(page: number = 1, pageSize: number = 10): Observable<ProductAnswer[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ProductAnswer[]>(`${this.apiUrl}/my/answers`, { params });
  }
}
