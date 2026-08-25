import { api } from "./api/api";
import {
  CreateOrderRequestDto,
  CheckoutResponseDto,
  OrderResponseDto,
} from "@/DTOs/OrderDTOs";

export async function checkout(
  request: CreateOrderRequestDto,
): Promise<CheckoutResponseDto> {
  const response = await api.post<CheckoutResponseDto>("/orders/checkout", request);
  return response.data;
}

export async function getOrderById(id: string): Promise<OrderResponseDto> {
  const response = await api.get<OrderResponseDto>(`/orders/${id}`);
  return response.data;
}
