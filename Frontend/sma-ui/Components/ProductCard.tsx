"use client";

import { useState } from "react";
import { useCartStore } from "@/Store/cartStore";
import { ProductResponseDto } from "@/DTOs/ProductDTOs";

interface ProductCardProps {
  product: ProductResponseDto;
}

export default function ProductCard({ product }: ProductCardProps) {
  const addItem = useCartStore((s) => s.addItem);
  const cartQuantity = useCartStore(
    (s) => s.items.find((item) => item.id === product.id)?.quantity ?? 0,
  );

  const [added, setAdded] = useState(false);
  const inStock = product.quantityAvailable > 0;
  const atMaxInCart = cartQuantity >= product.quantityAvailable;

  function handleAddToCart() {
    if (!inStock || atMaxInCart) return;

    addItem(product);
    setAdded(true);
    setTimeout(() => setAdded(false), 1500);
  }

  return (
    <div
      className={`flex w-full flex-col justify-between rounded-2xl border p-5 shadow-sm transition sm:w-[calc(50%-0.5rem)] lg:w-[calc(33.333%-0.75rem)] ${
        inStock
          ? "border-slate-200 bg-white hover:border-slate-300"
          : "border-slate-100 bg-slate-50 opacity-75"
      }`}
    >
      <div>
        <div className="mb-3 flex items-start justify-between gap-2">
          <h3 className="font-semibold text-slate-900">{product.name}</h3>
          <span
            className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${
              inStock
                ? "bg-emerald-100 text-emerald-800"
                : "bg-slate-200 text-slate-600"
            }`}
          >
            {inStock ? `${product.quantityAvailable} in stock` : "Out of stock"}
          </span>
        </div>

        <p className="text-sm text-slate-500">{product.sku}</p>

        {product.description && (
          <p className="mt-2 line-clamp-2 text-sm text-slate-600">
            {product.description}
          </p>
        )}

        <p className="mt-3 text-xl font-semibold text-slate-900">
          ${product.price.toFixed(2)}
        </p>
      </div>

      <div className="mt-4">
        {cartQuantity > 0 && (
          <p className="mb-2 text-sm text-slate-600">
            {cartQuantity} in cart
          </p>
        )}

        <button
          type="button"
          onClick={handleAddToCart}
          disabled={!inStock || atMaxInCart}
          className={`w-full rounded-xl px-4 py-2.5 text-sm font-medium transition ${
            added
              ? "bg-emerald-600 text-white"
              : inStock && !atMaxInCart
                ? "bg-slate-900 text-white hover:bg-slate-700"
                : "cursor-not-allowed bg-slate-200 text-slate-500"
          }`}
        >
          {added
            ? "Added!"
            : atMaxInCart
              ? "Max quantity reached"
              : inStock
                ? "Add to cart"
                : "Out of stock"}
        </button>
      </div>
    </div>
  );
}
