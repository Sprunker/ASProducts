import { useNavigate, useParams } from "react-router-dom";
import { useProduct, useCreateProduct, useUpdateProduct } from "@/features/products/hooks";
import { ProductForm } from "@/features/products/ProductForm";
import { ProductFormSkeleton } from "@/features/products/ProductFormSkeleton";
import { ErrorMessage } from "@/components/common/ErrorMessage";

export function ProductFormPage() {
  const { id } = useParams<{ id: string }>();
  const productId = id ? Number(id) : undefined;
  const isEditing = productId !== undefined;
  const navigate = useNavigate();

  const { data: product, isLoading, isError } = useProduct(productId);
  const createProduct = useCreateProduct();
  const updateProduct = useUpdateProduct();

  const isSubmitting = createProduct.isPending || updateProduct.isPending;

  const handleSubmit = (formData: FormData) => {
    if (isEditing && productId) {
      updateProduct.mutate(
        { id: productId, formData },
        { onSuccess: () => navigate(`/products/${productId}`) }
      );
    } else {
      createProduct.mutate(formData, {
        onSuccess: (created) => navigate(`/products/${created.id}`),
      });
    }
  };

  if (isEditing && isLoading) {
    return (
      <div className="container">
        <ProductFormSkeleton />
      </div>
    );
  }
  if (isEditing && isError) return <ErrorMessage message="No se pudo cargar el producto a editar." />;

  return (
    <div className="container">
      <ProductForm
        product={isEditing ? product : undefined}
        onSubmit={handleSubmit}
        isSubmitting={isSubmitting}
      />
    </div>
  );
}