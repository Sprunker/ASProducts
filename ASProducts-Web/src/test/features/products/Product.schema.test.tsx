import { describe, it, expect } from "vitest";
import { productSchema } from "@/features/products/product.schema";

const validPayload = (overrides: Record<string, unknown> = {}) => ({
  name: "Producto de prueba",
  description: "Una descripción cualquiera",
  restrictionAge: "18",
  company: "ACME",
  price: "99.99",
  image: null,
  ...overrides,
});

describe("productSchema", () => {
  it("Acepta un payload válido y coacciona tipos (string -> number)", () => {
    const result = productSchema.safeParse(validPayload());

    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.price).toBe(99.99);
      expect(result.data.restrictionAge).toBe(18);
    }
  });

  it("Rechaza cuando 'name' está vacío", () => {
    const result = productSchema.safeParse(validPayload({ name: "" }));

    expect(result.success).toBe(false);
    if (!result.success) {
      const nameIssue = result.error.issues.find((i) => i.path[0] === "name");
      expect(nameIssue?.message).toBe("Requerido");
    }
  });

  it("Rechaza cuando 'price' está vacío con el mensaje de requerido", () => {
    const result = productSchema.safeParse(validPayload({ price: "" }));

    expect(result.success).toBe(false);
    if (!result.success) {
      const priceIssue = result.error.issues.find((i) => i.path[0] === "price");
      expect(priceIssue?.message).toBe("El precio es requerido");
    }
  });

  it("Rechaza 'price' fuera de rango (mayor a 1000)", () => {
    const result = productSchema.safeParse(validPayload({ price: "1500" }));

    expect(result.success).toBe(false);
    if (!result.success) {
      const priceIssue = result.error.issues.find((i) => i.path[0] === "price");
      expect(priceIssue?.message).toBe("El precio máximo es 1000");
    }
  });

  it("Trata restrictionAge vacío ('') como opcional (undefined), no como error", () => {
    const result = productSchema.safeParse(validPayload({ restrictionAge: "" }));

    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.restrictionAge).toBeUndefined();
    }
  });

  it("Rechaza una imagen que supera los 5MB", () => {
    const bigFile = new File([new Uint8Array(6 * 1024 * 1024)], "big.png", {
      type: "image/png",
    });

    const result = productSchema.safeParse(validPayload({ image: bigFile }));

    expect(result.success).toBe(false);
    if (!result.success) {
      const imageIssue = result.error.issues.find((i) => i.path[0] === "image");
      expect(imageIssue?.message).toBe("La imagen no debe superar 5MB");
    }
  });

  it("Rechaza una imagen con formato no soportado", () => {
    const gifFile = new File(["gif-bytes"], "anim.gif", { type: "image/gif" });

    const result = productSchema.safeParse(validPayload({ image: gifFile }));

    expect(result.success).toBe(false);
    if (!result.success) {
      const imageIssue = result.error.issues.find((i) => i.path[0] === "image");
      expect(imageIssue?.message).toBe("Formato no soportado (usa JPG, PNG o WEBP)");
    }
  });
});