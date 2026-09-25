import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { publicGuard } from './core/guards/public.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [publicGuard],
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    canActivate: [publicGuard],
    loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
  },
  {
    path: 'recovery',
    canActivate: [publicGuard],
    loadComponent: () => import('./features/auth/recovery/recovery').then(m => m.Recovery)
  },
  {
    path: 'reset-password',
    canActivate: [publicGuard],
    loadComponent: () => import('./features/auth/reset-password/reset-password').then(m => m.ResetPassword)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./core/layout/main-layout/main-layout').then(m => m.MainLayout),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard/dashboard').then(m => m.Dashboard)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/products/product-list/product-list').then(m => m.ProductList)
      },
      {
        path: 'products/new',
        loadComponent: () => import('./features/products/product-form/product-form').then(m => m.ProductForm)
      },
      {
        path: 'products/:id/edit',
        loadComponent: () => import('./features/products/product-form/product-form').then(m => m.ProductForm)
      },
      {
        path: 'quotes',
        loadComponent: () => import('./features/quotes/quote-list/quote-list').then(m => m.QuoteList)
      },
      {
        path: 'quotes/new',
        loadComponent: () => import('./features/quotes/quote-form/quote-form').then(m => m.QuoteForm)
      },
      {
        path: 'quotes/:id',
        loadComponent: () => import('./features/quotes/quote-view/quote-view').then(m => m.QuoteView)
      },
      {
        path: 'quotes/:id/edit',
        loadComponent: () => import('./features/quotes/quote-form/quote-form').then(m => m.QuoteForm)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile/profile').then(m => m.Profile)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/customer-list/customer-list').then(m => m.CustomerList)
      },
      {
        path: 'customers/new',
        loadComponent: () => import('./features/customers/customer-form/customer-form').then(m => m.CustomerForm)
      },
      {
        path: 'customers/:id/edit',
        loadComponent: () => import('./features/customers/customer-form/customer-form').then(m => m.CustomerForm)
      },
      {
        path: 'vehicles',
        loadComponent: () => import('./features/vehicles/vehicle-list/vehicle-list').then(m => m.VehicleList)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];

