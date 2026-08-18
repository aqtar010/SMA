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
  const res = await api.get<AdminProductResponseDto[]>("/products/admin");
  return res.data;
}

export async function createProduct(
  dto: CreateProductDto,
): Promise<ProductResponseDto> {
  const res = await api.post<ProductResponseDto>("/products", dto);
  return res.data;
}

export async function updateProduct(
  id: string,
  dto: UpdateProductDto,
): Promise<AdminProductResponseDto> {
  const res = await api.put<AdminProductResponseDto>(`/products/${id}`, dto);
  return res.data;
}

export async function updateProductStock(
  id: string,
  dto: UpdateStockDto,
): Promise<AdminProductResponseDto> {
  const res = await api.patch<AdminProductResponseDto>(
    `/products/${id}/stock`,
    dto,
  );
  return res.data;
}
