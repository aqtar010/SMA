import { api } from "./api/api";
import {
  CreateOrderRequestDto,
  CheckoutResponseDto,
  PagedOrderResponseDto,
  PagedAdminOrderResponseDto,
  OrderResponseDto,
  AdminAnalyticsDto,
} from "@/DTOs/OrderDTOs";

export async function checkout(
  request: CreateOrderRequestDto,
  idempotencyKey: string,
): Promise<CheckoutResponseDto> {
  const response = await api.post<CheckoutResponseDto>("/orders/checkout", request, {
    headers: { "Idempotency-Key": idempotencyKey },
  });
  return response.data;
}

export async function getOrderById(id: string): Promise<OrderResponseDto> {
  const response = await api.get<OrderResponseDto>(`/orders/${id}`);
  return response.data;
}

export async function getOrders(
  page = 1,
  pageSize = 10,
): Promise<PagedOrderResponseDto> {
  const response = await api.get<PagedOrderResponseDto>("/orders", {
    params: { page, pageSize },
  });
  return response.data;
}

export async function getAdminOrders(
  page = 1,
  pageSize = 10,
): Promise<PagedAdminOrderResponseDto> {
  const response = await api.get<PagedAdminOrderResponseDto>("/admin/orders", {
    params: { page, pageSize },
  });
  return response.data;
}

export async function getAdminAnalytics(days = 7): Promise<AdminAnalyticsDto> {
  const response = await api.get<AdminAnalyticsDto>("/admin/analytics", {
    params: { days },
  });
  return response.data;
}
