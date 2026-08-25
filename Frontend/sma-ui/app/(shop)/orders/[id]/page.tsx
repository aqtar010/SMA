"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { getOrderById } from "@/Lib/OrderApis";
import { OrderResponseDto } from "@/DTOs/OrderDTOs";
import { useCartStore } from "@/Store/cartStore";

export default function OrderDetailPage() {
  const params = useParams();
  const orderId = params.id as string;
  const clearCart = useCartStore((s) => s.clearCart);

  const [order, setOrder] = useState<OrderResponseDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      try {
        const data = await getOrderById(orderId);
        setOrder(data);
        if (data.status === "Paid" || data.status === "Placed") clearCart();
      } catch {
        setError("Order not found or you do not have access.");
      } finally {
        setLoading(false);
      }
    }

    if (orderId) load();
  }, [clearCart, orderId]);

  if (loading) {
    return <p className="text-slate-600">Loading order…</p>;
  }

  if (error || !order) {
    return (
      <div className="rounded-2xl border border-slate-200 bg-white p-12 text-center shadow-sm">
        <p className="text-lg text-red-700">{error ?? "Order not found."}</p>
        <Link
          href="/"
          className="mt-4 inline-block rounded-2xl bg-slate-900 px-6 py-3 text-white transition hover:bg-slate-700"
        >
          Back to shop
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-lg">
      <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
        <div className="mb-6 text-center">
          <div className={`mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full ${order.status === "Paid" || order.status === "Placed" ? "bg-emerald-100" : "bg-amber-100"}`}>
            <span className={`text-2xl ${order.status === "Paid" || order.status === "Placed" ? "text-emerald-600" : "text-amber-600"}`}>
              {order.status === "Paid" || order.status === "Placed" ? "✓" : "…"}
            </span>
          </div>
          <h1 className="text-2xl font-semibold text-slate-900">
            {order.status === "Paid" || order.status === "Placed" ? "Order placed" : "Payment processing"}
          </h1>
          <p className="mt-1 text-slate-600">
            {order.status === "Paid" || order.status === "Placed"
              ? "Thank you for your purchase."
              : "Your order will update after Stripe confirms payment."}
          </p>
        </div>

        <dl className="space-y-4 text-sm">
          <div className="flex justify-between border-b border-slate-100 pb-3">
            <dt className="text-slate-600">Order ID</dt>
            <dd className="font-mono text-slate-900">{order.orderId}</dd>
          </div>
          <div className="flex justify-between border-b border-slate-100 pb-3">
            <dt className="text-slate-600">Status</dt>
            <dd className={`rounded-full px-2 py-0.5 font-medium ${order.status === "Paid" || order.status === "Placed" ? "bg-emerald-100 text-emerald-800" : "bg-amber-100 text-amber-800"}`}>
              {order.status}
            </dd>
          </div>
          <div className="flex justify-between border-b border-slate-100 pb-3">
            <dt className="text-slate-600">Total</dt>
            <dd className="font-semibold text-slate-900">
              ${order.totalAmount.toFixed(2)}
            </dd>
          </div>
          <div className="flex justify-between border-b border-slate-100 pb-3">
            <dt className="text-slate-600">Shipping to</dt>
            <dd className="max-w-[60%] text-right text-slate-900">
              {order.shippingAddress}
            </dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-slate-600">Placed on</dt>
            <dd className="text-slate-900">
              {new Date(order.createdAt).toLocaleString()}
            </dd>
          </div>
        </dl>

        <Link
          href="/"
          className="mt-8 block w-full rounded-2xl bg-slate-900 px-4 py-3 text-center text-white transition hover:bg-slate-700"
        >
          Continue shopping
        </Link>
      </div>
    </div>
  );
}
