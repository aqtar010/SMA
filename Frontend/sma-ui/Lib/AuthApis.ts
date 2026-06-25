import axios from "axios";
import { api } from "./api/api";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api";

export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterRequest = {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
};

export async function login(request: LoginRequest) {
  const response = await api.post(`${API_BASE_URL}/auth/login`, request);
  return response.data as { token: string; refreshToken?: string; role?: string; userId?: string; email?: string };
}

export async function registerUser(request: RegisterRequest) {
  const response = await api.post(`${API_BASE_URL}/auth/register`, request);
  return response.data;
}
