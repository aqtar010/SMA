"use client";

import Link from "next/link";
import { useCartStore } from "@/Store/cartStore";

interface CartSummaryProps {
  showCheckoutButton?: boolean;
}

export default function CartSummary({
  showCheckoutButton = true,
}: CartSummaryProps) {
  const items = useCartStore((s) => s.items);

  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);
  const subtotal = items.reduce(
    (sum, item) => sum + item.price * item.quantity,
    0,
  );

  return (
    <div className="rounded-2xl border border-slate-200 bg-slate-50 p-6">
      <h2 className="text-lg font-semibold text-slate-900">Order summary</h2>

      <div className="mt-4 space-y-2 text-sm text-slate-600">
        <div className="flex justify-between">
          <span>Items ({itemCount})</span>
          <span>${subtotal.toFixed(2)}</span>
        </div>
        <div className="flex justify-between">
          <span>Shipping</span>
          <span>Free</span>
        </div>
      </div>

      <div className="mt-4 flex justify-between border-t border-slate-200 pt-4 text-base font-semibold text-slate-900">
        <span>Total</span>
        <span>${subtotal.toFixed(2)}</span>
      </div>

      {showCheckoutButton && (
        <Link
          href={items.length > 0 ? "/checkout" : "#"}
          aria-disabled={items.length === 0}
          className={`mt-6 block w-full rounded-2xl px-4 py-3 text-center text-white transition ${
            items.length > 0
              ? "bg-slate-900 hover:bg-slate-700"
              : "cursor-not-allowed bg-slate-400"
          }`}
        >
          Proceed to checkout
        </Link>
      )}
    </div>
  );
}
