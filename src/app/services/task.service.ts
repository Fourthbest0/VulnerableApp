import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskDto, CreateTaskRequest } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private apiUrl = 'https://localhost:7157/api/tasks'; // Ajusta al puerto de tu API
  constructor(private client: HttpClient) {}

  getTasks(): Observable<TaskDto[]> {
    return this.client.get<TaskDto[]>(this.apiUrl);
  }

  createTask(request: CreateTaskRequest): Observable<TaskDto> {
    return this.client.post<TaskDto>(this.apiUrl, request);
  }

  completeTask(id: string): Observable<void> {
    return this.client.patch<void>(`${this.apiUrl}/${id}/complete`, {});
  }
}