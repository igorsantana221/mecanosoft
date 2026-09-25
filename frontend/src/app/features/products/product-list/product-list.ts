import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, Product, ProductType } from '../../../core/services/product.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss'
})
export class ProductList implements OnInit {
  private productService = inject(ProductService);

  readonly ProductType = ProductType;

  searchTerm = signal('');
  isLoading = signal(true);
  loadError = signal<string | null>(null);
  products = signal<Product[]>([]);

  filteredProducts = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.products();
    return this.products().filter(p =>
      p.name.toLowerCase().includes(term) ||
      (p.sku ?? '').toLowerCase().includes(term) ||
      (p.category ?? '').toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.loadProducts();
  }

  private loadProducts() {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.productService.getProducts().subscribe({
      next: (data) => {
        this.products.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar produtos', err);
        this.loadError.set('Não foi possível carregar os produtos. Verifique a conexão.');
        this.isLoading.set(false);
      }
    });
  }

  deleteProduct(id: string | undefined) {
    if (!id) return;
    if (!confirm('Tem certeza que deseja excluir este produto?')) return;

    this.productService.deleteProduct(id).subscribe({
      next: () => {
        this.products.update(list => list.filter(p => p.id !== id));
      },
      error: (err) => {
        console.error('Erro ao excluir produto', err);
        alert('Erro ao excluir produto. Tente novamente.');
      }
    });
  }
}
