export interface Vehicle {
  id?: string;
  customerId: string;
  customerName?: string;
  customerPhone?: string;
  customerDocument?: string;
  licensePlate: string;
  brand: string;
  model: string;
  year: number;
  color?: string;
  mileage: number;
  chassis?: string;
  renavam?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface OptionalVehicle {
  licensePlate: string;
  brand: string;
  model: string;
  year: number;
  color?: string;
  mileage: number;
  chassis?: string;
  renavam?: string;
  notes?: string;
}

export interface VehicleLookupData {
  licensePlate: string;
  brand: string;
  model: string;
  version?: string;
  year: number;
  modelYear?: number;
  color?: string;
  fuel?: string;
  chassis?: string;
  city?: string;
  state?: string;
  vehicleType?: string;
  engine?: string;
}

export interface VehicleLookupResponse {
  success: boolean;
  alreadyExists: boolean;
  existingCustomerName?: string;
  message: string;
  data?: VehicleLookupData;
}
