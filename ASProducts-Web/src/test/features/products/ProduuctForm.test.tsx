import { describe, it, expect, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { ProductForm } from "@/features/products/ProductForm";
import type { Product } from "@/features/products/product.types";

const getInput = (label: string | RegExp) =>
  screen.getByLabelText(label) as HTMLInputElement | HTMLTextAreaElement;

const renderForm = (props: Partial<Parameters<typeof ProductForm>[0]> = {}) => {
  const onSubmit = vi.fn();
  render(
    <MemoryRouter>
      <ProductForm onSubmit={onSubmit} isSubmitting={false} {...props} />
    </MemoryRouter>
  );
  return { onSubmit };
};

describe("ProductForm", () => {
  it("Renderiza en modo creación con campos vacíos", () => {
    renderForm();

    expect(screen.getByRole("heading", { name: "Nuevo Producto" })).toBeInTheDocument();
    expect(getInput(/Nombre/)).toHaveValue("");
    expect(screen.getByRole("button", { name: "Guardar Producto" })).toBeEnabled();
  });

  it("Precarga los valores cuando se edita un producto existente", () => {
    const product: Product = {
      id: 7,
      name: "Silla ergonómica",
      company: "OfiMax",
      price: 199.9,
      description: "Cómoda",
      restrictionAge: 0,
    };

    renderForm({ product });

    expect(screen.getByRole("heading", { name: "Editar Producto" })).toBeInTheDocument();
    expect(getInput(/Nombre/)).toHaveValue("Silla ergonómica");
    expect(getInput(/Compañía/)).toHaveValue("OfiMax");
    expect(getInput(/Precio/)).toHaveValue(199.9);
    expect(screen.getByRole("link", { name: "← Ir al producto" })).toHaveAttribute(
      "href",
      "/products/7"
    );
  });

  it("Deshabilita el botón de guardar mientras isSubmitting es true", () => {
    renderForm({ isSubmitting: true });

    const button = screen.getByRole("button", { name: "Guardando..." });
    expect(button).toBeDisabled();
  });

  it("<uestra errores de validación y NO llama a onSubmit si el formulario está vacío", async () => {
    const user = userEvent.setup();
    const { onSubmit } = renderForm();

    await user.click(screen.getByRole("button", { name: "Guardar Producto" }));

    expect(await screen.findAllByText("Requerido")).toHaveLength(2);
    expect(screen.getByText("El precio es requerido")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("Envía el formulario con un FormData correcto cuando los datos son válidos", async () => {
    const user = userEvent.setup();
    const { onSubmit } = renderForm();

    await user.type(getInput(/Nombre/), "Teclado mecánico");
    await user.type(getInput(/Compañía/), "TechCorp");
    await user.type(getInput(/Precio/), "89.99");
    await user.type(getInput(/Descripción/), "RGB, switches azules");

    await user.click(screen.getByRole("button", { name: "Guardar Producto" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(1));

    const formData = onSubmit.mock.calls[0][0] as FormData;
    expect(formData.get("name")).toBe("Teclado mecánico");
    expect(formData.get("company")).toBe("TechCorp");
    expect(formData.get("price")).toBe("89.99");
    expect(formData.get("description")).toBe("RGB, switches azules");
    expect(formData.get("image")).toBeNull();
  });

  it("Rechaza un precio de 0 (mínimo 1)", async () => {
    const user = userEvent.setup();
    const { onSubmit } = renderForm();

    await user.type(getInput(/Nombre/), "Producto X");
    await user.type(getInput(/Compañía/), "Empresa X");
    await user.type(getInput(/Precio/), "0");

    await user.click(screen.getByRole("button", { name: "Guardar Producto" }));

    expect(await screen.findByText("El precio mínimo es 1")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("Envía removeImage=true cuando se elimina la imagen existente sin seleccionar una nueva", async () => {
    const user = userEvent.setup();
    const product: Product = { id: 3, name: "Silla", company: "OfiMax", price: 100, imageUrl: "http://x/img.png" };
    const { onSubmit } = renderForm({ product });

    await user.click(screen.getByRole("button", { name: "Eliminar imagen" }));
    await user.click(screen.getByRole("button", { name: "Guardar Producto" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(1));
    const formData = onSubmit.mock.calls[0][0] as FormData;
    expect(formData.get("removeImage")).toBe("true");
    expect(formData.get("image")).toBeNull();
  });
});