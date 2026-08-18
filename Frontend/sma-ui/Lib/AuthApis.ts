import { api } from "./api/api";

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

export type LoginResponse = {
  token: string;
  refreshToken?: string;
  role?: string;
  userId?: string;
  email?: string;
};

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await api.post<LoginResponse>("/auth/login", request);
  return response.data;
}

export async function registerUser(request: RegisterRequest) {
  const response = await api.post("/auth/register", request);
  return response.data;
}
