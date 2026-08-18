"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCartStore } from "@/Store/cartStore";
import { useAuthStore } from "@/Store/authStore";

export default function Navbar() {
  const router = useRouter();
  const items = useCartStore((s) => s.items);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const isAdmin = useAuthStore((s) => s.isAdmin);

  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);
  const subtotal = items.reduce(
    (sum, item) => sum + item.price * item.quantity,
    0,
  );

  function handleLogout() {
    clearAuth();
    router.replace("/login");
  }

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
        <Link href="/" className="text-lg font-semibold text-slate-900">
          SMA Shop
        </Link>

        <div className="flex items-center gap-3 sm:gap-4">
          {isAdmin() && (
            <Link
              href="/admin/products"
              className="text-sm font-medium text-slate-700 hover:text-slate-900"
            >
              Admin
            </Link>
          )}

          <Link
            href="/cart"
            className="rounded-full bg-slate-900 px-3 py-1.5 text-sm text-white transition hover:bg-slate-700"
          >
            Cart ({itemCount}) • ${subtotal.toFixed(2)}
          </Link>

          <button
            type="button"
            onClick={handleLogout}
            className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm text-slate-700 transition hover:bg-slate-100"
          >
            Log out
          </button>
        </div>
      </div>
    </header>
  );
}
