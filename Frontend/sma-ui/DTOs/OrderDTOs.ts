export interface OrderItemRequestDto {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequestDto {
  shippingAddress: string;
  items: OrderItemRequestDto[];
}

export interface OrderResponseDto {
  orderId: string;
  totalAmount: number;
  status: string;
  shippingAddress: string;
  createdAt: string;
}
