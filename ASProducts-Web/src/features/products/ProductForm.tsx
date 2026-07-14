import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { productSchema, type ProductFormInput, type ProductFormOutput } from "./product.schema";
import type { Product } from "./product.types";
import { ImageUploader } from "@/components/ui/ImageUploader";
import { Link } from "react-router-dom";

interface ProductFormProps {
  product?: Product;
  onSubmit: (formData: FormData) => void;
  isSubmitting: boolean;
}

export function ProductForm({ product, onSubmit, isSubmitting }: ProductFormProps) {
  const {
    register,
    handleSubmit,
    control,
    setValue,
    formState: { errors },
  } = useForm<ProductFormInput, unknown, ProductFormOutput>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      name: product?.name ?? "",
      description: product?.description ?? "",
      restrictionAge: product?.restrictionAge ?? "",
      company: product?.company ?? "",
      price: product?.price ?? undefined,
      image: null,
      removeImage: false,
    },
  });

  const handleFormSubmit = (data: ProductFormOutput) => {
    const formData = new FormData();
    formData.append("name", data.name);
    formData.append("description", data.description ?? "");

    if (data.restrictionAge !== undefined) {
      formData.append("restrictionAge", String(data.restrictionAge));
    }

    formData.append("company", data.company);
    formData.append("price", data.price.toFixed(2));

    if (data.image) {
      formData.append("image", data.image);
    } else if (data.removeImage) {
      formData.append("removeImage", "true");
    }

    onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} noValidate className="product-form">
      <div className="product-form__header">
        <h1 className="page-heading">{product ? "Editar Producto" : "Nuevo Producto"}</h1>
        {product && <Link to={`/products/${product?.id}`} className="product-form__back">
          ← Ir al producto
        </Link>}
        <Link to="/products" className="product-form__back">
          ← Volver al listado
        </Link>
      </div>

      <div className="product-form__grid">
        <div className="product-form__image-col">
          <Controller
            name="image"
            control={control}
            render={({ field: { onChange } }) => (
              <ImageUploader
                existingImageUrl={product?.imageUrl}
                onChange={(file) => {
                  onChange(file);
                  if (file) setValue("removeImage", false); // seleccionar nueva imagen cancela la remoción
                }}
                onRemoveExisting={() => setValue("removeImage", true)}
                error={errors.image?.message as string | undefined}
              />
            )}
          />
        </div>

        <div className="product-form__fields-col">
          <div className="product-form__group">
            <label htmlFor="product-name" className="form-label">Nombre *</label>
            <input
              id="product-name"
              type="text"
              maxLength={50}
              className={`form-control ${errors.name ? "is-invalid" : ""}`}
              {...register("name")}
            />
            {errors.name && <div className="invalid-feedback">{errors.name.message}</div>}
          </div>

          <div className="product-form__group">
            <label htmlFor="product-company" className="form-label">Compañía *</label>
            <input
              id="product-company"
              type="text"
              maxLength={50}
              className={`form-control ${errors.company ? "is-invalid" : ""}`}
              {...register("company")}
            />
            {errors.company && <div className="invalid-feedback">{errors.company.message}</div>}
          </div>

          <div className="product-form__row">
            <div className="product-form__group">
              <label htmlFor="product-restrictionAge" className="form-label">Restricción de Edad</label>
              <input
                id="product-restrictionAge"
                type="number"
                min={0}
                max={100}
                className={`form-control ${errors.restrictionAge ? "is-invalid" : ""}`}
                {...register("restrictionAge")}
              />
              {errors.restrictionAge && (
                <div className="invalid-feedback">{errors.restrictionAge.message}</div>
              )}
            </div>

            <div className="product-form__group">
              <label htmlFor="product-price" className="form-label">Precio *</label>
              <div className="input-group has-validation">
                <span className="input-group-text">$</span>
                <input
                  id="product-price"
                  type="number"
                  step="0.01"
                  min={1}
                  max={1000}
                  className={`form-control ${errors.price ? "is-invalid" : ""}`}
                  {...register("price")}
                />
                {errors.price && <div className="invalid-feedback">{errors.price.message}</div>}
              </div>
            </div>
          </div>

          <div className="product-form__group">
            <label htmlFor="product-description" className="form-label">Descripción</label>
            <textarea
              id="product-description"
              maxLength={100}
              rows={3}
              className={`form-control ${errors.description ? "is-invalid" : ""}`}
              {...register("description")}
            />
            {errors.description && (
              <div className="invalid-feedback">{errors.description.message}</div>
            )}
          </div>
        </div>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn-sm btn-primary" disabled={isSubmitting}>
          {isSubmitting ? "Guardando..." : "Guardar Producto"}
        </button>
      </div>
    </form>
  );
}