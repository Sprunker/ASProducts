import { Link } from "react-router-dom";
import { getProductImageUrl, type Product } from "./product.types";

interface ProductCardProps {
  product: Product;
  onDeleteRequest: (product: Product) => void;
}

export function ProductCard({ product, onDeleteRequest }: ProductCardProps) {
  return (
    <div className="card h-100 product-card position-relative">
      {product.restrictionAge !== undefined && product.restrictionAge > 0 && (
        <span className="product-card__badge position-absolute start-0 m-2">
          +{product.restrictionAge} años
        </span>
      )}

      <img
        src={getProductImageUrl(product)}
        className="product-card__image"
        alt={product.name}
      />
      <div className="card-body product-card__body">
        <h5 className="card-title product-card__title bold text-truncate">{product.name}</h5>
        <p className="card-text product-card__company small mb-1">{product.company}</p>
        <p className="card-text fw-bold mb-2">
          {new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" }).format(product.price)}
        </p>
      </div>

      <div className="product-card__actions d-flex flex-column position-absolute end-0 m-2">
        <button
          type="button"
          className="btn-xs btn-danger"
          aria-label={`Eliminar ${product.name}`}
          onClick={() => onDeleteRequest(product)}
        >
          ❌
        </button>
        <Link to={`/products/${product.id}/edit`} className="btn-xs" aria-label={`Editar ${product.name}`}>
          ✏️
        </Link>
        <Link to={`/products/${product.id}`} className="btn-xs" aria-label={`Ver detalle de ${product.name}`}>
          👁️
        </Link>
      </div>
    </div>
  );
}