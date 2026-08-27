export interface ProductResponseDto {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  price: number;
  quantityAvailable: number;
  averageRating: number;
  ratingCount: number;
}

export interface ProductRatingSummaryDto {
  averageRating: number;
  ratingCount: number;
  currentUserRating: number | null;
  canRate: boolean;
}

export interface CreateProductRatingDto {
  rating: number;
  feedback?: string;
}

export interface ProductRatingResponseDto {
  id: string;
  productId: string;
  productName: string;
  customerName: string;
  customerEmail: string;
  rating: number;
  feedback: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PagedProductRatingResponseDto {
  items: ProductRatingResponseDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
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
