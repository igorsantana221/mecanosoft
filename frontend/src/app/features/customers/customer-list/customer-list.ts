import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CustomerService, Customer } from '../../../core/services/customer.service';

type DocumentFilter = 'all' | 'cpf' | 'cnpj';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-list.html',
})
export class CustomerList implements OnInit {
  private customerService = inject(CustomerService);

  // Expor Math para uso no template
  readonly Math = Math;

  // --- State ---
  allCustomers = signal<Customer[]>([]);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  // --- Filter signals ---
  searchQuery = signal<string>('');
  documentFilter = signal<DocumentFilter>('all');
  dateFilter = signal<string>('');

  // --- Pagination ---
  currentPage = signal<number>(1);
  readonly pageSize = 10;

  // --- Computed: filtered list (client-side, multi-tenant data already isolated by JWT on backend) ---
  filteredCustomers = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const docFilter = this.documentFilter();
    const date = this.dateFilter();

    return this.allCustomers().filter(c => {
      // Busca por nome, email ou telefone
      const matchSearch = !query ||
        c.name.toLowerCase().includes(query) ||
        (c.email?.toLowerCase().includes(query) ?? false) ||
        (c.phone?.includes(query) ?? false);

      // Filtro por tipo de documento (CPF = 11 dígitos, CNPJ = 14 dígitos)
      const docDigits = c.document?.replace(/\D/g, '') ?? '';
      const matchDoc =
        docFilter === 'all' ||
        (docFilter === 'cpf' && docDigits.length === 11) ||
        (docFilter === 'cnpj' && docDigits.length === 14);

      // Filtro por data de cadastro (compara só a parte da data)
      const matchDate = !date || (c.createdAt?.startsWith(date) ?? false);

      return matchSearch && matchDoc && matchDate;
    });
  });

  // --- Computed: paginação ---
  totalPages = computed(() => Math.max(1, Math.ceil(this.filteredCustomers().length / this.pageSize)));

  paginatedCustomers = computed(() => {
    const page = this.currentPage();
    const start = (page - 1) * this.pageSize;
    return this.filteredCustomers().slice(start, start + this.pageSize);
  });

  pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i + 1)
  );

  // --- Stats computados ---
  totalActive = computed(() => this.allCustomers().length);
  totalWithEmail = computed(() => this.allCustomers().filter(c => c.email).length);
  newestCustomerDate = computed(() => {
    const sorted = [...this.allCustomers()].sort((a, b) =>
      new Date(b.createdAt ?? 0).getTime() - new Date(a.createdAt ?? 0).getTime()
    );
    return sorted[0]?.createdAt ? new Date(sorted[0].createdAt).toLocaleDateString('pt-BR') : '-';
  });

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.customerService.getCustomers().subscribe({
      next: (data) => {
        this.allCustomers.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar clientes', err);
        this.errorMessage.set('Não foi possível carregar os clientes. Verifique sua conexão.');
        this.isLoading.set(false);
      }
    });
  }

  // --- Filter actions ---
  setDocumentFilter(filter: DocumentFilter) {
    this.documentFilter.set(filter);
    this.currentPage.set(1);
  }

  onSearchChange(value: string) {
    this.searchQuery.set(value);
    this.currentPage.set(1);
  }

  onDateChange(value: string) {
    this.dateFilter.set(value);
    this.currentPage.set(1);
  }

  clearFilters() {
    this.searchQuery.set('');
    this.documentFilter.set('all');
    this.dateFilter.set('');
    this.currentPage.set(1);
  }

  hasActiveFilters = computed(() =>
    this.searchQuery() !== '' || this.documentFilter() !== 'all' || this.dateFilter() !== ''
  );

  // --- Pagination actions ---
  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  // --- Helpers ---
  getInitials(name: string): string {
    if (!name) return '??';
    const parts = name.trim().split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  getAvatarColor(name: string): string {
    const colors = [
      'bg-blue-100 text-blue-700',
      'bg-emerald-100 text-emerald-700',
      'bg-amber-100 text-amber-700',
      'bg-rose-100 text-rose-700',
      'bg-cyan-100 text-cyan-700',
      'bg-violet-100 text-violet-700',
    ];
    const idx = name.charCodeAt(0) % colors.length;
    return colors[idx];
  }

  getDocumentType(document?: string): string {
    if (!document) return '';
    const digits = document.replace(/\D/g, '');
    if (digits.length === 11) return 'CPF';
    if (digits.length === 14) return 'CNPJ';
    return '';
  }

  formatDate(dateStr?: string): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('pt-BR');
  }

  deleteCustomer(id: string | undefined) {
    if (!id) return;
    if (confirm('Tem certeza que deseja excluir este cliente?')) {
      this.customerService.deleteCustomer(id).subscribe({
        next: () => this.loadCustomers(),
        error: (err) => console.error('Erro ao deletar', err)
      });
    }
  }
}
