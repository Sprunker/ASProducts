import { httpClient } from "@/api/httpClient";
import type { Product } from "@/features/products/product.types";

export const productsApi = {
  getAll: (): Promise<Product[]> =>
    httpClient.get("/api/products").then((r) => r.data),

  getById: (id: number): Promise<Product> =>
    httpClient.get(`/api/products/${id}`).then((r) => r.data),

  create: (formData: FormData): Promise<Product> =>
    httpClient.post("/api/products", formData).then((r) => r.data),

  update: (id: number, formData: FormData): Promise<Product> =>
    httpClient.put(`/api/products/${id}`, formData).then((r) => r.data),

  remove: (id: number): Promise<void> =>
    httpClient.delete(`/api/products/${id}`),
};