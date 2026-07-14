import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { productsApi } from "@/api/products.api";
import { env } from "@/config/env";
import { productMock } from "./product.mock";
import type { Product } from "@/features/products/product.types";
import { useToast } from "@/components/common/ToastContext";

const productSource = env.VITE_USE_MOCK ? productMock : productsApi;

export const useProducts = () =>
  useQuery<Product[]>({
    queryKey: ["products"],
    queryFn: productSource.getAll,
  });

export const useProduct = (id?: number) =>
  useQuery<Product>({
    queryKey: ["products", id],
    queryFn: () => productSource.getById(id!),
    enabled: !!id,
  });

export const useCreateProduct = () => {
  const qc = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: (formData: FormData) => productSource.create(formData),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["products"] });
      showToast({ message: "Producto creado exitosamente.", variant: "success" });
    },
    onError: () => {
      showToast({ message: "No se pudo crear el producto.", variant: "danger" });
    },
  });
};

export const useUpdateProduct = () => {
  const qc = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: ({ id, formData }: { id: number; formData: FormData }) =>
      productSource.update(id, formData),
    onSuccess: (data, { id }) => {
      qc.invalidateQueries({ queryKey: ["products"] });
      qc.setQueryData<Product>(["products", id], {
        ...data,
        _imageVersion: Date.now(),
      });

      showToast({ message: "Producto actualizado exitosamente.", variant: "success" });
    },
    onError: () => {
      showToast({ message: "No se pudo actualizar el producto.", variant: "danger" });
    },
  });
};

export const useDeleteProduct = () => {
  const qc = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: (id: number) => productSource.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["products"] });
      showToast({ message: "Producto eliminado exitosamente.", variant: "success" });
    },
    onError: () => {
      showToast({ message: "No se pudo eliminar el producto.", variant: "danger" });
    },
  });
};