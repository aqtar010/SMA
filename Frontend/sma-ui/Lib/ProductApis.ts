import { api } from "./api/api";
import {
  AdminProductResponseDto,
  CreateProductDto,
  ProductResponseDto,
  UpdateProductDto,
  UpdateStockDto,
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
