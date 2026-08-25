"use client";

import { useEffect, useState } from "react";
import axios from "axios";
import { getAdminOrders } from "@/Lib/OrderApis";
import { AdminOrderResponseDto } from "@/DTOs/OrderDTOs";

const PAGE_SIZE = 10;

function getStatusClassName(status: string) {
    const normalizedStatus = status.toLowerCase();
    if (normalizedStatus === "paid" || normalizedStatus === "placed") {
        return "bg-emerald-100 text-emerald-800";
    }
    if (normalizedStatus === "failed" || normalizedStatus === "expired") {
        return "bg-red-100 text-red-800";
    }
    return "bg-amber-100 text-amber-800";
}

export default function AdminOrdersPage() {
    const [orders, setOrders] = useState<AdminOrderResponseDto[]>([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;

        getAdminOrders(page, PAGE_SIZE)
            .then((result) => {
                if (!active) return;
                setOrders(result.items);
                setTotalPages(result.totalPages);
                setTotalCount(result.totalCount);
            })
            .catch((err) => {
                if (!active) return;
                const message = axios.isAxiosError(err)
                    ? err.response?.data || err.message
                    : "Failed to load orders.";
                setError(String(message));
            })
            .finally(() => {
                if (active) setLoading(false);
            });

        return () => {
            active = false;
        };
    }, [page]);

    function goToPage(nextPage: number) {
        setLoading(true);
        setError(null);
        setPage(nextPage);
    }

    if (loading) return <p className="text-slate-600">Loading orders...</p>;

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-3xl font-semibold text-slate-900">All orders</h1>
                <p className="mt-1 text-slate-600">
                    Review orders from every customer{totalCount > 0 ? ` (${totalCount})` : ""}.
                </p>
            </div>

            {error && (
                <p className="mb-4 rounded-2xl bg-red-100 px-4 py-3 text-sm text-red-700">
                    {error}
                </p>
            )}

            {orders.length === 0 && !error ? (
                <div className="rounded-2xl border border-slate-200 bg-white p-12 text-center shadow-sm">
                    <h2 className="text-xl font-semibold text-slate-900">No orders yet</h2>
                    <p className="mt-2 text-slate-600">Customer orders will appear here.</p>
                </div>
            ) : (
                <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-slate-200 text-left text-sm">
                            <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
                                <tr>
                                    <th className="px-5 py-4 font-medium">Order</th>
                                    <th className="px-5 py-4 font-medium">Customer</th>
                                    <th className="px-5 py-4 font-medium">Placed</th>
                                    <th className="px-5 py-4 font-medium">Status</th>
                                    <th className="px-5 py-4 font-medium">Shipping address</th>
                                    <th className="px-5 py-4 text-right font-medium">Total</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-100">
                                {orders.map((order) => (
                                    <tr key={order.orderId} className="align-top">
                                        <td className="whitespace-nowrap px-5 py-4 font-mono text-xs text-slate-600">
                                            {order.orderId}
                                        </td>
                                        <td className="px-5 py-4">
                                            <p className="font-medium text-slate-900">{order.customerName}</p>
                                            <p className="mt-1 text-slate-500">{order.customerEmail}</p>
                                        </td>
                                        <td className="whitespace-nowrap px-5 py-4 text-slate-600">
                                            {new Date(order.createdAt).toLocaleString()}
                                        </td>
                                        <td className="px-5 py-4">
                                            <span className={`whitespace-nowrap rounded-full px-3 py-1 text-xs font-medium ${getStatusClassName(order.status)}`}>
                                                {order.status}
                                            </span>
                                        </td>
                                        <td className="max-w-xs px-5 py-4 text-slate-600">{order.shippingAddress}</td>
                                        <td className="whitespace-nowrap px-5 py-4 text-right font-semibold text-slate-900">
                                            ${order.totalAmount.toFixed(2)}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}

            {totalPages > 1 && (
                <nav className="flex items-center justify-between border-t border-slate-200 pt-5" aria-label="All orders pagination">
                    <button
                        type="button"
                        onClick={() => goToPage(page - 1)}
                        disabled={page === 1 || loading}
                        className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                        Previous
                    </button>
                    <span className="text-sm text-slate-600">Page {page} of {totalPages}</span>
                    <button
                        type="button"
                        onClick={() => goToPage(page + 1)}
                        disabled={page === totalPages || loading}
                        className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                        Next
                    </button>
                </nav>
            )}
        </div>
    );
}