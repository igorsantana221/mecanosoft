import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuoteService } from '../../../core/services/quote.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Quote } from '../../../core/models/quote.model';

@Component({
  selector: 'app-quote-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './quote-list.html',
  styleUrl: './quote-list.scss'
})
export class QuoteList implements OnInit {
  private quoteService = inject(QuoteService);
  private notification = inject(NotificationService);

  searchTerm = signal('');
  statusFilter = signal<string>('Todos');
  isLoading = signal(true);

  quotes = signal<Quote[]>([]);

  ngOnInit() {
    this.loadQuotes();
  }

  loadQuotes() {
    this.isLoading.set(true);
    this.quoteService.getQuotes().subscribe({
      next: (data) => {
        this.quotes.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.notification.error('Erro ao carregar orçamentos.');
        this.isLoading.set(false);
      }
    });
  }

  filteredQuotes() {
    return this.quotes().filter(q => {
      const clientName = q.customerName || '';
      const number = q.number || '';
      const matchesSearch = clientName.toLowerCase().includes(this.searchTerm().toLowerCase()) ||
                           number.toLowerCase().includes(this.searchTerm().toLowerCase());
      const matchesStatus = this.statusFilter() === 'Todos' || q.status === this.statusFilter();
      return matchesSearch && matchesStatus;
    });
  }

  setStatusFilter(status: string) {
    this.statusFilter.set(status);
  }

  getStatusLabel(status: string | undefined): string {
    const map: Record<string, string> = {
      'Draft': 'Rascunho',
      'Pending': 'Pendente',
      'Paid': 'Pago',
      'Canceled': 'Cancelado'
    };
    return map[status || ''] || status || '';
  }

  formatDate(dateStr: string | undefined): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' });
  }
}
