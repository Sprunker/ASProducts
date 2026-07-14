import { NavLink } from "react-router-dom";

export function Navbar() {
  return (
    <nav className="navbar navbar-expand-lg app-navbar">
      <div className="container">
        <NavLink to="/products" className="navbar-brand d-flex fs-40 fw-bold">
          <img src="/as.png" className="w-54"/> ASProducts
        </NavLink>
        <div className="navbar-nav app-navbar__links fw-bold">
          <NavLink
            to="/products/create"
            className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
          >
            Nuevo Producto ➕
          </NavLink>
        </div>
      </div>
    </nav>
  );
}