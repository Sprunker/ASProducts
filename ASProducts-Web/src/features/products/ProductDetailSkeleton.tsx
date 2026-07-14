export function ProductDetailSkeleton() {
  return (
    <div className="container">
      <div className="product-detail product-detail-placeholder placeholder-glow">
        <div className="product-detail__header">
          <span className="placeholder page-heading" style={{ width: 140, display: "inline-block" }} />
          <span className="placeholder" style={{ width: 130, height: 18 }} />
        </div>

        <div className="product-detail__grid">
          <div className="product-detail__image">
            <span className="placeholder w-100 h-100" />
          </div>

          <div className="product-detail__info">
            <span className="placeholder product-detail__name" style={{ width: "70%", display: "block" }} />
            <span className="placeholder product-detail__company" style={{ width: "40%", display: "block" }} />
            <span className="placeholder product-detail__price" style={{ width: "30%", display: "block" }} />
            <span className="placeholder product-detail__age-badge" style={{ width: 90, display: "inline-block" }} />
            <span className="placeholder" style={{ width: "100%", height: 14, display: "block" }} />
            <span className="placeholder" style={{ width: "90%", height: 14, display: "block", marginTop: 6 }} />
            <span className="placeholder" style={{ width: "60%", height: 14, display: "block", marginTop: 6 }} />
          </div>
        </div>

        <div className="product-detail__actions">
          <span className="placeholder" style={{ width: 110, height: 40 }} />
          <span className="placeholder" style={{ width: 90, height: 40 }} />
          <span className="placeholder" style={{ width: 100, height: 40 }} />
        </div>
      </div>
    </div>
  );
}