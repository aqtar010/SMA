export interface ProductResponseDto {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  price: number;
  quantityAvailable: number;
}

export interface AdminProductResponseDto {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  price: number;
  isActive: boolean;
  quantityAvailable: number;
  quantityReserved: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductDto {
  sku: string;
  name: string;
  description?: string;
  price: number;
  initialStock: number;
}

export interface UpdateProductDto {
  sku?: string;
  name?: string;
  description?: string;
  price?: number;
  isActive?: boolean;
}

export interface UpdateStockDto {
  quantityAvailable: number;
}
