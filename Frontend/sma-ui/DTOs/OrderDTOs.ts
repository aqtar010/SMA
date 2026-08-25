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

export interface PagedOrderResponseDto {
  items: OrderResponseDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CheckoutResponseDto extends OrderResponseDto {
  checkoutUrl: string;
}
