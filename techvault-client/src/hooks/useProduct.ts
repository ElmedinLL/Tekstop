import { useQuery } from "@tanstack/react-query";
import { api } from "../lib/api";
import type { ProductDetail } from "../types/product";

/** Missing or unpublished products: API returns 200 with JSON null (no HTTP 404, so DevTools stays clean). */
export async function fetchProductDetail(
  id: number,
): Promise<ProductDetail | null> {
  const { data } = await api.get<ProductDetail | null>(`/products/${id}`);
  return data ?? null;
}

export function useProduct(id: number | null | undefined) {
  const validId =
    typeof id === "number" && Number.isFinite(id) && id > 0 ? id : null;

  return useQuery({
    queryKey: ["product", validId],
    queryFn: () => fetchProductDetail(validId!),
    enabled: validId != null,
  });
}
