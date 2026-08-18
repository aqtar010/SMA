"use client";

import { useEffect, useState } from "react";
import ProductCard from "@/Components/ProductCard";
import { fetchProducts } from "@/Lib/ProductApis";
import { ProductResponseDto } from "@/DTOs/ProductDTOs";

export default function Home() {
  const [products, setProducts] = useState<ProductResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      try {
        const data = await fetchProducts();
        setProducts(data);
      } catch (e) {
        console.error("Failed to fetch products", e);
        setError("Could not load products. Please try again.");
      } finally {
        setLoading(false);
      }
    }

    load();
  }, []);

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-semibold text-slate-900">Products</h1>
        <p className="mt-1 text-slate-600">
          Browse our catalog and add items to your cart.
        </p>
      </div>

      {loading && (
        <p className="text-slate-600">Loading products…</p>
      )}

      {error && (
        <p className="rounded-2xl bg-red-100 px-4 py-3 text-sm text-red-700">
          {error}
        </p>
      )}

      {!loading && !error && products.length === 0 && (
        <p className="rounded-2xl border border-slate-200 bg-white p-8 text-center text-slate-600">
          No products available right now.
        </p>
      )}

      {!loading && products.length > 0 && (
        <div className="flex flex-wrap gap-4">
          {products.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  );
}
