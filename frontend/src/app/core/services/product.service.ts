import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export enum ProductType {
  Product = 0,
  Service = 1
}

export interface Product {
  id?: string;
  name: string;
  sku?: string;
  description?: string;
  category?: string;
  unit: string;
  type: ProductType;
  isActive: boolean;
  imageUrl?: string;
  costPrice: number;
  salePrice: number;
  // Fiscal — NF-e (Produto)
  ncm?: string;
  cfop?: string;
  cst?: string;
  origin?: string;
  icmsRate?: number;
  ipiRate?: number;
  pisRate?: number;
  cofinsRate?: number;
  // Fiscal — NFS-e (Serviço)
  serviceCode?: string;
  issqnRate?: number;
  retainIss: boolean;
  createdAt?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:44329/api/Products';

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  createProduct(product: Omit<Product, 'id' | 'createdAt' | 'updatedAt'>): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  updateProduct(id: string, product: Omit<Product, 'id' | 'createdAt' | 'updatedAt'>): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, product);
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  uploadImage(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.apiUrl}/upload-image`, formData);
  }
}
