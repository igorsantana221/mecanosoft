import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { ProductService, ProductType } from '../../../core/services/product.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss'
})
export class ProductForm implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);

  readonly ProductType = ProductType;

  /** ID do produto quando em modo edição (null = modo criação) */
  editId = signal<string | null>(null);
  isEditMode = signal(false);

  isLoading = signal(false);
  isSubmitting = signal(false);
  submitError = signal<string | null>(null);
  activeTab = signal<'general' | 'fiscal' | 'pricing'>('general');
  isUploadingImage = signal(false);
  previewImageUrl = signal<string | null>(null);

  productForm = this.fb.group({
    // ─── Gerais
    name: ['', [Validators.required, Validators.maxLength(200)]],
    sku: [''],
    description: [''],
    category: [''],
    unit: ['UN', [Validators.required]],
    type: [ProductType.Product, [Validators.required]],
    isActive: [true],
    imageUrl: [''],

    // ─── Preços
    costPrice: [0, [Validators.min(0)]],
    salePrice: [0, [Validators.required, Validators.min(0)]],

    // ─── Fiscal Produto (NF-e)
    ncm: ['', [Validators.maxLength(10)]],
    cfop: ['', [Validators.maxLength(10)]],
    cst: ['', [Validators.maxLength(10)]],
    origin: ['0'],
    icmsRate: [null as number | null, [Validators.min(0), Validators.max(100)]],
    ipiRate: [null as number | null, [Validators.min(0), Validators.max(100)]],
    pisRate: [null as number | null, [Validators.min(0), Validators.max(100)]],
    cofinsRate: [null as number | null, [Validators.min(0), Validators.max(100)]],

    // ─── Fiscal Serviço (NFS-e)
    serviceCode: ['', [Validators.maxLength(10)]],
    issqnRate: [null as number | null, [Validators.min(0), Validators.max(100)]],
    retainIss: [false]
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editId.set(id);
      this.isEditMode.set(true);
      this.loadProduct(id);
    }
  }

  private loadProduct(id: string) {
    this.isLoading.set(true);
    this.productService.getProduct(id).subscribe({
      next: (product) => {
        this.productForm.patchValue({
          name: product.name,
          sku: product.sku ?? '',
          description: product.description ?? '',
          category: product.category ?? '',
          unit: product.unit,
          type: product.type,
          isActive: product.isActive,
          imageUrl: product.imageUrl ?? '',
          costPrice: product.costPrice,
          salePrice: product.salePrice,
          ncm: product.ncm ?? '',
          cfop: product.cfop ?? '',
          cst: product.cst ?? '',
          origin: product.origin ?? '0',
          icmsRate: product.icmsRate ?? null,
          ipiRate: product.ipiRate ?? null,
          pisRate: product.pisRate ?? null,
          cofinsRate: product.cofinsRate ?? null,
          serviceCode: product.serviceCode ?? '',
          issqnRate: product.issqnRate ?? null,
          retainIss: product.retainIss
        });
        if (product.imageUrl) {
          this.previewImageUrl.set(product.imageUrl);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar produto', err);
        this.submitError.set('Não foi possível carregar os dados do produto.');
        this.isLoading.set(false);
      }
    });
  }

  get isService(): boolean {
    return this.productForm.get('type')?.value === ProductType.Service;
  }

  get margin(): number {
    const cost = +(this.productForm.get('costPrice')?.value ?? 0);
    const sale = +(this.productForm.get('salePrice')?.value ?? 0);
    if (cost === 0 || sale === 0) return 0;
    return ((sale - cost) / sale) * 100;
  }

  get markup(): number {
    const cost = +(this.productForm.get('costPrice')?.value ?? 0);
    const sale = +(this.productForm.get('salePrice')?.value ?? 0);
    if (cost === 0) return 0;
    return ((sale - cost) / cost) * 100;
  }

  setTab(tab: 'general' | 'fiscal' | 'pricing') {
    this.activeTab.set(tab);
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      alert('O arquivo não pode ter mais de 5MB.');
      return;
    }

    this.isUploadingImage.set(true);
    const reader = new FileReader();
    reader.onload = (e) => this.previewImageUrl.set(e.target?.result as string);
    reader.readAsDataURL(file);

    this.productService.uploadImage(file).subscribe({
      next: (res) => {
        const fullUrl = `https://localhost:44329${res.url}`;
        this.productForm.patchValue({ imageUrl: fullUrl });
        this.isUploadingImage.set(false);
      },
      error: (err) => {
        console.error('Erro no upload', err);
        alert('Erro ao enviar imagem.');
        this.isUploadingImage.set(false);
        this.previewImageUrl.set(null);
      }
    });
  }

  onSubmit() {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const v = this.productForm.value;

    const payload = {
      name: v.name!,
      sku: v.sku || undefined,
      description: v.description || undefined,
      category: v.category || undefined,
      unit: v.unit!,
      type: v.type!,
      isActive: v.isActive ?? true,
      imageUrl: v.imageUrl || undefined,
      costPrice: +(v.costPrice ?? 0),
      salePrice: +(v.salePrice ?? 0),
      ncm: v.ncm || undefined,
      cfop: v.cfop || undefined,
      cst: v.cst || undefined,
      origin: v.origin || undefined,
      icmsRate: v.icmsRate ?? undefined,
      ipiRate: v.ipiRate ?? undefined,
      pisRate: v.pisRate ?? undefined,
      cofinsRate: v.cofinsRate ?? undefined,
      serviceCode: v.serviceCode || undefined,
      issqnRate: v.issqnRate ?? undefined,
      retainIss: v.retainIss ?? false
    };

    const id = this.editId();
    const request$ = id
      ? this.productService.updateProduct(id, payload)
      : this.productService.createProduct(payload);

    request$.subscribe({
      next: () => {
        this.router.navigate(['/products']);
      },
      error: (err) => {
        console.error('Erro ao salvar produto', err);
        this.submitError.set('Erro ao salvar produto. Verifique os dados e tente novamente.');
        this.isSubmitting.set(false);
      }
    });
  }

  cancel() {
    this.router.navigate(['/products']);
  }

  isInvalid(controlName: string): boolean {
    const ctrl = this.productForm.get(controlName);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}
