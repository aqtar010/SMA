import { create } from "zustand";
interface AuthState {
  accessToken: string | null;
  authenticated: boolean;
  initialized: boolean;

  initialize: () => void;
  setAccessToken: (token: string | null) => void;
  getAccessToken: () => string | null;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>((set,get) => ({
  accessToken: null,
  authenticated: false,
  initialized: false,

  initialize: () => {
    const token = localStorage.getItem("jwtToken");

    set({
      accessToken: token,
      authenticated: !!token,
      initialized: true,
    });
  },
  getAccessToken: () => get().accessToken,
  setAccessToken: (token) => {
    if (token) localStorage.setItem("jwtToken", token);
    else localStorage.removeItem("jwtToken");

    set({
      accessToken: token,
      authenticated: !!token,
    });
  },

  clearAuth: () => {
    localStorage.removeItem("jwtToken");

    set({
      accessToken: null,
      authenticated: false,
    });
  },
}));
