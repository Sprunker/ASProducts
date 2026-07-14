import type { Product } from "./product.types";

const initialProducts: Product[] = [
  // {
  //   id: 1,
  //   name: "Auriculares inalámbricos",
  //   description: "Sonido claro, diseño cómodo y batería para todo el día.",
  //   restrictionAge: 0,
  //   company: "AudioPlus",
  //   price: 129.99,
  // },
  // {
  //   id: 2,
  //   name: "Kit de cuidado facial",
  //   description: "Rutina completa para una piel fresca y suave.",
  //   restrictionAge: 0,
  //   company: "BellezaNatural",
  //   price: 49.9,
  // },
  // {
  //   id: 3,
  //   name: "Juego de mesa familiar",
  //   description: "Diversión para todas las edades en cada partida.",
  //   restrictionAge: 8,
  //   company: "MesaMágica",
  //   price: 39.5,
  // },
  // {
  //   id: 4,
  //   name: "Lámpara de lectura LED",
  //   description: "Iluminación ajustable con luz cálida y fría.",
  //   restrictionAge: 0,
  //   company: "HogarLuz",
  //   price: 24.75,
  // },
  // {
  //   id: 5,
  //   name: "Mochila urbana",
  //   description: "Resistente al agua y con compartimentos para laptop.",
  //   restrictionAge: 0,
  //   company: "CityGear",
  //   price: 59.0,
  // },
];

let products = [...initialProducts];
let nextProductId = products.length + 1;

function parseFormData(formData: FormData): Omit<Product, "id" | "imageUrl"> {
  const name = formData.get("name") as string;
  const description = (formData.get("description") as string) || undefined;
  const restrictionAgeRaw = formData.get("restrictionAge");
  const company = formData.get("company") as string;
  const priceRaw = formData.get("price") as string;

  const restrictionAge =
    restrictionAgeRaw === null || restrictionAgeRaw === ""
      ? undefined
      : Number(restrictionAgeRaw);

  const hasNewImage = formData.has("image");
  const removeImage = formData.get("removeImage") === "true";

  return {
    name,
    description: description === "" ? undefined : description,
    restrictionAge: Number.isNaN(restrictionAge) ? undefined : restrictionAge,
    company,
    price: Number(priceRaw),
  };
}

export const productMock = {
  getAll: async (): Promise<Product[]> => {
    return [...products];
  },

  getById: async (id: number): Promise<Product> => {
    const product = products.find((item) => item.id === id);
    if (!product) {
      throw new Error("Producto no encontrado");
    }
    return product;
  },

  create: async (formData: FormData): Promise<Product> => {
    const payload = parseFormData(formData);
    const newProduct: Product = {
      id: nextProductId++,
      ...payload,
    };
    products = [...products, newProduct];
    return newProduct;
  },

  update: async (id: number, formData: FormData): Promise<Product> => {
    const payload = parseFormData(formData);
    const productIndex = products.findIndex((item) => item.id === id);
    if (productIndex === -1) throw new Error("Producto no encontrado");

    const current = products[productIndex];
    const hasNewImage = formData.has("image");
    const removeImage = formData.get("removeImage") === "true";

    const imageUrl = hasNewImage
      ? current.imageUrl
      : removeImage
        ? undefined
        : current.imageUrl;

    const updatedProduct: Product = { id, ...payload, imageUrl };
    products = products.map((item) => (item.id === id ? updatedProduct : item));
    return updatedProduct;
  },

  remove: async (id: number): Promise<void> => {
    products = products.filter((item) => item.id !== id);
  },

  reset: (): void => {
    products = [...initialProducts];
    nextProductId = products.length + 1;
  },
};
