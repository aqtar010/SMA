import { api } from "./api/api";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api";

export async function fetchProducts() {
    const route= `${API_BASE_URL}/Products`
    console.log(route);
  const res = await api.get(route);
  // axios responses have `status` and `data` properties
  if (res.status < 200 || res.status >= 300) throw new Error("Failed to fetch products");
  return res.data;
}