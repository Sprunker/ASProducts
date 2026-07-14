import { z } from "zod";

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ACCEPTED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];

export const productSchema = z.object({
  name: z.string().min(1, "Requerido").max(50, "Máximo 50 caracteres"),
  description: z
    .string()
    .max(100, "Máximo 100 caracteres")
    .optional()
    .or(z.literal("")),
  restrictionAge: z.preprocess(
    (value) => (value === "" || value === null ? undefined : value),
    z.coerce.number().int().min(0, "Mínimo 0").max(100, "Máximo 100").optional()
  ),
  company: z.string().min(1, "Requerido").max(50, "Máximo 50 caracteres"),
  price: z.preprocess(
    (value) => (value === "" || value === null ? undefined : value),
    z.coerce
      .number({ message: "El precio es requerido" })
      .min(1, "El precio mínimo es 1")
      .max(1000, "El precio máximo es 1000")
      .multipleOf(0.01, "Máximo 2 decimales")
  ),
  image: z
    .instanceof(File)
    .refine((file) => file.size <= MAX_FILE_SIZE, "La imagen no debe superar 5MB")
    .refine(
      (file) => ACCEPTED_IMAGE_TYPES.includes(file.type),
      "Formato no soportado (usa JPG, PNG o WEBP)"
    )
    .nullable()
    .optional(),
  removeImage: z.boolean().optional().default(false),
});

export type ProductFormInput = z.input<typeof productSchema>;
export type ProductFormOutput = z.output<typeof productSchema>;