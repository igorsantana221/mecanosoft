import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { VehicleService } from '../../../core/services/vehicle.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Vehicle, VehicleLookupResponse } from '../../../core/models/vehicle.model';
import { VehicleFormModal } from '../../vehicles/vehicle-form-modal/vehicle-form-modal';

interface ViaCepResponse {
  logradouro?: string;
  bairro?: string;
  localidade?: string;
  uf?: string;
  erro?: boolean;
}

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, VehicleFormModal],
  templateUrl: './customer-form.html',
})
export class CustomerForm implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private customerService = inject(CustomerService);
  private vehicleService = inject(VehicleService);
  private notification = inject(NotificationService);
  private http = inject(HttpClient);

  editId = signal<string | null>(null);
  isEditMode = signal(false);
  activeTab = signal<'details' | 'vehicles'>('details');

  isLoading = signal(false);
  isSubmitting = signal(false);
  submitError = signal<string | null>(null);
  isSearchingCep = signal(false);
  cepError = signal<string | null>(null);

  // Vehicles linked to this customer (edit mode)
  vehicles = signal<Vehicle[]>([]);
  isLoadingVehicles = signal(false);

  // Optional vehicle section toggle (create mode)
  includeVehicle = signal(false);
  isConsultingPlate = signal(false);
  plateLookupStatus = signal<{ type: 'success' | 'warning' | 'error' | 'alreadyExists'; message: string } | null>(null);

  // Vehicle modal state (edit mode)
  isVehicleModalOpen = signal(false);
  selectedVehicle = signal<Vehicle | null>(null);
  vehicleToDelete = signal<Vehicle | null>(null);

  customerForm = this.fb.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required]],
    document: ['', [Validators.required]],
    type: ['Individual', [Validators.required]],
    isActive: [true],
    address: this.fb.group({
      zipCode: ['', [Validators.required]],
      street: ['', [Validators.required]],
      number: [''],
      complement: [''],
      neighborhood: [''],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
    }),
    vehicle: this.fb.group({
      licensePlate: [''],
      brand: [''],
      model: [''],
      year: [new Date().getFullYear()],
      color: [''],
      mileage: [0],
      chassis: [''],
      renavam: [''],
      notes: ['']
    })
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editId.set(id);
      this.isEditMode.set(true);
      this.loadCustomer(id);
      this.loadCustomerVehicles(id);
    }
  }

  toggleIncludeVehicle() {
    const nextVal = !this.includeVehicle();
    this.includeVehicle.set(nextVal);

    const vehicleGroup = this.customerForm.get('vehicle');
    if (nextVal) {
      vehicleGroup?.get('licensePlate')?.setValidators([Validators.required, Validators.maxLength(10)]);
      vehicleGroup?.get('brand')?.setValidators([Validators.required]);
      vehicleGroup?.get('model')?.setValidators([Validators.required]);
    } else {
      vehicleGroup?.get('licensePlate')?.clearValidators();
      vehicleGroup?.get('brand')?.clearValidators();
      vehicleGroup?.get('model')?.clearValidators();
    }
    vehicleGroup?.get('licensePlate')?.updateValueAndValidity();
    vehicleGroup?.get('brand')?.updateValueAndValidity();
    vehicleGroup?.get('model')?.updateValueAndValidity();
  }

  loadCustomer(id: string) {
    this.isLoading.set(true);
    this.customerService.getCustomer(id).subscribe({
      next: (customer: Customer) => {
        const parsed = this.parseAddressString(customer.address ?? '');

        this.customerForm.patchValue({
          name: customer.name,
          email: customer.email ?? '',
          phone: customer.phone ?? '',
          document: customer.document ?? '',
          address: {
            zipCode: parsed.zipCode,
            street: parsed.street,
            number: parsed.number,
            complement: '',
            neighborhood: parsed.neighborhood,
            city: parsed.city,
            state: parsed.state,
          }
        });

        if (customer.vehicles) {
          this.vehicles.set(customer.vehicles);
        }

        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Erro ao carregar cliente', err);
        this.submitError.set('Não foi possível carregar os dados do cliente.');
        this.isLoading.set(false);
      }
    });
  }

  loadCustomerVehicles(id: string) {
    this.isLoadingVehicles.set(true);
    this.vehicleService.getVehiclesByCustomer(id).subscribe({
      next: (data: Vehicle[]) => {
        this.vehicles.set(data);
        this.isLoadingVehicles.set(false);
      },
      error: () => {
        this.isLoadingVehicles.set(false);
      }
    });
  }

  private parseAddressString(address: string): {
    street: string; number: string; neighborhood: string; city: string; state: string; zipCode: string;
  } {
    const empty = { street: '', number: '', neighborhood: '', city: '', state: '', zipCode: '' };
    if (!address) return empty;

    try {
      const parts = address.split(',').map(p => p.trim());
      const street = parts[0] ?? '';
      const cityStatePart = parts[1] ?? '';
      const zipCode = parts[2]?.trim() ?? '';
      const [city, state] = cityStatePart.split(' - ').map(p => p.trim());
      return { street, number: '', neighborhood: '', city: city ?? '', state: state ?? '', zipCode };
    } catch {
      return { ...empty, street: address };
    }
  }

  onCepBlur() {
    const zipCode = this.customerForm.get('address.zipCode')?.value ?? '';
    const digits = zipCode.replace(/\D/g, '');
    if (digits.length !== 8) return;

    this.isSearchingCep.set(true);
    this.cepError.set(null);

    this.http.get<ViaCepResponse>(`https://viacep.com.br/ws/${digits}/json/`).subscribe({
      next: (data: ViaCepResponse) => {
        if (data.erro) {
          this.cepError.set('CEP não encontrado.');
          this.isSearchingCep.set(false);
          return;
        }
        this.customerForm.get('address')?.patchValue({
          zipCode: null,
          number: null,
          complement: null,
          street: data.logradouro ?? '',
          neighborhood: data.bairro ?? '',
          city: data.localidade ?? '',
          state: data.uf ?? '',
        });
        this.isSearchingCep.set(false);
      },
      error: () => {
        this.cepError.set('Erro ao buscar CEP. Preencha manualmente.');
        this.isSearchingCep.set(false);
      }
    });
  }

  onPlateInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const value = input.value.toUpperCase().replace(/[^A-Z0-9-]/g, '');
    this.customerForm.get('vehicle.licensePlate')?.setValue(value, { emitEvent: false });
    this.plateLookupStatus.set(null);
  }

  consultPlate() {
    const plate = (this.customerForm.get('vehicle.licensePlate')?.value || '').trim();
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

          const vGroup = this.customerForm.get('vehicle');
          vGroup?.patchValue({
            licensePlate: res.data.licensePlate || plate,
            brand: res.data.brand ?? vGroup.get('brand')?.value ?? '',
            model: res.data.model ?? vGroup.get('model')?.value ?? '',
            year: res.data.year ?? vGroup.get('year')?.value ?? new Date().getFullYear(),
            color: res.data.color ?? vGroup.get('color')?.value ?? '',
            mileage: vGroup.get('mileage')?.value ?? 0,
            chassis: res.data.chassis ?? vGroup.get('chassis')?.value ?? '',
            renavam: vGroup.get('renavam')?.value ?? '',
            notes: res.data.city ? `Município: ${res.data.city}/${res.data.state || ''}` : (vGroup.get('notes')?.value ?? '')
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

  onSubmit() {
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const formValue = this.customerForm.value;
    const addr = formValue.address;

    const parts: string[] = [];
    if (addr?.street) parts.push(addr.street + (addr.number ? `, ${addr.number}` : ''));
    if (addr?.neighborhood) parts.push(addr.neighborhood);
    const cityState = [addr?.city, addr?.state].filter(Boolean).join(' - ');
    if (cityState) parts.push(cityState);
    if (addr?.zipCode) parts.push(addr.zipCode);
    const addressString = parts.join(', ');

    const payload: Customer = {
      name: formValue.name ?? '',
      email: formValue.email ?? '',
      phone: formValue.phone ?? '',
      document: formValue.document ?? '',
      address: addressString
    };

    if (!this.isEditMode() && this.includeVehicle()) {
      const v = formValue.vehicle;
      if (v && v.licensePlate && v.brand && v.model) {
        payload.vehicle = {
          licensePlate: v.licensePlate.toUpperCase().trim(),
          brand: v.brand.trim(),
          model: v.model.trim(),
          year: v.year || new Date().getFullYear(),
          color: v.color?.trim(),
          mileage: v.mileage || 0,
          chassis: v.chassis?.trim(),
          renavam: v.renavam?.trim(),
          notes: v.notes?.trim()
        };
      }
    }

    const id = this.editId();
    const request$ = id
      ? this.customerService.updateCustomer(id, payload)
      : this.customerService.createCustomer(payload);

    request$.subscribe({
      next: () => {
        this.notification.success(id ? 'Cliente atualizado com sucesso!' : 'Cliente cadastrado com sucesso!');
        this.router.navigate(['/customers']);
      },
      error: (err: any) => {
        console.error('Erro ao salvar cliente', err);
        const msg = typeof err?.error === 'string' ? err.error : (err?.error?.message || 'Erro ao salvar cliente. Verifique os dados.');
        this.submitError.set(msg);
        this.notification.error(msg);
        this.isSubmitting.set(false);
      }
    });
  }

  // Vehicle Tab Actions in Edit Mode
  openAddVehicleModal() {
    this.selectedVehicle.set(null);
    this.isVehicleModalOpen.set(true);
  }

  openEditVehicleModal(v: Vehicle) {
    this.selectedVehicle.set(v);
    this.isVehicleModalOpen.set(true);
  }

  confirmDeleteVehicle(v: Vehicle) {
    this.vehicleToDelete.set(v);
  }

  cancelDeleteVehicle() {
    this.vehicleToDelete.set(null);
  }

  executeDeleteVehicle() {
    const v = this.vehicleToDelete();
    if (!v || !v.id) return;

    this.vehicleService.deleteVehicle(v.id).subscribe({
      next: () => {
        this.notification.success(`Veículo ${v.licensePlate} excluído com sucesso.`);
        this.vehicleToDelete.set(null);
        if (this.editId()) {
          this.loadCustomerVehicles(this.editId()!);
        }
      },
      error: (err: any) => {
        const msg = typeof err?.error === 'string' ? err.error : 'Erro ao excluir veículo.';
        this.notification.error(msg);
        this.vehicleToDelete.set(null);
      }
    });
  }

  onVehicleSaved() {
    if (this.editId()) {
      this.loadCustomerVehicles(this.editId()!);
    }
  }

  cancel() {
    this.router.navigate(['/customers']);
  }

  isInvalid(path: string): boolean {
    const ctrl = this.customerForm.get(path);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}
