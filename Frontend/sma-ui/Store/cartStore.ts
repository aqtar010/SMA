import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import { ProductResponseDto } from "@/DTOs/ProductDTOs";

type CartItem = ProductResponseDto & { quantity: number };

interface CartState {
    items: CartItem[];
    addItem: (product: ProductResponseDto) => void;
    removeItem: (id: string) => void;
    updateQuantity: (id: string, quantity: number) => void;
    clearCart: () => void;
}

export const useCartStore = create<CartState>()(
    persist(
        (set, get) => ({
            items: [],

            addItem: (product) => {
                if (product.quantityAvailable <= 0) return;

                const existing = get().items.find((item) => item.id === product.id);

                if (existing) {
                    if (existing.quantity >= product.quantityAvailable) return;

                    set({
                        items: get().items.map((item) =>
                            item.id === product.id
                                ? { ...item, quantity: item.quantity + 1 }
                                : item,
                        ),
                    });
                } else {
                    set({
                        items: [...get().items, { ...product, quantity: 1 }],
                    });
                }
            },

            removeItem: (id) =>
                set({
                    items: get().items.filter((item) => item.id !== id),
                }),

            updateQuantity: (id, quantity) => {
                if (quantity <= 0) {
                    get().removeItem(id);
                    return;
                }

                set({
                    items: get().items.map((item) =>
                        item.id === id ? { ...item, quantity } : item,
                    ),
                });
            },

            clearCart: () => set({ items: [] }),
        }),
        {
            name: "sma-cart",
            storage: createJSONStorage(() => localStorage),
        },
    ),
);
