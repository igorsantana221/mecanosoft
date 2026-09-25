export interface QuoteItem {
  id?: string;
  description: string;
  details?: string;
  quantity: number;
  unitPrice: number;
  subtotal?: number;
}

export interface Quote {
  id?: string;
  title: string;
  number?: string;
  customerId: string;
  customerName?: string;
  issueDate: string;
  validityDays: number;
  status?: string;
  notes?: string;
  discount: number;
  tax: number;
  total?: number;
  createdAt?: string;
  updatedAt?: string;
  items: QuoteItem[];
}
