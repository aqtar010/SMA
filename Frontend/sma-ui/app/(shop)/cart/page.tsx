"use client";

import Link from "next/link";
import CartItemRow from "@/Components/CartItemRow";
import CartSummary from "@/Components/CartSummary";
import { useCartStore } from "@/Store/cartStore";

export default function CartPage() {
  const items = useCartStore((s) => s.items);

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-semibold text-slate-900">Your cart</h1>
        <p className="mt-1 text-slate-600">
          Review items before checkout.
        </p>
      </div>

      {items.length === 0 ? (
        <div className="rounded-2xl border border-slate-200 bg-white p-12 text-center shadow-sm">
          <p className="text-lg text-slate-600">Your cart is empty.</p>
          <Link
            href="/"
            className="mt-4 inline-block rounded-2xl bg-slate-900 px-6 py-3 text-white transition hover:bg-slate-700"
          >
            Continue shopping
          </Link>
        </div>
      ) : (
        <div className="grid gap-8 lg:grid-cols-[1fr_320px]">
          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            {items.map((item) => (
              <CartItemRow key={item.id} item={item} />
            ))}

            <Link
              href="/"
              className="mt-4 inline-block text-sm text-slate-600 hover:text-slate-900"
            >
              ← Continue shopping
            </Link>
          </div>

          <CartSummary />
        </div>
      )}
    </div>
  );
}
