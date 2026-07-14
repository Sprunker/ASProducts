import { env } from "@/config/env"

export interface Product {
  id: number;
  name: string;
  description?: string;
  restrictionAge?: number;
  company: string;
  price: number;
  imageUrl?: string;
  _imageVersion?: number;
}

export function getProductImageUrl(product: Product): string {
  if (!product.imageUrl) return "/placeholder-product.jpg";

  const cacheBuster = product._imageVersion ? `?v=${product._imageVersion}` : "";

  return `${env.VITE_API_BASE_URL}${product.imageUrl}${cacheBuster}`;
}