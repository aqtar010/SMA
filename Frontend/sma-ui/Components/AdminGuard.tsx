"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/Store/authStore";

export default function AdminGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const initialized = useAuthStore((s) => s.initialized);
  const authenticated = useAuthStore((s) => s.authenticated);
  const role = useAuthStore((s) => s.role);
  const isAdminUser = role === "Admin" || role === "SuperAdmin";
  const initialize = useAuthStore((s) => s.initialize);

  useEffect(() => {
    initialize();
  }, [initialize]);

  useEffect(() => {
    if (!initialized) return;

    if (!authenticated) {
      router.replace("/login");
      return;
    }

    if (!isAdminUser) {
      router.replace("/");
    }
  }, [initialized, authenticated, isAdminUser, router]);

  if (!initialized || !authenticated || !isAdminUser) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50 text-slate-900">
        <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
          <p className="text-lg font-semibold">Checking admin access…</p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
