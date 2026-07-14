import { useState } from "react";
import { useProducts, useDeleteProduct } from "@/features/products/hooks";
import { ProductGrid } from "@/features/products/ProductGrid";
import { ProductGridSkeleton } from "@/features/products/ProductGridSkeleton";
import { ConfirmModal } from "@/components/common/ConfirmModal";
import { ErrorMessage } from "@/components/common/ErrorMessage";
import type { Product } from "@/features/products/product.types";

export function ProductListPage() {
  const { data: products, isLoading, isError } = useProducts();
  const deleteProduct = useDeleteProduct();

  const [productToDelete, setProductToDelete] = useState<Product | null>(null);

  const handleConfirmDelete = () => {
    if (!productToDelete) return;
    deleteProduct.mutate(productToDelete.id, {
      onSettled: () => setProductToDelete(null),
    });
  };

  return (
    <div className="container">
      {isLoading && <ProductGridSkeleton count={8} />}
      {isError && <ErrorMessage message="No se pudieron cargar los productos." />}

      {products && products.length === 0 && (
        <div className="alert alert-primary notify-message fw-semibold" role="alert">
          ℹ️ Aún no hay productos registrados.
        </div>
      )}
      {products && products.length > 0 && (
        <ProductGrid products={products} onDeleteRequest={setProductToDelete} />
      )}

      <ConfirmModal
        show={!!productToDelete}
        title="⚠️ Eliminar producto"
        message={`¿Estás seguro de eliminar "${productToDelete?.name}"? Esta acción no se puede deshacer.`}
        onConfirm={handleConfirmDelete}
        onCancel={() => setProductToDelete(null)}
      />
    </div>
  );
}