import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";

export type ProductUpdatePayload = {
  id?: string;
  productId?: string;
  sku?: string;
  name?: string;
  description?: string;
  price?: number;
  isActive?: boolean;
  quantityAvailable?: number;
  updatedAt?: string;
};

const API_BASE_URL =
  (process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api").replace(
    /\/api\/?$/,
    "",
  );

export function useProductHub(
  onProductUpdated: (payload: ProductUpdatePayload) => void,
) {
  useEffect(() => {
    let cancelled = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/productHub`)
      .withAutomaticReconnect()
      .build();

    connection.on("ProductUpdated", onProductUpdated);

    const startConnection = async () => {
      try {
        await connection.start();

        if (cancelled) {
          await connection.stop();
          return;
        }

        await connection.invoke("JoinProductGroup", "products");
      } catch (err) {
        if (!cancelled) {
          console.error("SignalR connection failed:", err);
        }
      }
    };

    void startConnection();

    return () => {
      cancelled = true;
      connection.off("ProductUpdated", onProductUpdated);
      connection.stop().catch(() => undefined);
    };
  }, [onProductUpdated]);
}
