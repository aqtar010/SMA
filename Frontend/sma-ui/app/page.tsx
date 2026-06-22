// frontend/app/page.tsx
import { fetchProducts } from '@/Lib/api';

export default async function Home() {
  const products = await fetchProducts();

  return (
    <main>
      <h1>Our Store</h1>
      <div className="grid grid-cols-3 gap-4">
        {products.map((p: any) => (
          <div key={p.id} className="border p-4">
            <h2>{p.name}</h2>
            <p>${p.price}</p>
            <p>Stock: {p.quantityAvailable}</p>
          </div>
        ))}
      </div>
    </main>
  );
}