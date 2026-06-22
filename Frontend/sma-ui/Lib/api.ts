// frontend/lib/api.ts
const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api";

export async function fetchProducts() {
    const route= `${API_BASE_URL}/Products`
    console.log(route);
  const res = await fetch(route, { cache: "no-store" });
  if (!res.ok) throw new Error("Failed to fetch products");
  return res.json();
}
