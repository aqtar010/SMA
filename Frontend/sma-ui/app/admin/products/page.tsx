"use client";

import { useEffect, useState } from "react";
import axios from "axios";
import {
  createProduct,
  fetchAdminProducts,
  updateProduct,
  updateProductStock,
} from "@/Lib/ProductApis";
import {
  AdminProductResponseDto,
  CreateProductDto,
} from "@/DTOs/ProductDTOs";

type EditForm = {
  sku: string;
  name: string;
  description: string;
  price: string;
  isActive: boolean;
};

const emptyCreateForm: CreateProductDto = {
  sku: "",
  name: "",
  description: "",
  price: 0,
  initialStock: 0,
};

export default function AdminProductsPage() {
  const [products, setProducts] = useState<AdminProductResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [createForm, setCreateForm] = useState<CreateProductDto>(emptyCreateForm);
  const [creating, setCreating] = useState(false);

  const [editingProduct, setEditingProduct] =
    useState<AdminProductResponseDto | null>(null);
  const [editForm, setEditForm] = useState<EditForm | null>(null);
  const [savingEdit, setSavingEdit] = useState(false);

  const [stockEdits, setStockEdits] = useState<Record<string, string>>({});
  const [savingStockId, setSavingStockId] = useState<string | null>(null);

  async function loadProducts() {
    setError(null);
    try {
      const data = await fetchAdminProducts();
      setProducts(data);
    } catch (err) {
      const message = axios.isAxiosError(err)
        ? err.response?.data || err.message
        : "Failed to load products.";
      setError(String(message));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadProducts();
  }, []);

  function showSuccess(message: string) {
    setSuccess(message);
    setTimeout(() => setSuccess(null), 3000);
  }

  async function handleCreate(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setCreating(true);
    setError(null);

    try {
      await createProduct({
        ...createForm,
        price: Number(createForm.price),
        initialStock: Number(createForm.initialStock),
      });
      setCreateForm(emptyCreateForm);
      showSuccess("Product created.");
      await loadProducts();
    } catch (err) {
      const message = axios.isAxiosError(err)
        ? err.response?.data || err.message
        : "Failed to create product.";
      setError(String(message));
    } finally {
      setCreating(false);
    }
  }

  function openEdit(product: AdminProductResponseDto) {
    setEditingProduct(product);
    setEditForm({
      sku: product.sku,
      name: product.name,
      description: product.description ?? "",
      price: product.price.toString(),
      isActive: product.isActive,
    });
  }

  async function handleSaveEdit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!editingProduct || !editForm) return;

    setSavingEdit(true);
    setError(null);

    try {
      await updateProduct(editingProduct.id, {
        sku: editForm.sku,
        name: editForm.name,
        description: editForm.description,
        price: Number(editForm.price),
        isActive: editForm.isActive,
      });
      setEditingProduct(null);
      setEditForm(null);
      showSuccess("Product updated.");
      await loadProducts();
    } catch (err) {
      const message = axios.isAxiosError(err)
        ? err.response?.data || err.message
        : "Failed to update product.";
      setError(String(message));
    } finally {
      setSavingEdit(false);
    }
  }

  async function handleSaveStock(productId: string) {
    const value = stockEdits[productId];
    if (value === undefined) return;

    setSavingStockId(productId);
    setError(null);

    try {
      await updateProductStock(productId, {
        quantityAvailable: Number(value),
      });
      setStockEdits((prev) => {
        const next = { ...prev };
        delete next[productId];
        return next;
      });
      showSuccess("Stock updated.");
      await loadProducts();
    } catch (err) {
      const message = axios.isAxiosError(err)
        ? err.response?.data || err.message
        : "Failed to update stock.";
      setError(String(message));
    } finally {
      setSavingStockId(null);
    }
  }

  const activeCount = products.filter((p) => p.isActive).length;
  const lowStockCount = products.filter(
    (p) => p.isActive && p.quantityAvailable <= 5,
  ).length;

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-semibold text-slate-900">
          Product management
        </h1>
        <p className="mt-1 text-slate-600">
          Manage catalog, pricing, stock levels, and product visibility.
        </p>
      </div>

      {success && (
        <p className="mb-4 rounded-2xl bg-emerald-100 px-4 py-3 text-sm text-emerald-800">
          {success}
        </p>
      )}

      {error && (
        <p className="mb-4 rounded-2xl bg-red-100 px-4 py-3 text-sm text-red-700">
          {error}
        </p>
      )}

      <div className="mb-8 grid gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-slate-600">Total products</p>
          <p className="mt-1 text-2xl font-semibold">{products.length}</p>
        </div>
        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-slate-600">Active</p>
          <p className="mt-1 text-2xl font-semibold">{activeCount}</p>
        </div>
        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-slate-600">Low stock (≤ 5)</p>
          <p className="mt-1 text-2xl font-semibold text-amber-600">
            {lowStockCount}
          </p>
        </div>
      </div>

      <div className="mb-8 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-slate-900">Add new product</h2>
        <form
          onSubmit={handleCreate}
          className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
        >
          <Field label="SKU">
            <input
              value={createForm.sku}
              onChange={(e) =>
                setCreateForm({ ...createForm, sku: e.target.value })
              }
              required
              className={inputClass}
            />
          </Field>
          <Field label="Name">
            <input
              value={createForm.name}
              onChange={(e) =>
                setCreateForm({ ...createForm, name: e.target.value })
              }
              required
              className={inputClass}
            />
          </Field>
          <Field label="Price">
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={createForm.price || ""}
              onChange={(e) =>
                setCreateForm({ ...createForm, price: Number(e.target.value) })
              }
              required
              className={inputClass}
            />
          </Field>
          <Field label="Initial stock">
            <input
              type="number"
              min="0"
              value={createForm.initialStock || ""}
              onChange={(e) =>
                setCreateForm({
                  ...createForm,
                  initialStock: Number(e.target.value),
                })
              }
              required
              className={inputClass}
            />
          </Field>
          <Field label="Description" className="sm:col-span-2">
            <input
              value={createForm.description ?? ""}
              onChange={(e) =>
                setCreateForm({ ...createForm, description: e.target.value })
              }
              className={inputClass}
            />
          </Field>
          <div className="flex items-end">
            <button
              type="submit"
              disabled={creating}
              className="w-full rounded-xl bg-slate-900 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-slate-700 disabled:opacity-70"
            >
              {creating ? "Creating…" : "Create product"}
            </button>
          </div>
        </form>
      </div>

      <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-100 px-6 py-4">
          <h2 className="text-lg font-semibold text-slate-900">All products</h2>
        </div>

        {loading ? (
          <p className="p-6 text-slate-600">Loading products…</p>
        ) : products.length === 0 ? (
          <p className="p-6 text-slate-600">No products yet.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-slate-600">
                <tr>
                  <th className="px-4 py-3 font-medium">Product</th>
                  <th className="px-4 py-3 font-medium">SKU</th>
                  <th className="px-4 py-3 font-medium">Price</th>
                  <th className="px-4 py-3 font-medium">Stock</th>
                  <th className="px-4 py-3 font-medium">Reserved</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {products.map((product) => {
                  const stockValue =
                    stockEdits[product.id] ?? String(product.quantityAvailable);
                  const stockDirty =
                    stockEdits[product.id] !== undefined &&
                    stockEdits[product.id] !== String(product.quantityAvailable);

                  return (
                    <tr key={product.id} className="text-slate-700">
                      <td className="px-4 py-3">
                        <p className="font-medium text-slate-900">
                          {product.name}
                        </p>
                        {product.description && (
                          <p className="mt-0.5 max-w-xs truncate text-xs text-slate-500">
                            {product.description}
                          </p>
                        )}
                      </td>
                      <td className="px-4 py-3 font-mono text-xs">
                        {product.sku}
                      </td>
                      <td className="px-4 py-3">
                        ${product.price.toFixed(2)}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <input
                            type="number"
                            min="0"
                            value={stockValue}
                            onChange={(e) =>
                              setStockEdits((prev) => ({
                                ...prev,
                                [product.id]: e.target.value,
                              }))
                            }
                            className="w-20 rounded-lg border border-slate-300 px-2 py-1"
                          />
                          {stockDirty && (
                            <button
                              type="button"
                              onClick={() => handleSaveStock(product.id)}
                              disabled={savingStockId === product.id}
                              className="rounded-lg bg-slate-900 px-2 py-1 text-xs text-white hover:bg-slate-700 disabled:opacity-70"
                            >
                              {savingStockId === product.id ? "…" : "Save"}
                            </button>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3">{product.quantityReserved}</td>
                      <td className="px-4 py-3">
                        <span
                          className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                            product.isActive
                              ? "bg-emerald-100 text-emerald-800"
                              : "bg-slate-200 text-slate-600"
                          }`}
                        >
                          {product.isActive ? "Active" : "Inactive"}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <button
                          type="button"
                          onClick={() => openEdit(product)}
                          className="text-sm font-medium text-slate-900 hover:underline"
                        >
                          Edit
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {editingProduct && editForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl">
            <h2 className="text-xl font-semibold text-slate-900">
              Edit product
            </h2>
            <form onSubmit={handleSaveEdit} className="mt-4 space-y-4">
              <Field label="SKU">
                <input
                  value={editForm.sku}
                  onChange={(e) =>
                    setEditForm({ ...editForm, sku: e.target.value })
                  }
                  required
                  className={inputClass}
                />
              </Field>
              <Field label="Name">
                <input
                  value={editForm.name}
                  onChange={(e) =>
                    setEditForm({ ...editForm, name: e.target.value })
                  }
                  required
                  className={inputClass}
                />
              </Field>
              <Field label="Description">
                <textarea
                  value={editForm.description}
                  onChange={(e) =>
                    setEditForm({ ...editForm, description: e.target.value })
                  }
                  rows={3}
                  className={inputClass}
                />
              </Field>
              <Field label="Price">
                <input
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={editForm.price}
                  onChange={(e) =>
                    setEditForm({ ...editForm, price: e.target.value })
                  }
                  required
                  className={inputClass}
                />
              </Field>
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <input
                  type="checkbox"
                  checked={editForm.isActive}
                  onChange={(e) =>
                    setEditForm({ ...editForm, isActive: e.target.checked })
                  }
                  className="rounded border-slate-300"
                />
                Product is active (visible in shop)
              </label>

              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => {
                    setEditingProduct(null);
                    setEditForm(null);
                  }}
                  className="rounded-xl border border-slate-300 px-4 py-2 text-sm text-slate-700 hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={savingEdit}
                  className="rounded-xl bg-slate-900 px-4 py-2 text-sm text-white hover:bg-slate-700 disabled:opacity-70"
                >
                  {savingEdit ? "Saving…" : "Save changes"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

const inputClass =
  "w-full rounded-xl border border-slate-300 bg-slate-50 px-3 py-2 outline-none focus:border-slate-500";

function Field({
  label,
  children,
  className = "",
}: {
  label: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <label className={`block ${className}`}>
      <span className="mb-1 block text-sm font-medium text-slate-700">
        {label}
      </span>
      {children}
    </label>
  );
}
