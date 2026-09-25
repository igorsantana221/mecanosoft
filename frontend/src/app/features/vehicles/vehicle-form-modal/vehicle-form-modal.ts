import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { VehicleService } from '../../../core/services/vehicle.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Vehicle, VehicleLookupResponse } from '../../../core/models/vehicle.model';

@Component({
  selector: 'app-vehicle-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './vehicle-form-modal.html'
})
export class VehicleFormModal implements OnInit, OnChanges {
  private fb = inject(FormBuilder);
  private vehicleService = inject(VehicleService);
  private customerService = inject(CustomerService);
  private notification = inject(NotificationService);

  @Input() isOpen = false;
  @Input() vehicle: Vehicle | null = null;
  @Input() preselectedCustomerId: string | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<Vehicle>();

  customers = signal<Customer[]>([]);
  filteredCustomers = signal<Customer[]>([]);
  customerSearchText = signal('');
  isSearchingCustomer = signal(false);
  isSubmitting = signal(false);

  // Plate Lookup State
  isConsultingPlate = signal(false);
  plateLookupStatus = signal<{ type: 'success' | 'warning' | 'error' | 'alreadyExists'; message: string } | null>(null);

  vehicleForm = this.fb.group({
    customerId: ['', [Validators.required]],
    licensePlate: ['', [Validators.required, Validators.maxLength(10)]],
    brand: ['', [Validators.required, Validators.maxLength(100)]],
    model: ['', [Validators.required, Validators.maxLength(100)]],
    year: [new Date().getFullYear(), [Validators.required, Validators.min(1900), Validators.max(2100)]],
    color: [''],
    mileage: [0, [Validators.required, Validators.min(0)]],
    chassis: [''],
    renavam: [''],
    notes: ['']
  });

  ngOnInit() {
    this.loadCustomers();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['isOpen'] && this.isOpen) {
      this.resetAndPopulateForm();
    }
  }

  loadCustomers() {
    this.customerService.getCustomers().subscribe({
      next: (data: Customer[]) => {
        this.customers.set(data);
        this.filteredCustomers.set(data);
      }
    });
  }

  onCustomerSearch(event: Event) {
    const term = (event.target as HTMLInputElement).value.toLowerCase().trim();
    this.customerSearchText.set(term);
    this.isSearchingCustomer.set(true);

    if (!term) {
      this.filteredCustomers.set(this.customers());
      return;
    }

    const filtered = this.customers().filter(c =>
      c.name.toLowerCase().includes(term) ||
      (c.document && c.document.includes(term)) ||
      (c.phone && c.phone.includes(term))
    );
    this.filteredCustomers.set(filtered);
  }

  onPlateInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const value = input.value.toUpperCase().replace(/[^A-Z0-9-]/g, '');
    this.vehicleForm.patchValue({ licensePlate: value }, { emitEvent: false });
    this.plateLookupStatus.set(null);
  }

  consultPlate() {
    const plate = (this.vehicleForm.get('licensePlate')?.value || '').trim();
    if (!plate) {
      this.notification.error('Digite a placa do veículo para realizar a consulta.');
      return;
    }

    this.isConsultingPlate.set(true);
    this.plateLookupStatus.set(null);

    this.vehicleService.consultPlate(plate).subscribe({
      next: (res: VehicleLookupResponse) => {
        this.isConsultingPlate.set(false);

        if (res.alreadyExists) {
          this.plateLookupStatus.set({
            type: 'alreadyExists',
            message: res.message
          });
          this.notification.error(res.message);
          return;
        }

        if (res.success && res.data) {
          this.plateLookupStatus.set({
            type: 'success',
            message: '✓ Veículo encontrado! Dados preenchidos automaticamente.'
          });

          // Preenche os dados retornados no formulário
          this.vehicleForm.patchValue({
            licensePlate: res.data.licensePlate || plate,
            brand: res.data.brand ?? this.vehicleForm.get('brand')?.value ?? '',
            model: res.data.model ?? this.vehicleForm.get('model')?.value ?? '',
            year: res.data.year ?? this.vehicleForm.get('year')?.value ?? new Date().getFullYear(),
            color: res.data.color ?? this.vehicleForm.get('color')?.value ?? '',
            chassis: res.data.chassis ?? this.vehicleForm.get('chassis')?.value ?? '',
            notes: res.data.city ? `Município: ${res.data.city}/${res.data.state || ''}` : (this.vehicleForm.get('notes')?.value ?? '')
          });

          this.notification.success('Dados do veículo preenchidos automaticamente!');
        } else {
          this.plateLookupStatus.set({
            type: 'warning',
            message: res.message || '⚠ Não foi possível localizar os dados desta placa.'
          });
        }
      },
      error: () => {
        this.isConsultingPlate.set(false);
        this.plateLookupStatus.set({
          type: 'error',
          message: '⚠ Não foi possível consultar a placa no momento. Você pode prosseguir com o cadastro manual.'
        });
      }
    });
  }

  resetAndPopulateForm() {
    this.plateLookupStatus.set(null);
    const targetCustId = this.preselectedCustomerId || (this.vehicle ? this.vehicle.customerId : '');

    this.vehicleForm.reset({
      customerId: targetCustId,
      licensePlate: this.vehicle?.licensePlate || '',
      brand: this.vehicle?.brand || '',
      model: this.vehicle?.model || '',
      year: this.vehicle?.year || new Date().getFullYear(),
      color: this.vehicle?.color || '',
      mileage: this.vehicle?.mileage || 0,
      chassis: this.vehicle?.chassis || '',
      renavam: this.vehicle?.renavam || '',
      notes: this.vehicle?.notes || ''
    });

    if (this.preselectedCustomerId) {
      const selected = this.customers().find(c => c.id === this.preselectedCustomerId);
      if (selected) {
        this.customerSearchText.set(selected.name);
      }
    } else if (this.vehicle && this.vehicle.customerName) {
      this.customerSearchText.set(this.vehicle.customerName);
    } else {
      this.customerSearchText.set('');
    }
  }

  selectCustomer(customer: Customer) {
    if (customer.id) {
      this.vehicleForm.patchValue({ customerId: customer.id });
      this.customerSearchText.set(customer.name);
      this.isSearchingCustomer.set(false);
    }
  }

  getSelectedCustomerName(): string {
    const id = this.vehicleForm.get('customerId')?.value;
    if (!id) return '';
    const customer = this.customers().find(c => c.id === id);
    return customer ? customer.name : '';
  }

  onSubmit() {
    if (this.vehicleForm.invalid) {
      this.vehicleForm.markAllAsTouched();
      this.notification.error('Preencha os campos obrigatórios do veículo.');
      return;
    }

    this.isSubmitting.set(true);
    const formVal = this.vehicleForm.value;

    const payload: Vehicle = {
      customerId: formVal.customerId!,
      licensePlate: (formVal.licensePlate || '').toUpperCase().trim(),
      brand: (formVal.brand || '').trim(),
      model: (formVal.model || '').trim(),
      year: formVal.year || new Date().getFullYear(),
      color: (formVal.color || '').trim(),
      mileage: formVal.mileage || 0,
      chassis: (formVal.chassis || '').trim(),
      renavam: (formVal.renavam || '').trim(),
      notes: (formVal.notes || '').trim()
    };

    if (this.vehicle && this.vehicle.id) {
      this.vehicleService.updateVehicle(this.vehicle.id, payload).subscribe({
        next: (res: Vehicle) => {
          this.notification.success('Veículo atualizado com sucesso!');
          this.isSubmitting.set(false);
          this.saved.emit(res);
          this.close();
        },
        error: (err: any) => {
          const msg = typeof err?.error === 'string' ? err.error : (err?.error?.message || 'Erro ao atualizar veículo.');
          this.notification.error(msg);
          this.isSubmitting.set(false);
        }
      });
    } else {
      this.vehicleService.createVehicle(payload).subscribe({
        next: (res: Vehicle) => {
          this.notification.success('Veículo cadastrado com sucesso!');
          this.isSubmitting.set(false);
          this.saved.emit(res);
          this.close();
        },
        error: (err: any) => {
          const msg = typeof err?.error === 'string' ? err.error : (err?.error?.message || 'Erro ao cadastrar veículo.');
          this.notification.error(msg);
          this.isSubmitting.set(false);
        }
      });
    }
  }

  close() {
    this.closed.emit();
  }
}
