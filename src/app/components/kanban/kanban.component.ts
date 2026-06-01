import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TaskDto } from '../../models/task.model';
import { TaskService } from '../../services/task.service';

@Component({
  selector: 'app-kanban',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule, MatCardModule, MatIconModule, MatButtonModule],
  templateUrl: './kanban.component.html',
  styleUrls: ['./kanban.component.scss']
})
export class KanbanComponent implements OnInit {
  todo: TaskDto[] = [];
  done: TaskDto[] = [];

  dialogVisible = false;
  newTitle = '';
  newDescription = '';
  saving = false;
  submitted = false;
  toast: { message: string, type: 'success' | 'error' } | null = null;

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks() {
    this.taskService.getTasks().subscribe({
      next: (tasks) => {
        this.todo = tasks.filter(t => !t.isCompleted);
        this.done = tasks.filter(t => t.isCompleted);
      },
      error: () => this.showToast('Error al cargar las tareas', 'error')
    });
  }

  openDialog() {
    this.dialogVisible = true;
    this.newTitle = '';
    this.newDescription = '';
    this.submitted = false;
    this.saving = false;
  }

  cancelDialog() {
    this.dialogVisible = false;
  }

  saveTask() {
    this.submitted = true;
    if (!this.newTitle.trim() || !this.newDescription.trim()) return;
    if (this.saving) return;
    this.saving = true;

    this.taskService.createTask({
      title: this.newTitle.trim(),
      description: this.newDescription.trim()
    }).subscribe({
      next: (task) => {
        this.todo.push(task);
        this.dialogVisible = false;
        this.saving = false;
        this.showToast('Tarea creada exitosamente', 'success');
      },
      error: () => {
        this.saving = false;
        this.showToast('Error al guardar la tarea', 'error');
      }
    });
  }

  showToast(message: string, type: 'success' | 'error') {
    this.toast = { message, type };
    setTimeout(() => this.toast = null, 3000);
  }

  drop(event: CdkDragDrop<TaskDto[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const task = event.previousContainer.data[event.previousIndex];
      this.taskService.completeTask(task.id).subscribe({
        next: () => { 
          transferArrayItem(
            event.previousContainer.data,
            event.container.data,
            event.previousIndex,
            event.currentIndex
          );
          const vaACompletadas = event.container.data === this.done;
          const mensaje = vaACompletadas ? 'Tarea completada' : 'Tarea regresada a pendientes';
          this.showToast(mensaje, 'success');
        },
        error: () => this.showToast('No se pudo actualizar la tarea', 'error')
      });
    }
  }
}