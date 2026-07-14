import { Outlet } from "react-router-dom";
import { Navbar } from "@/components/common/Navbar";

export function BaseLayout() {
  return (
    <>
      <Navbar />
      <main className="container main-content">
        <Outlet />
      </main>
    </>
  );
}