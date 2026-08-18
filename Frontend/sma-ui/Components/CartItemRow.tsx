"use client";

import { useCartStore } from "@/Store/cartStore";
import { ProductResponseDto } from "@/DTOs/ProductDTOs";

type CartItem = ProductResponseDto & { quantity: number };

export default function CartItemRow({ item }: { item: CartItem }) {
  const updateQuantity = useCartStore((s) => s.updateQuantity);
  const removeItem = useCartStore((s) => s.removeItem);

  const lineTotal = item.price * item.quantity;
  const atMaxStock = item.quantity >= item.quantityAvailable;

  return (
    <div className="flex flex-col gap-4 border-b border-slate-100 py-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0 flex-1">
        <p className="font-medium text-slate-900">{item.name}</p>
        <p className="text-sm text-slate-500">{item.sku}</p>
        <p className="mt-1 text-sm text-slate-600">${item.price.toFixed(2)} each</p>
      </div>

      <div className="flex items-center gap-4">
        <div className="flex items-center rounded-xl border border-slate-200">
          <button
            type="button"
            onClick={() => updateQuantity(item.id, item.quantity - 1)}
            className="px-3 py-2 text-slate-600 hover:bg-slate-50"
            aria-label="Decrease quantity"
          >
            −
          </button>
          <span className="min-w-[2rem] px-2 text-center text-sm font-medium">
            {item.quantity}
          </span>
          <button
            type="button"
            onClick={() => updateQuantity(item.id, item.quantity + 1)}
            disabled={atMaxStock}
            className="px-3 py-2 text-slate-600 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
            aria-label="Increase quantity"
          >
            +
          </button>
        </div>

        <p className="min-w-[5rem] text-right font-semibold text-slate-900">
          ${lineTotal.toFixed(2)}
        </p>

        <button
          type="button"
          onClick={() => removeItem(item.id)}
          className="text-sm text-red-600 hover:text-red-800"
        >
          Remove
        </button>
      </div>
    </div>
  );
}
