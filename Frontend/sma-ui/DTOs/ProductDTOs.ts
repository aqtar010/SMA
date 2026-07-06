export interface ProductResponseDto {
  id: string;                // Guid becomes string
  sku: string;               
  name: string;              
  description: string | null; // string? becomes nullable
  price: number;             // decimal becomes number
  quantityAvailable: number; // int becomes number
}