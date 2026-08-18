"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import axios from "axios";
import CartSummary from "@/Components/CartSummary";
import { useCartStore } from "@/Store/cartStore";
import { checkout } from "@/Lib/OrderApis";

export default function CheckoutPage() {
  const router = useRouter();
  const items = useCartStore((s) => s.items);
  const clearCart = useCartStore((s) => s.clearCart);

  const [shippingAddress, setShippingAddress] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const order = await checkout({
        shippingAddress: shippingAddress.trim(),
        items: items.map((item) => ({
          productId: item.id,
          quantity: item.quantity,
        })),
      });

      clearCart();
      router.replace(`/orders/${order.orderId}`);
    } catch (err) {
      const message = axios.isAxiosError(err)
        ? err.response?.data || err.message
        : "Checkout failed. Please try again.";
      setError(String(message));
    } finally {
      setLoading(false);
    }
  }

  if (items.length === 0) {
    return (
      <div className="rounded-2xl border border-slate-200 bg-white p-12 text-center shadow-sm">
        <p className="text-lg text-slate-600">Your cart is empty.</p>
        <Link
          href="/"
          className="mt-4 inline-block rounded-2xl bg-slate-900 px-6 py-3 text-white transition hover:bg-slate-700"
        >
          Browse products
        </Link>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-semibold text-slate-900">Checkout</h1>
        <p className="mt-1 text-slate-600">
          Enter your shipping details to place the order.
        </p>
      </div>

      <div className="grid gap-8 lg:grid-cols-[1fr_320px]">
        <form
          onSubmit={handleSubmit}
          className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm"
        >
          <h2 className="text-lg font-semibold text-slate-900">
            Shipping address
          </h2>

          <div className="mt-4">
            <label
              htmlFor="shippingAddress"
              className="mb-2 block text-sm font-medium text-slate-700"
            >
              Full address
            </label>
            <textarea
              id="shippingAddress"
              value={shippingAddress}
              onChange={(e) => setShippingAddress(e.target.value)}
              required
              rows={4}
              placeholder="Street, city, state, zip code"
              className="w-full rounded-2xl border border-slate-300 bg-slate-50 px-4 py-3 outline-none focus:border-slate-500"
            />
          </div>

          {error && (
            <p className="mt-4 rounded-2xl bg-red-100 px-4 py-3 text-sm text-red-700">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loading}
            className="mt-6 w-full rounded-2xl bg-slate-900 px-4 py-3 text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {loading ? "Placing order…" : "Place order"}
          </button>

          <Link
            href="/cart"
            className="mt-4 inline-block text-sm text-slate-600 hover:text-slate-900"
          >
            ← Back to cart
          </Link>
        </form>

        <div>
          <CartSummary showCheckoutButton={false} />

          <div className="mt-4 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
            <h3 className="text-sm font-medium text-slate-700">Items</h3>
            <ul className="mt-2 space-y-2">
              {items.map((item) => (
                <li
                  key={item.id}
                  className="flex justify-between text-sm text-slate-600"
                >
                  <span>
                    {item.name} × {item.quantity}
                  </span>
                  <span>${(item.price * item.quantity).toFixed(2)}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
