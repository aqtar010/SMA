import { api } from "./api/api";
import {
  CreateOrderRequestDto,
  OrderResponseDto,
} from "@/DTOs/OrderDTOs";

export async function checkout(
  request: CreateOrderRequestDto,
): Promise<OrderResponseDto> {
  const response = await api.post<OrderResponseDto>("/orders/checkout", request);
  return response.data;
}

export async function getOrderById(id: string): Promise<OrderResponseDto> {
  const response = await api.get<OrderResponseDto>(`/orders/${id}`);
  return response.data;
}
