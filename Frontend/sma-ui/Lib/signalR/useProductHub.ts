import { useEffect, useRef } from "react";
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
    ""
  );

export function useProductHub(
  onProductUpdated: (payload: ProductUpdatePayload) => void
) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const callbackRef = useRef(onProductUpdated);

  // Keep callback reference updated without triggering re-connects
  useEffect(() => {
    callbackRef.current = onProductUpdated;
  }, [onProductUpdated]);

  useEffect(() => {
    // Instantiate single HubConnection on ref if not created yet
    if (!connectionRef.current) {
      connectionRef.current = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/hubs/productHub`)
        .withAutomaticReconnect()
        .build();
    }

    const connection = connectionRef.current;

    // Attach event handler using stable wrapper
    const handleProductUpdated = (payload: ProductUpdatePayload) => {
      callbackRef.current(payload);
    };

    connection.on("ProductUpdated", handleProductUpdated);

    // Start connection only if completely disconnected
    if (connection.state === signalR.HubConnectionState.Disconnected) {
      connection
        .start()
        .then(() => {
          console.log("SignalR Connected");
          return connection.invoke("JoinProductGroup", "products");
        })
        .catch((err) => {
          // Ignore negotiation cancellations caused by React dev double-mounts
          if (err?.message?.includes("stopped during negotiation")) return;
          console.error("SignalR Connection Error:", err);
        });
    }

    return () => {
      connection.off("ProductUpdated", handleProductUpdated);
      // Avoid calling connection.stop() during negotiation/mount transitions
    };
  }, []);
}