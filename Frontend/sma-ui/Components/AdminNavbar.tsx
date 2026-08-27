"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/Store/authStore";

export default function AdminNavbar() {
  const router = useRouter();
  const email = useAuthStore((s) => s.email);
  const role = useAuthStore((s) => s.role);
  const clearAuth = useAuthStore((s) => s.clearAuth);

  function handleLogout() {
    clearAuth();
    router.replace("/login");
  }

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3">
        <div className="flex items-center gap-6">
          <Link href="/admin" className="text-lg font-semibold text-slate-900">
            SMA Admin
          </Link>
          <nav className="hidden gap-4 text-sm sm:flex">
            <Link href="/admin" className="font-medium text-slate-900">
              Dashboard
            </Link>
            <Link
              href="/admin/products"
              className="font-medium text-slate-900"
            >
              Products
            </Link>
            <Link
              href="/admin/orders"
              className="font-medium text-slate-900"
            >
              Orders
            </Link>
            <Link
              href="/admin/ratings"
              className="font-medium text-slate-900"
            >
              Ratings
            </Link>
            <Link
              href="/"
              className="text-slate-600 hover:text-slate-900"
            >
              View shop
            </Link>
          </nav>
        </div>

        <div className="flex items-center gap-3">
          <span className="hidden text-sm text-slate-600 sm:inline">
            {email ?? "Admin"} · {role}
          </span>
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
