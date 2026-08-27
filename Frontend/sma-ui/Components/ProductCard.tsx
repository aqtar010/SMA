"use client";

import { useEffect, useState } from "react";
import axios from "axios";
import { useCartStore } from "@/Store/cartStore";
import { ProductRatingSummaryDto, ProductResponseDto } from "@/DTOs/ProductDTOs";
import { getProductRatingSummary, saveProductRating } from "@/Lib/ProductApis";

interface ProductCardProps {
  product: ProductResponseDto;
}

export default function ProductCard({ product }: ProductCardProps) {
  const addItem = useCartStore((s) => s.addItem);
  const cartQuantity = useCartStore(
    (s) => s.items.find((item) => item.id === product.id)?.quantity ?? 0,
  );

  const [added, setAdded] = useState(false);
  const [ratingSummary, setRatingSummary] = useState<ProductRatingSummaryDto | null>(null);
  const [rating, setRating] = useState(0);
  const [feedback, setFeedback] = useState("");
  const [showRatingForm, setShowRatingForm] = useState(false);
  const [ratingError, setRatingError] = useState<string | null>(null);
  const [savingRating, setSavingRating] = useState(false);
  const inStock = product.quantityAvailable > 0;
  const atMaxInCart = cartQuantity >= product.quantityAvailable;

  useEffect(() => {
    getProductRatingSummary(product.id)
      .then((summary) => {
        setRatingSummary(summary);
        if (summary.currentUserRating) setRating(summary.currentUserRating);
      })
      .catch(() => undefined);
  }, [product.id]);

  async function handleRatingSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!rating) return;
    setSavingRating(true);
    setRatingError(null);
    try {
      const summary = await saveProductRating(product.id, { rating, feedback });
      setRatingSummary(summary);
      setShowRatingForm(false);
    } catch (error) {
      setRatingError(axios.isAxiosError(error) ? String(error.response?.data ?? error.message) : "Could not save your rating.");
    } finally {
      setSavingRating(false);
    }
  }

  function handleAddToCart() {
    if (!inStock || atMaxInCart) return;

    addItem(product);
    setAdded(true);
    setTimeout(() => setAdded(false), 1500);
  }

  return (
    <div
      className={`flex w-full flex-col justify-between rounded-2xl border p-5 shadow-sm transition sm:w-[calc(50%-0.5rem)] lg:w-[calc(33.333%-0.75rem)] ${
        inStock
          ? "border-slate-200 bg-white hover:border-slate-300"
          : "border-slate-100 bg-slate-50 opacity-75"
      }`}
    >
      <div>
        <div className="mb-3 flex items-start justify-between gap-2">
          <h3 className="font-semibold text-slate-900">{product.name}</h3>
          <span
            className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${
              inStock
                ? "bg-emerald-100 text-emerald-800"
                : "bg-slate-200 text-slate-600"
            }`}
          >
            {inStock ? `${product.quantityAvailable} in stock` : "Out of stock"}
          </span>
        </div>

        <p className="text-sm text-slate-500">{product.sku}</p>

        <div className="mt-2 flex items-center gap-2 text-sm">
          <span className="tracking-wide text-amber-500">{"★".repeat(Math.round(ratingSummary?.averageRating ?? 0))}{"☆".repeat(5 - Math.round(ratingSummary?.averageRating ?? 0))}</span>
          <span className="text-slate-500">{ratingSummary ? `${ratingSummary.averageRating.toFixed(1)} (${ratingSummary.ratingCount})` : "No ratings"}</span>
        </div>

        {product.description && (
          <p className="mt-2 line-clamp-2 text-sm text-slate-600">
            {product.description}
          </p>
        )}

        <p className="mt-3 text-xl font-semibold text-slate-900">
          ${product.price.toFixed(2)}
        </p>
      </div>

      <div className="mt-4">
        {cartQuantity > 0 && (
          <p className="mb-2 text-sm text-slate-600">
            {cartQuantity} in cart
          </p>
        )}

        <button
          type="button"
          onClick={handleAddToCart}
          disabled={!inStock || atMaxInCart}
          className={`w-full rounded-xl px-4 py-2.5 text-sm font-medium transition ${
            added
              ? "bg-emerald-600 text-white"
              : inStock && !atMaxInCart
                ? "bg-slate-900 text-white hover:bg-slate-700"
                : "cursor-not-allowed bg-slate-200 text-slate-500"
          }`}
        >
          {added
            ? "Added!"
            : atMaxInCart
              ? "Max quantity reached"
              : inStock
                ? "Add to cart"
                : "Out of stock"}
        </button>

        {ratingSummary?.canRate && (
          <button type="button" onClick={() => setShowRatingForm((visible) => !visible)} className="mt-3 text-sm font-medium text-teal-700 hover:underline">
            {ratingSummary.currentUserRating ? "Edit your review" : "Rate this product"}
          </button>
        )}

        {showRatingForm && ratingSummary?.canRate && (
          <form onSubmit={handleRatingSubmit} className="mt-3 space-y-2 border-t border-slate-200 pt-3">
            <div className="flex gap-1" aria-label="Choose a rating">
              {Array.from({ length: 5 }, (_, index) => index + 1).map((value) => (
                <button key={value} type="button" onClick={() => setRating(value)} aria-label={`${value} stars`} className={`text-xl ${value <= rating ? "text-amber-500" : "text-slate-300"}`}>★</button>
              ))}
            </div>
            <textarea value={feedback} onChange={(event) => setFeedback(event.target.value)} maxLength={2000} placeholder="Share your feedback (optional)" className="min-h-20 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm" />
            {ratingError && <p className="text-xs text-red-700">{ratingError}</p>}
            <button type="submit" disabled={!rating || savingRating} className="rounded-lg bg-slate-900 px-3 py-2 text-sm font-medium text-white disabled:opacity-50">{savingRating ? "Saving..." : "Submit rating"}</button>
          </form>
        )}
      </div>
    </div>
  );
}
