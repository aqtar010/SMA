"use client";

import { useEffect, useState } from "react";
import axios from "axios";
import { AdminProductResponseDto, ProductRatingResponseDto } from "@/DTOs/ProductDTOs";
import { fetchAdminProducts, getAdminProductRatings } from "@/Lib/ProductApis";

const PAGE_SIZE = 10;

export default function AdminRatingsPage() {
  const [products, setProducts] = useState<AdminProductResponseDto[]>([]);
  const [productId, setProductId] = useState("");
  const [ratings, setRatings] = useState<ProductRatingResponseDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchAdminProducts()
      .then((loadedProducts) => {
        setProducts(loadedProducts);
        if (loadedProducts.length) setProductId(loadedProducts[0].id);
      })
      .catch(() => setError("Could not load products."));
  }, []);

  useEffect(() => {
    if (!productId) return;
    const fetchTask = window.setTimeout(() => {
      setLoading(true);
      setError(null);
      getAdminProductRatings(productId, page, PAGE_SIZE)
        .then((result) => {
          setRatings(result.items);
          setTotalPages(result.totalPages);
          setTotalCount(result.totalCount);
        })
        .catch((err) => setError(axios.isAxiosError(err) ? String(err.response?.data ?? err.message) : "Could not load ratings."))
        .finally(() => setLoading(false));
    }, 0);

    return () => window.clearTimeout(fetchTask);
  }, [page, productId]);

  function changeProduct(value: string) {
    setProductId(value);
    setPage(1);
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-semibold text-slate-900">Product ratings</h1>
        <p className="mt-1 text-slate-600">Review customer feedback product by product{totalCount ? ` (${totalCount})` : ""}.</p>
      </div>

      <div className="mb-6 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <label className="text-sm font-medium text-slate-700" htmlFor="rating-product">Product</label>
        <select id="rating-product" value={productId} onChange={(event) => changeProduct(event.target.value)} className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm sm:min-w-72">
          {products.map((product) => <option key={product.id} value={product.id}>{product.name}</option>)}
        </select>
      </div>

      {error && <p className="mb-4 rounded-xl bg-red-100 px-4 py-3 text-sm text-red-700">{error}</p>}
      {loading ? <p className="text-slate-600">Loading ratings...</p> : ratings.length === 0 ? <div className="rounded-xl border border-slate-200 bg-white p-12 text-center shadow-sm"><h2 className="text-xl font-semibold text-slate-900">No ratings yet</h2><p className="mt-2 text-slate-600">Verified customer feedback will appear here.</p></div> : (
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-5 py-4 font-medium">Customer</th><th className="px-5 py-4 font-medium">Rating</th><th className="px-5 py-4 font-medium">Feedback</th><th className="px-5 py-4 font-medium">Submitted</th></tr></thead>
              <tbody className="divide-y divide-slate-100">{ratings.map((rating) => <tr key={rating.id} className="align-top"><td className="px-5 py-4"><p className="font-medium text-slate-900">{rating.customerName}</p><p className="mt-1 text-slate-500">{rating.customerEmail}</p></td><td className="whitespace-nowrap px-5 py-4"><span className="tracking-wide text-amber-500">{"★".repeat(rating.rating)}{"☆".repeat(5 - rating.rating)}</span><span className="ml-2 text-slate-600">{rating.rating}/5</span></td><td className="max-w-md px-5 py-4 text-slate-600">{rating.feedback || "No written feedback"}</td><td className="whitespace-nowrap px-5 py-4 text-slate-500">{new Date(rating.createdAt).toLocaleString()}</td></tr>)}</tbody>
            </table>
          </div>
        </div>
      )}

      {totalPages > 1 && <nav className="mt-5 flex items-center justify-between border-t border-slate-200 pt-5" aria-label="Ratings pagination"><button type="button" onClick={() => setPage((current) => current - 1)} disabled={page === 1 || loading} className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 disabled:opacity-50">Previous</button><span className="text-sm text-slate-600">Page {page} of {totalPages}</span><button type="button" onClick={() => setPage((current) => current + 1)} disabled={page === totalPages || loading} className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 disabled:opacity-50">Next</button></nav>}
    </div>
  );
}
