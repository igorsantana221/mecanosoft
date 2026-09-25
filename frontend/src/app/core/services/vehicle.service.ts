import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Vehicle } from '../models/vehicle.model';

export interface VehicleFilterParams {
  search?: string;
  licensePlate?: string;
  brand?: string;
  model?: string;
  customerId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:44329/api/Vehicles';

  getVehicles(filters?: VehicleFilterParams): Observable<Vehicle[]> {
    let params = new HttpParams();

    if (filters) {
      if (filters.search) params = params.set('search', filters.search);
      if (filters.licensePlate) params = params.set('licensePlate', filters.licensePlate);
      if (filters.brand) params = params.set('brand', filters.brand);
      if (filters.model) params = params.set('model', filters.model);
      if (filters.customerId) params = params.set('customerId', filters.customerId);
    }

    return this.http.get<Vehicle[]>(this.apiUrl, { params });
  }

  getVehicle(id: string): Observable<Vehicle> {
    return this.http.get<Vehicle>(`${this.apiUrl}/${id}`);
  }

  getVehiclesByCustomer(customerId: string): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(`https://localhost:44329/api/customers/${customerId}/vehicles`);
  }

  createVehicle(vehicle: Vehicle): Observable<Vehicle> {
    return this.http.post<Vehicle>(this.apiUrl, vehicle);
  }

  updateVehicle(id: string, vehicle: Vehicle): Observable<Vehicle> {
    return this.http.put<Vehicle>(`${this.apiUrl}/${id}`, vehicle);
  }

  deleteVehicle(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  consultPlate(licensePlate: string): Observable<import('../models/vehicle.model').VehicleLookupResponse> {
    return this.http.post<import('../models/vehicle.model').VehicleLookupResponse>(`${this.apiUrl}/consult-plate`, { licensePlate });
  }
}
