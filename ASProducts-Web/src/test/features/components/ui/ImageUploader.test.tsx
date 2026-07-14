import { describe, it, expect, vi, beforeAll, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ImageUploader } from "@/components/ui/ImageUploader";

beforeAll(() => {
  URL.createObjectURL = vi.fn(() => "blob:mock-url");
  URL.revokeObjectURL = vi.fn();
});

const buildFile = (name: string, type: string, sizeBytes = 1024) =>
  new File([new Uint8Array(sizeBytes)], name, { type });

describe("ImageUploader", () => {
  let onChange: ReturnType<typeof vi.fn<(file: File | null) => void>>;

  beforeEach(() => {
    onChange = vi.fn();
  });

  it("Muestra el placeholder cuando no hay imagen", () => {
    render(<ImageUploader onChange={onChange} />);

    expect(screen.getByText("Arrastra y suelta")).toBeInTheDocument();
    expect(screen.queryByAltText("Vista previa")).not.toBeInTheDocument();
  });

  it("Acepta un archivo de imagen válido y notifica el cambio (onChange)", async () => {
    const user = userEvent.setup();
    render(<ImageUploader onChange={onChange} />);

    const file = buildFile("foto.png", "image/png");

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;

    await user.upload(input, file);

    expect(onChange).toHaveBeenCalledWith(file);
    expect(screen.getByAltText("Vista previa")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Eliminar imagen" })).toBeInTheDocument();
  });

  it("Rechaza un archivo que no es imagen y muestra un error accesible, sin llamar onChange con el archivo", async () => {
    const user = userEvent.setup({ applyAccept: false });
    render(<ImageUploader onChange={onChange} />);

    const file = buildFile("documento.pdf", "application/pdf");
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;

    await user.upload(input, file);

    expect(screen.getByText("El archivo no es una imagen válida.")).toBeInTheDocument();
    expect(onChange).not.toHaveBeenCalledWith(file);
    expect(screen.queryByAltText("Vista previa")).not.toBeInTheDocument();
  });

  it("Rechaza un archivo de imagen que supera 5MB", async () => {
    const user = userEvent.setup();
    render(<ImageUploader onChange={onChange} />);

    const bigFile = buildFile("grande.png", "image/png", 6 * 1024 * 1024);
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;

    await user.upload(input, bigFile);

    expect(screen.getByText("La imagen debe pesar menos de 5MB.")).toBeInTheDocument();
    expect(onChange).not.toHaveBeenCalledWith(bigFile);
  });

  it("Es operable por teclado: el dropzone tiene role=button y responde a Enter", async () => {
    const user = userEvent.setup();
    render(<ImageUploader onChange={onChange} />);

    const dropzone = document.querySelector(".image-uploader__dropzone") as HTMLElement;

    expect(dropzone).toHaveAttribute("tabIndex", "0");
    expect(dropzone).toHaveAttribute("role", "button");

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    const clickSpy = vi.spyOn(input, "click");

    dropzone.focus();
    await user.keyboard("{Enter}");

    expect(clickSpy).toHaveBeenCalled();
  });

  it("Permite quitar la imagen seleccionada mediante el botón 'Eliminar imagen'", async () => {
    const user = userEvent.setup();
    render(<ImageUploader onChange={onChange} />);

    const file = buildFile("foto.png", "image/png");
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await user.upload(input, file);

    const removeButton = screen.getByRole("button", { name: "Eliminar imagen" });
    await user.click(removeButton);

    expect(onChange).toHaveBeenLastCalledWith(null);
    expect(screen.queryByAltText("Vista previa")).not.toBeInTheDocument();
  });
});