import { create } from "zustand";

export type UserRole = "Customer" | "Admin" | "SuperAdmin";

interface AuthState {
  accessToken: string | null;
  role: UserRole | null;
  email: string | null;
  authenticated: boolean;
  initialized: boolean;

  initialize: () => Promise<void>;
  setAuth: (token: string, role: UserRole, email?: string) => void;
  setAccessToken: (token: string | null) => void;
  getAccessToken: () => string | null;
  isAdmin: () => boolean;
  clearAuth: () => void;
}

function parseRole(value: string | null): UserRole | null {
  if (value === "Customer" || value === "Admin" || value === "SuperAdmin") {
    return value;
  }
  return null;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  accessToken: null,
  role: null,
  email: null,
  authenticated: false,
  initialized: false,

  initialize: async () => {
    localStorage.removeItem("jwtToken");
    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api"}/auth/refresh`,
        { method: "POST", credentials: "include", headers: { "Content-Type": "application/json" }, body: "{}" },
      );
      if (!response.ok) throw new Error("No active session");
      const result = await response.json() as { token: string; role?: string; email?: string };
      const role = parseRole(result.role ?? null);
      localStorage.setItem("userRole", role ?? "");
      if (result.email) localStorage.setItem("userEmail", result.email);
      set({ accessToken: result.token, role, email: result.email ?? null, authenticated: true, initialized: true });
    } catch {
      set({ accessToken: null, role: null, email: null, authenticated: false, initialized: true });
    }
  },

  getAccessToken: () => get().accessToken,

  setAuth: (token, role, email) => {
    localStorage.setItem("userRole", role);
    if (email) localStorage.setItem("userEmail", email);
    else localStorage.removeItem("userEmail");

    set({
      accessToken: token,
      role,
      email: email ?? null,
      authenticated: true,
    });
  },

  setAccessToken: (token) => {
    set({
      accessToken: token,
      authenticated: !!token,
    });
  },

  isAdmin: () => {
    const role = get().role;
    return role === "Admin" || role === "SuperAdmin";
  },

  clearAuth: () => {
    void fetch(
      `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api"}/auth/revoke`,
      { method: "POST", credentials: "include", headers: { "Content-Type": "application/json" }, body: "{}" },
    ).catch(() => undefined);
    localStorage.removeItem("userRole");
    localStorage.removeItem("userEmail");

    set({
      accessToken: null,
      role: null,
      email: null,
      authenticated: false,
    });
  },
}));
