import type { Product } from "./product.types";
import { ProductCard } from "./ProductCard";

interface ProductGridProps {
  products: Product[];
  onDeleteRequest: (product: Product) => void;
}

export function ProductGrid({ products, onDeleteRequest }: ProductGridProps) {
  if (products.length === 0) {
    return (
      <div className="text-center text-muted py-5">
        No hay productos registrados todavía.
      </div>
    );
  }

  return (
    <div className="product-grid">
      {products.map((product) => (
        <div className="product-grid__item" key={product.id}>
          <ProductCard product={product} onDeleteRequest={onDeleteRequest} />
        </div>
      ))}
    </div>
  );
}