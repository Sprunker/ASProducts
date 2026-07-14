export function ProductCardSkeleton() {
  return (
    <div className="card h-100 product-card position-relative product-card-placeholder" aria-hidden="true">
      <span
        className="product-card__badge position-absolute start-0 m-2 placeholder"
        style={{ width: 70, height: 20 }}
      ></span>

      <span
        className="placeholder card-img-top"
        style={{ height: 180, width: "100%" }}
      ></span>

      <div className="card-body product-card__body">
        <h5 className="card-title placeholder-glow mb-2">
          <span className="placeholder col-8"></span>
        </h5>
        <p className="card-text placeholder-glow small mb-1">
          <span className="placeholder col-5"></span>
        </p>
        <p className="card-text placeholder-glow mb-2">
          <span className="placeholder col-4"></span>
        </p>
      </div>

      <div className="product-card__actions d-flex flex-column position-absolute end-0 m-2 placeholder-glow">
        <span className="placeholder btn btn-sm mb-1" style={{ width: 40 }}></span>
        <span className="placeholder btn btn-sm mb-1" style={{ width: 40 }}></span>
        <span className="placeholder btn btn-sm" style={{ width: 40 }}></span>
      </div>
    </div>
  );
}