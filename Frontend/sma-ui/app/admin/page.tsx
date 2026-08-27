"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import axios from "axios";
import { AdminAnalyticsDto, AdminOrderResponseDto } from "@/DTOs/OrderDTOs";
import { getAdminAnalytics, getAdminOrders } from "@/Lib/OrderApis";

function formatCurrency(value: number) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
  });
}

function statusClassName(status: string) {
  const normalized = status.toLowerCase();
  if (normalized === "paid" || normalized === "placed") {
    return "bg-emerald-100 text-emerald-800";
  }
  if (normalized === "failed" || normalized === "expired") {
    return "bg-red-100 text-red-800";
  }
  return "bg-amber-100 text-amber-800";
}

export default function AdminDashboardPage() {
  const [orders, setOrders] = useState<AdminOrderResponseDto[]>([]);
  const [analytics, setAnalytics] = useState<AdminAnalyticsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function loadDashboard() {
    setLoading(true);
    setError(null);
    try {
      const [loadedOrders, loadedAnalytics] = await Promise.all([
        getAdminOrders(1, 5),
        getAdminAnalytics(7),
      ]);
      setOrders(loadedOrders.items);
      setAnalytics(loadedAnalytics);
    } catch (err) {
      const message = axios.isAxiosError(err)
        ? err.response?.data || err.message
        : "Failed to load dashboard analytics.";
      setError(String(message));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    const fetchTask = window.setTimeout(() => {
      loadDashboard();
    }, 0);

    return () => window.clearTimeout(fetchTask);
  }, []);

  if (loading) {
    return <p className="text-slate-600">Loading dashboard...</p>;
  }

  if (!analytics) return <p className="text-slate-600">No analytics available.</p>;

  const statusCounts = analytics.orderStatusCounts;
  const maxStatusCount = Math.max(...Object.values(statusCounts), 1);
  const recentOrders = [...orders]
    .sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt))
    .slice(0, 5);
  const lastSevenDays = analytics.dailySales.map((day) => ({
    label: new Date(day.date).toLocaleDateString(undefined, { weekday: "short" }),
    amount: day.amount,
  }));
  const maxDailyRevenue = Math.max(...lastSevenDays.map((day) => day.amount), 1);

  return (
    <div className="space-y-8">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-teal-700">
            Operations overview
          </p>
          <h1 className="mt-2 text-4xl font-semibold tracking-tight text-slate-950">
            Good morning, admin.
          </h1>
          <p className="mt-2 text-slate-600">A live pulse on sales, orders, and inventory.</p>
        </div>
        <button
          type="button"
          onClick={loadDashboard}
          className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:border-teal-600 hover:text-teal-700"
        >
          Refresh data
        </button>
      </div>

      {error && (
        <div className="flex flex-col gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 sm:flex-row sm:items-center sm:justify-between">
          <span>{error}</span>
          <button type="button" onClick={loadDashboard} className="font-semibold underline">Try again</button>
        </div>
      )}

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Key metrics">
        <Metric label="Gross sales" value={formatCurrency(analytics.grossSales)} detail={`${analytics.paidOrderCount} paid orders`} accent="teal" />
        <Metric label="All orders" value={analytics.orderCount.toLocaleString()} detail={`${Object.keys(statusCounts).length} statuses`} accent="blue" />
        <Metric label="Average order" value={formatCurrency(analytics.averageOrderValue)} detail="Across paid orders" accent="amber" />
        <Metric label="Inventory value" value={formatCurrency(analytics.inventoryValue)} detail={`${analytics.activeProductCount} active products`} accent="rose" />
      </section>

      <section className="grid gap-6 lg:grid-cols-[1.35fr_0.65fr]">
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex items-start justify-between">
            <div>
              <h2 className="text-lg font-semibold text-slate-950">Sales activity</h2>
              <p className="mt-1 text-sm text-slate-500">Paid revenue over the last seven days</p>
            </div>
            <span className="rounded-full bg-teal-50 px-3 py-1 text-xs font-semibold text-teal-700">USD</span>
          </div>
          <div className="mt-8 flex h-48 items-end gap-3 sm:gap-5">
            {lastSevenDays.map((day) => (
              <div key={day.label} className="flex min-w-0 flex-1 flex-col items-center gap-2">
                <span className="text-xs font-medium text-slate-500">{formatCurrency(day.amount)}</span>
                <div className="flex h-32 w-full items-end rounded-md bg-slate-100">
                  <div className="w-full rounded-md bg-teal-600 transition-all" style={{ height: `${Math.max((day.amount / maxDailyRevenue) * 100, day.amount ? 8 : 3)}%` }} />
                </div>
                <span className="text-xs text-slate-500">{day.label}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-semibold text-slate-950">Order status</h2>
          <p className="mt-1 text-sm text-slate-500">Current distribution</p>
          <div className="mt-7 space-y-5">
            {Object.entries(statusCounts).length === 0 ? <p className="text-sm text-slate-500">No orders yet.</p> : Object.entries(statusCounts).map(([status, count]) => (
              <div key={status}>
                <div className="mb-2 flex justify-between text-sm"><span className="capitalize text-slate-600">{status}</span><span className="font-semibold text-slate-900">{count}</span></div>
                <div className="h-2 rounded-full bg-slate-100"><div className="h-2 rounded-full bg-slate-800" style={{ width: `${(count / maxStatusCount) * 100}%` }} /></div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="grid gap-6 lg:grid-cols-[1fr_1.35fr]">
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-6">
          <div className="flex items-start justify-between gap-4">
            <div><h2 className="text-lg font-semibold text-slate-950">Inventory attention</h2><p className="mt-1 text-sm text-slate-600">{analytics.lowStockProductCount} active products need a look.</p></div>
            <Link href="/admin/products" className="text-sm font-semibold text-teal-800 hover:underline">Manage</Link>
          </div>
          <div className="mt-5 space-y-3">
            {analytics.lowStockProductCount === 0 ? <p className="text-sm text-slate-600">Everything is comfortably stocked.</p> : <p className="text-sm text-slate-600">Open product management to review low-stock items.</p>}
          </div>
        </div>

        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
          <div className="flex items-center justify-between border-b border-slate-100 px-6 py-4"><div><h2 className="text-lg font-semibold text-slate-950">Recent orders</h2><p className="mt-1 text-sm text-slate-500">The latest customer activity</p></div><Link href="/admin/orders" className="text-sm font-semibold text-teal-700 hover:underline">View all</Link></div>
          {recentOrders.length === 0 ? <p className="p-6 text-sm text-slate-500">No orders yet.</p> : <div className="overflow-x-auto"><table className="min-w-full text-left text-sm"><tbody className="divide-y divide-slate-100">{recentOrders.map((order) => <tr key={order.orderId}><td className="whitespace-nowrap px-6 py-4"><p className="font-medium text-slate-900">{order.customerName}</p><p className="mt-1 font-mono text-xs text-slate-400">#{order.orderId.slice(0, 8)}</p></td><td className="whitespace-nowrap px-6 py-4 text-slate-500">{formatDate(order.createdAt)}</td><td className="whitespace-nowrap px-6 py-4"><span className={`rounded-full px-2.5 py-1 text-xs font-medium ${statusClassName(order.status)}`}>{order.status}</span></td><td className="whitespace-nowrap px-6 py-4 text-right font-semibold text-slate-900">{formatCurrency(order.totalAmount)}</td></tr>)}</tbody></table></div>}
        </div>
      </section>
    </div>
  );
}

function Metric({ label, value, detail, accent }: { label: string; value: string; detail: string; accent: "teal" | "blue" | "amber" | "rose" }) {
  const accents = { teal: "bg-teal-500", blue: "bg-blue-500", amber: "bg-amber-500", rose: "bg-rose-500" };
  return <div className="relative overflow-hidden rounded-xl border border-slate-200 bg-white p-5 shadow-sm"><div className={`absolute inset-y-0 left-0 w-1 ${accents[accent]}`} /><p className="text-sm text-slate-500">{label}</p><p className="mt-2 text-3xl font-semibold tracking-tight text-slate-950">{value}</p><p className="mt-2 text-xs font-medium text-slate-500">{detail}</p></div>;
}
