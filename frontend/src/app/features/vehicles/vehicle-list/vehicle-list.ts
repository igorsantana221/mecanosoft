import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { VehicleService } from '../../../core/services/vehicle.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Vehicle } from '../../../core/models/vehicle.model';
import { VehicleFormModal } from '../vehicle-form-modal/vehicle-form-modal';

@Component({
  selector: 'app-vehicle-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, VehicleFormModal],
  templateUrl: './vehicle-list.html'
})
export class VehicleList implements OnInit {
  private vehicleService = inject(VehicleService);
  private customerService = inject(CustomerService);
  private notification = inject(NotificationService);

  vehicles = signal<Vehicle[]>([]);
  customers = signal<Customer[]>([]);
  isLoading = signal(true);

  // Filters
  searchTerm = signal('');
  plateFilter = signal('');
  brandFilter = signal('');
  modelFilter = signal('');
  selectedCustomerId = signal('');

  // Modal State
  isModalOpen = signal(false);
  selectedVehicle = signal<Vehicle | null>(null);

  // Delete Confirm Modal State
  vehicleToDelete = signal<Vehicle | null>(null);

  ngOnInit() {
    this.loadCustomers();
    this.loadVehicles();
  }

  loadCustomers() {
    this.customerService.getCustomers().subscribe({
      next: (data: Customer[]) => this.customers.set(data)
    });
  }

  loadVehicles() {
    this.isLoading.set(true);
    this.vehicleService.getVehicles({
      search: this.searchTerm(),
      licensePlate: this.plateFilter(),
      brand: this.brandFilter(),
      model: this.modelFilter(),
      customerId: this.selectedCustomerId()
    }).subscribe({
      next: (data: Vehicle[]) => {
        this.vehicles.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.notification.error('Erro ao carregar lista de veículos.');
        this.isLoading.set(false);
      }
    });
  }

  onFilterChange() {
    this.loadVehicles();
  }

  clearFilters() {
    this.searchTerm.set('');
    this.plateFilter.set('');
    this.brandFilter.set('');
    this.modelFilter.set('');
    this.selectedCustomerId.set('');
    this.loadVehicles();
  }

  openAddModal() {
    this.selectedVehicle.set(null);
    this.isModalOpen.set(true);
  }

  openEditModal(vehicle: Vehicle) {
    this.selectedVehicle.set(vehicle);
    this.isModalOpen.set(true);
  }

  confirmDelete(vehicle: Vehicle) {
    this.vehicleToDelete.set(vehicle);
  }

  cancelDelete() {
    this.vehicleToDelete.set(null);
  }

  executeDelete() {
    const v = this.vehicleToDelete();
    if (!v || !v.id) return;

    this.vehicleService.deleteVehicle(v.id).subscribe({
      next: () => {
        this.notification.success(`Veículo ${v.licensePlate} excluído com sucesso.`);
        this.vehicleToDelete.set(null);
        this.loadVehicles();
      },
      error: (err: any) => {
        const msg = typeof err?.error === 'string' ? err.error : 'Erro ao excluir veículo.';
        this.notification.error(msg);
        this.vehicleToDelete.set(null);
      }
    });
  }

  onVehicleSaved() {
    this.loadVehicles();
  }
}
