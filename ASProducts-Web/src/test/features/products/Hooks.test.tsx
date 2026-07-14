import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import type { Product } from "@/features/products/product.types";

const showToast = vi.fn();
vi.mock("@/components/common/ToastContext", () => ({
  useToast: () => ({ showToast }),
}));

vi.mock("@/config/env", () => ({
  env: { VITE_USE_MOCK: false, VITE_API_BASE_URL: "http://localhost:1234" },
}));

const getAllMock = vi.fn();
const createMock = vi.fn();
vi.mock("@/api/products.api", () => ({
  productsApi: {
    getAll: (...args: unknown[]) => getAllMock(...args),
    getById: vi.fn(),
    create: (...args: unknown[]) => createMock(...args),
    update: vi.fn(),
    remove: vi.fn(),
  },
}));

const { useProducts, useCreateProduct } = await import("@/features/products/hooks");

const buildWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  const Wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
  return { Wrapper, queryClient };
};

const sampleProducts: Product[] = [
  { id: 1, name: "Mouse", company: "TechCo", price: 20.50 },
];

describe("useProducts", () => {
  beforeEach(() => {
    getAllMock.mockReset();
    createMock.mockReset();
    showToast.mockReset();
  });

  it("Expone estado 'pending' mientras la petición está en curso", () => {
    getAllMock.mockReturnValue(new Promise(() => {}));
    const { Wrapper } = buildWrapper();

    const { result } = renderHook(() => useProducts(), { wrapper: Wrapper });

    expect(result.current.isPending).toBe(true);
    expect(result.current.data).toBeUndefined();
  });

  it("Expone los datos cuando la API responde exitosamente", async () => {
    getAllMock.mockResolvedValue(sampleProducts);
    const { Wrapper } = buildWrapper();

    const { result } = renderHook(() => useProducts(), { wrapper: Wrapper });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(sampleProducts);
  });

  it("Expone estado de error cuando la API falla", async () => {
    getAllMock.mockRejectedValue(new Error("Network error"));
    const { Wrapper } = buildWrapper();

    const { result } = renderHook(() => useProducts(), { wrapper: Wrapper });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error?.message).toBe("Network error");
  });
});

describe("useCreateProduct", () => {
  beforeEach(() => {
    getAllMock.mockReset();
    createMock.mockReset();
    showToast.mockReset();
  });

  it("Invalida la query de 'products' y muestra un toast de éxito al crear", async () => {
    const newProduct: Product = { id: 2, name: "Teclado", company: "TechCo", price: 49.99 };
    createMock.mockResolvedValue(newProduct);
    const { Wrapper, queryClient } = buildWrapper();
    const invalidateSpy = vi.spyOn(queryClient, "invalidateQueries");

    const { result } = renderHook(() => useCreateProduct(), { wrapper: Wrapper });

    result.current.mutate(new FormData());

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ["products"] });
    expect(showToast).toHaveBeenCalledWith({
      message: "Producto creado exitosamente.",
      variant: "success",
    });
  });

  it("Muestra un toast de error si la creación falla", async () => {
    createMock.mockRejectedValue(new Error("No se pudo crear el producto."));
    const { Wrapper } = buildWrapper();

    const { result } = renderHook(() => useCreateProduct(), { wrapper: Wrapper });

    result.current.mutate(new FormData());

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(showToast).toHaveBeenCalledWith({
      message: "No se pudo crear el producto.",
      variant: "danger",
    });
  });
});