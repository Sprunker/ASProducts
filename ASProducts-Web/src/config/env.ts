import { z } from "zod";

const envSchema = z.object({
  VITE_USE_MOCK: z.string().transform((v) => v === "true"),
  VITE_API_BASE_URL: z.url("VITE_API_BASE_URL must be a valid URL"),
});

const parsed = envSchema.safeParse(import.meta.env);

if (!parsed.success) {
  const tree = z.treeifyError(parsed.error);
  
  console.error("Invalid environment variables:", tree);
  throw new Error("Invalid environment configuration. Please check your .env file");
}

export const env = parsed.data;