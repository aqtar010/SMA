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

export interface AdminOrderResponseDto extends OrderResponseDto {
  userId: string;
  customerEmail: string;
  customerName: string;
}

export interface PagedAdminOrderResponseDto {
  items: AdminOrderResponseDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CheckoutResponseDto extends OrderResponseDto {
  checkoutUrl: string;
}

export interface AdminAnalyticsDto {
  grossSales: number;
  paidOrderCount: number;
  orderCount: number;
  averageOrderValue: number;
  inventoryValue: number;
  activeProductCount: number;
  lowStockProductCount: number;
  orderStatusCounts: Record<string, number>;
  dailySales: { date: string; amount: number }[];
}
