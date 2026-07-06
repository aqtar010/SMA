"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/Store/authStore";
import ProductCard from "@/Components/ProductCard";
import {fetchProducts} from "@/Lib/ProductApis";
import { ProductResponseDto } from "@/DTOs/ProductDTOs";



export default function Home() {
  const router = useRouter();

  const authenticated = useAuthStore((s) => s.authenticated);
  const setAccessToken = useAuthStore((s) => s.setAccessToken);
  const clearAuth = useAuthStore((s) => s.clearAuth);

  const [products,setProducts]=useState<ProductResponseDto[]>([]);



  useEffect(() => {
    const token = localStorage.getItem("jwtToken");

    if (!token) {
      clearAuth();
      router.replace("/login");
      return;
    }

    setAccessToken(token);
  }, [router, clearAuth, setAccessToken]);
  useEffect(() => {
    if (!authenticated) return;

    async function load() {
      try {
        const data = await fetchProducts();
        if (Array.isArray(data)) setProducts(data as ProductResponseDto[]);
      } catch (e) {
        console.error("Failed to fetch products", e);
      }
    }

    load();
  }, [authenticated]);

  function logOutAction() {
    clearAuth(); // clearAuth should remove localStorage internally
    router.replace("/login");
  }

  if (!authenticated) {
    return (
      <main className="min-h-screen flex items-center justify-center bg-slate-50 text-slate-900">
        <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
          <p className="text-lg font-semibold">Checking authentication…</p>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-slate-50 text-slate-900">
      <div className="mx-auto max-w-3xl p-6">
        <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
          <h1 className="text-3xl font-semibold mb-3">Welcome back</h1>

          <p className="text-slate-600 mb-6">
            You are logged in with a saved token.
          </p>

          <button
            className="rounded-lg bg-slate-900 px-4 py-2 text-white hover:bg-slate-700"
            onClick={logOutAction}
          >
            Log out
          </button>
          <div className="products-grid" style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
        {products.map((product) => (
          <ProductCard 
            key={product.id} // React requires a unique key for list items
            name={product.name}
            price={product.price}
            quantityAvailable={product.quantityAvailable}
            sku={product.sku}
            description={product.description}
            id={product.id}
          />
        ))}
      </div>
        </div>
      </div>
    </main>
  );
}