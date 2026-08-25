"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { getOrders } from "@/Lib/OrderApis";
import { OrderResponseDto } from "@/DTOs/OrderDTOs";

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

export default function OrdersPage() {
    const [orders, setOrders] = useState<OrderResponseDto[]>([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function loadOrders() {
            setLoading(true);
            setError(null);
            try {
                const result = await getOrders(page, PAGE_SIZE);
                setOrders(result.items);
                setTotalPages(result.totalPages);
                setTotalCount(result.totalCount);
            } catch {
                setError("We could not load your orders. Please try again.");
            } finally {
                setLoading(false);
            }
        }

        loadOrders();
    }, [page]);

    if (loading) {
        return <p className="text-slate-600">Loading your orders...</p>;
    }

    if (error) {
        return (
            <div className="rounded-2xl border border-red-200 bg-red-50 p-8 text-center">
                <p className="text-red-700">{error}</p>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-3xl font-semibold text-slate-900">Your orders</h1>
                <p className="mt-1 text-slate-600">
                    Review your previous purchases{totalCount > 0 ? ` (${totalCount})` : ""}.
                </p>
            </div>

            {orders.length === 0 ? (
                <div className="rounded-2xl border border-slate-200 bg-white p-12 text-center shadow-sm">
                    <h2 className="text-xl font-semibold text-slate-900">No orders yet</h2>
                    <p className="mt-2 text-slate-600">Your completed purchases will appear here.</p>
                    <Link
                        href="/"
                        className="mt-6 inline-block rounded-2xl bg-slate-900 px-6 py-3 text-white transition hover:bg-slate-700"
                    >
                        Start shopping
                    </Link>
                </div>
            ) : (
                <div className="space-y-4">
                    {orders.map((order) => (
                        <Link
                            key={order.orderId}
                            href={`/orders/${order.orderId}`}
                            className="block rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition hover:border-slate-400 hover:shadow-md"
                        >
                            <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                                <div>
                                    <p className="font-mono text-sm text-slate-500">{order.orderId}</p>
                                    <p className="mt-1 text-sm text-slate-600">
                                        {new Date(order.createdAt).toLocaleString()}
                                    </p>
                                </div>
                                <span className={`w-fit rounded-full px-3 py-1 text-sm font-medium ${getStatusClassName(order.status)}`}>
                                    {order.status}
                                </span>
                            </div>

                            <div className="mt-5 grid gap-4 border-t border-slate-100 pt-4 sm:grid-cols-2">
                                <div>
                                    <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Shipping to</p>
                                    <p className="mt-1 text-slate-900">{order.shippingAddress}</p>
                                </div>
                                <div className="sm:text-right">
                                    <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Total</p>
                                    <p className="mt-1 text-lg font-semibold text-slate-900">${order.totalAmount.toFixed(2)}</p>
                                </div>
                            </div>
                        </Link>
                    ))}
                </div>
            )}

            {totalPages > 1 && (
                <nav className="flex items-center justify-between border-t border-slate-200 pt-5" aria-label="Orders pagination">
                    <button
                        type="button"
                        onClick={() => setPage((currentPage) => currentPage - 1)}
                        disabled={page === 1 || loading}
                        className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                        Previous
                    </button>
                    <span className="text-sm text-slate-600">
                        Page {page} of {totalPages}
                    </span>
                    <button
                        type="button"
                        onClick={() => setPage((currentPage) => currentPage + 1)}
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