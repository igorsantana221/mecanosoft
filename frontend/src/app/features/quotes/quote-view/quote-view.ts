import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { QuoteService } from '../../../core/services/quote.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Quote } from '../../../core/models/quote.model';

@Component({
  selector: 'app-quote-view',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './quote-view.html',
  styleUrl: './quote-view.scss'
})
export class QuoteView implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private quoteService = inject(QuoteService);
  private notification = inject(NotificationService);

  quote = signal<Quote | null>(null);
  isLoading = signal(true);
  isSendingEmail = signal(false);

  // Settings mock for printing logo/company info (in real app, this comes from Tenant/Company settings)
  companyInfo = {
    name: 'Minha Empresa Ltda',
    cnpj: '00.000.000/0001-00',
    email: 'contato@minhaempresa.com',
    phone: '(00) 00000-0000'
  };

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadQuote(id);
    } else {
      this.router.navigate(['/quotes']);
    }
  }

  loadQuote(id: string) {
    this.isLoading.set(true);
    this.quoteService.getQuote(id).subscribe({
      next: (data) => {
        this.quote.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.notification.error('Orçamento não encontrado.');
        this.router.navigate(['/quotes']);
      }
    });
  }

  printQuote() {
    window.print();
  }

  sendWhatsApp() {
    const q = this.quote();
    if (!q) return;

    // A real app might have customer.phone, here we prompt or use a default if missing.
    // Assuming we want the user to type the number or it opens web whatsapp
    
    let text = `Olá, ${q.customerName}!\n\nSegue o link/resumo da sua proposta comercial *${q.number}* - ${q.title}.\n`;
    text += `\n*Valor Total:* R$ ${q.total?.toFixed(2).replace('.', ',')}`;
    text += `\n*Validade:* ${q.validityDays} dias`;
    text += `\n\nFicamos à disposição para dúvidas!`;

    const encodedText = encodeURIComponent(text);
    const url = `https://wa.me/?text=${encodedText}`;
    window.open(url, '_blank');
  }

  sendEmail() {
    const q = this.quote();
    if (!q || !q.id) return;

    if (!confirm(`Deseja enviar este orçamento por e-mail para o cliente?`)) {
      return;
    }

    this.isSendingEmail.set(true);
    this.quoteService.sendEmail(q.id).subscribe({
      next: () => {
        this.notification.success('E-mail enviado com sucesso!');
        this.isSendingEmail.set(false);
      },
      error: (err) => {
        this.notification.error(err?.error || 'Erro ao enviar e-mail.');
        this.isSendingEmail.set(false);
      }
    });
  }

  getStatusLabel(status: string | undefined | number): string {
    const map: Record<string, string> = {
      'Draft': 'Rascunho',
      'Pending': 'Pendente',
      'Paid': 'Aprovado',
      'Canceled': 'Cancelado'
    };
    return map[String(status)] || String(status || '');
  }
}
