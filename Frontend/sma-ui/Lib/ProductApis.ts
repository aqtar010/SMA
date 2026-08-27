import { api } from "./api/api";
import {
  AdminProductResponseDto,
  CreateProductDto,
  ProductResponseDto,
  UpdateProductDto,
  UpdateStockDto,
  CreateProductRatingDto,
  ProductRatingSummaryDto,
  PagedProductRatingResponseDto,
} from "@/DTOs/ProductDTOs";

export async function fetchProducts(): Promise<ProductResponseDto[]> {
  const res = await api.get<ProductResponseDto[]>("/products");
  return res.data;
}

export async function fetchAdminProducts(): Promise<AdminProductResponseDto[]> {
  const res = await api.get<AdminProductResponseDto[]>("/admin/products");
  return res.data;
}

export async function createProduct(
  dto: CreateProductDto,
): Promise<ProductResponseDto> {
  const res = await api.post<ProductResponseDto>("/admin/products", dto);
  return res.data;
}

export async function updateProduct(
  id: string,
  dto: UpdateProductDto,
): Promise<AdminProductResponseDto> {
  const res = await api.put<AdminProductResponseDto>(`/admin/products/${id}`, dto);
  return res.data;
}

export async function updateProductStock(
  id: string,
  dto: UpdateStockDto,
): Promise<AdminProductResponseDto> {
  const res = await api.patch<AdminProductResponseDto>(
    `/admin/products/${id}/stock`,
    dto,
  );
  return res.data;
}

export async function getProductRatingSummary(
  id: string,
): Promise<ProductRatingSummaryDto> {
  const res = await api.get<ProductRatingSummaryDto>(`/products/${id}/ratings`);
  return res.data;
}

export async function saveProductRating(
  id: string,
  dto: CreateProductRatingDto,
): Promise<ProductRatingSummaryDto> {
  const res = await api.post<ProductRatingSummaryDto>(`/products/${id}/ratings`, dto);
  return res.data;
}

export async function getAdminProductRatings(
  id: string,
  page = 1,
  pageSize = 10,
): Promise<PagedProductRatingResponseDto> {
  const res = await api.get<PagedProductRatingResponseDto>(`/admin/products/${id}/ratings`, {
    params: { page, pageSize },
  });
  return res.data;
}
