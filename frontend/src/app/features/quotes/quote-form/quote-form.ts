import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormArray } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { QuoteService } from '../../../core/services/quote.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Quote } from '../../../core/models/quote.model';

@Component({
  selector: 'app-quote-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './quote-form.html',
  styleUrl: './quote-form.scss'
})
export class QuoteForm implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private quoteService = inject(QuoteService);
  private customerService = inject(CustomerService);
  private notification = inject(NotificationService);

  customers = signal<Customer[]>([]);
  isLoading = signal(false);
  isLoadingCustomers = signal(true);
  isEditMode = signal(false);
  quoteId = signal<string | null>(null);

  quoteForm = this.fb.group({
    customerId: ['', [Validators.required]],
    title: ['', [Validators.required]],
    issueDate: [new Date().toISOString().substring(0, 10), [Validators.required]],
    validityDays: [15, [Validators.required, Validators.min(1)]],
    items: this.fb.array([]),
    notes: [''],
    discount: [0],
    tax: [0],
    status: ['Draft']
  });

  get items() {
    return this.quoteForm.get('items') as FormArray;
  }

  constructor() { }

  ngOnInit() {
    this.loadCustomers();
    
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.quoteId.set(id);
      this.loadQuote(id);
    } else {
      this.addItem();
    }
  }

  loadCustomers() {
    this.isLoadingCustomers.set(true);
    this.customerService.getCustomers().subscribe({
      next: (data) => {
        this.customers.set(data);
        this.isLoadingCustomers.set(false);
      },
      error: () => {
        this.notification.error('Erro ao carregar lista de clientes.');
        this.isLoadingCustomers.set(false);
      }
    });
  }

  loadQuote(id: string) {
    this.isLoading.set(true);
    this.quoteService.getQuote(id).subscribe({
      next: (quote) => {
        // Clear items first
        while (this.items.length) {
          this.items.removeAt(0);
        }

        // Add items from quote
        if (quote.items && quote.items.length > 0) {
          quote.items.forEach(item => {
            const itemForm = this.fb.group({
              description: [item.description, Validators.required],
              details: [item.details || ''],
              quantity: [item.quantity, [Validators.required, Validators.min(1)]],
              unitPrice: [item.unitPrice, [Validators.required, Validators.min(0)]]
            });
            this.items.push(itemForm);
          });
        } else {
          this.addItem();
        }

        this.quoteForm.patchValue({
          customerId: quote.customerId,
          title: quote.title,
          issueDate: quote.issueDate ? quote.issueDate.substring(0, 10) : new Date().toISOString().substring(0, 10),
          validityDays: quote.validityDays,
          notes: quote.notes || '',
          discount: quote.discount,
          tax: quote.tax,
          status: quote.status
        });

        this.isLoading.set(false);
      },
      error: () => {
        this.notification.error('Erro ao carregar orçamento.');
        this.isLoading.set(false);
        this.router.navigate(['/quotes']);
      }
    });
  }

  addItem() {
    const itemForm = this.fb.group({
      description: ['', Validators.required],
      details: [''],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]]
    });
    this.items.push(itemForm);
  }

  removeItem(index: number) {
    if (this.items.length > 1) {
      this.items.removeAt(index);
    }
  }

  calculateSubtotal() {
    let subtotal = 0;
    this.items.controls.forEach(control => {
      const quantity = control.get('quantity')?.value || 0;
      const price = control.get('unitPrice')?.value || 0;
      subtotal += quantity * price;
    });
    return subtotal;
  }

  calculateTotal() {
    const subtotal = this.calculateSubtotal();
    const discount = this.quoteForm.get('discount')?.value || 0;
    const tax = this.quoteForm.get('tax')?.value || 0;

    const withDiscount = subtotal * (1 - discount / 100);
    const withTax = withDiscount * (1 + tax / 100);

    return withTax;
  }

  getStatusMap() {
    return {
      'Draft': 0,
      'Pending': 1,
      'Paid': 2,
      'Canceled': 3
    };
  }

  onSubmit() {
    if (this.quoteForm.valid) {
      this.isLoading.set(true);

      const formValue = this.quoteForm.value;
      const statusMap = this.getStatusMap();
      const statusStr = formValue.status || 'Draft';
      const statusEnum = (statusMap as any)[statusStr] ?? 0;

      const payload: Quote | any = {
        title: formValue.title || '',
        customerId: formValue.customerId || '',
        issueDate: formValue.issueDate || new Date().toISOString().substring(0, 10),
        validityDays: formValue.validityDays || 15,
        notes: formValue.notes || '',
        discount: formValue.discount || 0,
        tax: formValue.tax || 0,
        status: statusEnum,
        items: (formValue.items || []).map((item: any) => ({
          description: item.description || '',
          details: item.details || '',
          quantity: item.quantity || 1,
          unitPrice: item.unitPrice || 0
        }))
      };

      if (this.isEditMode() && this.quoteId()) {
        this.quoteService.updateQuote(this.quoteId()!, payload).subscribe({
          next: () => {
            this.notification.success('Orçamento atualizado com sucesso!');
            this.isLoading.set(false);
            this.router.navigate(['/quotes', this.quoteId()]);
          },
          error: (err) => {
            this.notification.error(err?.error?.message || 'Erro ao atualizar orçamento.');
            this.isLoading.set(false);
          }
        });
      } else {
        this.quoteService.createQuote(payload).subscribe({
          next: (res) => {
            this.notification.success('Orçamento salvo com sucesso!');
            this.isLoading.set(false);
            this.router.navigate(['/quotes', res.id]);
          },
          error: (err) => {
            this.notification.error(err?.error?.message || 'Erro ao salvar orçamento.');
            this.isLoading.set(false);
          }
        });
      }
    } else {
      this.quoteForm.markAllAsTouched();
      this.notification.error('Preencha todos os campos obrigatórios.');
    }
  }

  cancel() {
    this.router.navigate(['/quotes']);
  }
}
