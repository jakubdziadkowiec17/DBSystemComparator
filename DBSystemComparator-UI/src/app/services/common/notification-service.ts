import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  constructor(private messageService: MessageService) {}

  showSuccessToast(detail: string): void {
    var summary = 'Success';
    this.messageService.add({ severity: 'success', summary: summary, detail: detail, life: 5000 });
  }

  showErrorToast(detail: string): void {
    var summary = 'Error';
    this.messageService.add({ severity: 'error', summary: summary, detail: detail, life: 5000 });
  }
}