import { create } from "zustand";

export type UserRole = "Customer" | "Admin" | "SuperAdmin";

interface AuthState {
  accessToken: string | null;
  role: UserRole | null;
  email: string | null;
  authenticated: boolean;
  initialized: boolean;

  initialize: () => void;
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

  initialize: () => {
    const token = localStorage.getItem("jwtToken");
    const role = parseRole(localStorage.getItem("userRole"));
    const email = localStorage.getItem("userEmail");

    set({
      accessToken: token,
      role,
      email,
      authenticated: !!token,
      initialized: true,
    });
  },

  getAccessToken: () => get().accessToken,

  setAuth: (token, role, email) => {
    localStorage.setItem("jwtToken", token);
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
    if (token) localStorage.setItem("jwtToken", token);
    else localStorage.removeItem("jwtToken");

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
    localStorage.removeItem("jwtToken");
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
