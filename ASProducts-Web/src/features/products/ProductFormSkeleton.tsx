export function ProductFormSkeleton() {
  return (
    <div className="product-form product-form-placeholder placeholder-glow" aria-hidden="true">
      <div className="product-form__header mb-4">
        <span className="placeholder col-4" style={{ height: 32, display: "inline-block" }}></span>
        <span className="placeholder" style={{ width: 130, height: 18 }} />
      </div>

      <div className="product-form__grid">
        <div className="product-form__image-col">
          <span
            className="placeholder rounded"
            style={{ width: "100%", height: 325, display: "inline-block" }}
          ></span>
        </div>

        <div className="product-form__fields-col">
          <div className="product-form__group">
            <span className="placeholder col-2 mb-1 d-block" style={{ height: 14 }}></span>
            <span className="placeholder col-12" style={{ height: 38, display: "inline-block" }}></span>
          </div>

          <div className="product-form__group">
            <span className="placeholder col-2 mb-1 d-block" style={{ height: 14 }}></span>
            <span className="placeholder col-12" style={{ height: 38, display: "inline-block" }}></span>
          </div>

          <div className="product-form__row">
            <div className="product-form__group">
              <span className="placeholder col-4 mb-1 d-block" style={{ height: 14 }}></span>
              <span className="placeholder col-12" style={{ height: 38, display: "inline-block" }}></span>
            </div>

            <div className="product-form__group">
              <span className="placeholder col-3 mb-1 d-block" style={{ height: 14 }}></span>
              <span className="placeholder col-12" style={{ height: 38, display: "inline-block" }}></span>
            </div>
          </div>

          <div className="product-form__group">
            <span className="placeholder col-2 mb-1 d-block" style={{ height: 14 }}></span>
            <span className="placeholder col-12" style={{ height: 72, display: "inline-block" }}></span>
          </div>
        </div>
      </div>

      <div className="form-actions">
        <span className="placeholder btn-sm" style={{ width: 140, display: "inline-block" }}></span>
      </div>
    </div>
  );
}