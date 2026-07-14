import axios from "axios";
import { env } from "@/config/env"

export const httpClient = axios.create({
  baseURL: env.VITE_API_BASE_URL,
});