import { useState } from "react";
import { Link, useParams, useNavigate } from "react-router-dom";
import { useProduct, useDeleteProduct } from "@/features/products/hooks";
import { ErrorMessage } from "@/components/common/ErrorMessage";
import { getProductImageUrl } from "@/features/products/product.types";
import { ProductDetailSkeleton } from "@/features/products/ProductDetailSkeleton";
import { ConfirmModal } from "@/components/common/ConfirmModal";

export function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const productId = id ? Number(id) : undefined;

  const { data: product, isLoading, isError } = useProduct(productId);
  const deleteProduct = useDeleteProduct();

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  if (isLoading) return <ProductDetailSkeleton />;
  if (isError || !product) return <ErrorMessage message="Producto no encontrado." />;

  const handleDelete = () => {
    deleteProduct.mutate(product.id, {
      onSuccess: () => {
        setIsDeleteModalOpen(false);
        navigate("/products");
      },
    });
  };

  return (
    <div className="container">
      <div className="product-detail">
        <div className="product-detail__header">
          <h1 className="page-heading mb-0">Detalles</h1>
          <Link to="/products" className="product-detail__back">
            ← Volver al listado
          </Link>
        </div>

        <div className="product-detail__grid">
          <div className="product-detail__image">
            <img
              src={getProductImageUrl(product)}
              alt={product.name}
              className="product-detail__image-el"
            />
          </div>

          <div className="product-detail__info">
            <h2 className="product-detail__name">{product.name}</h2>
            <p className="product-detail__company">{product.company}</p>
            <p className="product-detail__price">
              {new Intl.NumberFormat("es-MX", {
                style: "currency",
                currency: "MXN",
              }).format(product.price)}
            </p>

            {product.restrictionAge !== undefined && product.restrictionAge > 0 && (
              <span className="product-detail__age-badge">
                +{product.restrictionAge} años
              </span>
            )}

            {product.description && (
              <p className="product-detail__description">{product.description}</p>
            )}

            <div className="product-detail__actions">
              <Link to={`/products/${product.id}/edit`} className="btn">
                Editar ✏️
              </Link>
              <button
                type="button"
                className="btn btn-danger"
                onClick={() => setIsDeleteModalOpen(true)}
                disabled={deleteProduct.isPending}
              >
                Eliminar ❌
              </button>
            </div>
          </div>
        </div>
      </div>

      <ConfirmModal
        show={isDeleteModalOpen}
        title="⚠️ Eliminar producto"
        message={`¿Estás seguro de eliminar "${product.name}"? Esta acción no se puede deshacer.`}
        onConfirm={handleDelete}
        onCancel={() => setIsDeleteModalOpen(false)}
      />
    </div>
  );
}