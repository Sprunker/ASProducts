import { useEffect, useRef, useState } from "react";

interface ImageUploaderProps {
  label?: string;
  existingImageUrl?: string;
  onChange: (file: File | null) => void;
  onRemoveExisting?: () => void;
  error?: string;
}

export function ImageUploader({
  label = "Imagen",
  existingImageUrl,
  onChange,
  onRemoveExisting,
  error: externalError,
}: ImageUploaderProps) {
  const [preview, setPreview] = useState<string | undefined>(existingImageUrl);
  const [isDragging, setIsDragging] = useState(false);
  const [internalError, setInternalError] = useState<string | undefined>(undefined);
  const inputRef = useRef<HTMLInputElement>(null);

  const activeError = externalError || internalError;

  useEffect(() => {
    setPreview(existingImageUrl);
  }, [existingImageUrl]);

  useEffect(() => {
    return () => {
      if (preview?.startsWith("blob:")) URL.revokeObjectURL(preview);
    };
  }, [preview]);

  const validateFile = (file: File | null) => {
    if (!file) return true;

    const isImage = file.type.startsWith("image/");
    const isSizeValid = file.size < 5 * 1024 * 1024;

    if (!isImage) {
      setInternalError("El archivo no es una imagen válida.");
      return false;
    }
    if (!isSizeValid) {
      setInternalError("La imagen debe pesar menos de 5MB.");
      return false;
    }

    setInternalError(undefined);
    return true;
  };

  const applyFile = (file: File | null) => {
    const isValid = validateFile(file);

    if (file && isValid) {
      const objectUrl = URL.createObjectURL(file);
      setPreview(objectUrl);
      onChange(file);
    } else if (!file) {
      setPreview(existingImageUrl);
      setInternalError(undefined);
      onChange(null);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    applyFile(e.target.files?.[0] ?? null);
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);
    const file = e.dataTransfer.files?.[0];
    if (!file) return;

    const isValid = validateFile(file);
    applyFile(isValid ? file : file);

    if (inputRef.current && isValid) {
      const dataTransfer = new DataTransfer();
      dataTransfer.items.add(file);
      inputRef.current.files = dataTransfer.files;
    }
  };

  const handleRemove = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.stopPropagation();
    setPreview(undefined);
    onChange(null);
    onRemoveExisting?.();
  };

  const showRemoveButton = !!preview;

  return (
    <div className="image-uploader position-relative w-100 h-100">
      {label && <label className="form-label">{label}</label>}

      <div
        className={`image-uploader__dropzone position-relative ${isDragging ? "is-dragging" : ""} ${
          activeError ? "is-invalid" : ""
        }`}
        role="button"
        tabIndex={0}
        onClick={() => inputRef.current?.click()}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") inputRef.current?.click();
        }}
        onDragOver={(e) => {
          e.preventDefault();
          setIsDragging(true);
        }}
        onDragLeave={() => setIsDragging(false)}
        onDrop={handleDrop}
      >
        {preview ? (
          <img src={preview} alt="Vista previa" className="image-uploader__preview-img" />
        ) : (
          <div className="image-uploader__placeholder">
            <span className="image-uploader__title">Imagen</span>
            <span className="image-uploader__hint">Arrastra y suelta</span>
            <span className="image-uploader__hint">o selecciona un archivo...</span>
          </div>
        )}

        {showRemoveButton && (
          <button
            type="button"
            className="btn-xs btn-danger position-absolute top-0 end-0 m-2"
            aria-label="Eliminar imagen"
            onClick={handleRemove}
          >
            ❌
          </button>
        )}
      </div>

      <input
        ref={inputRef}
        type="file"
        accept="image/png, image/jpeg, image/webp"
        className="image-uploader__input"
        onChange={handleFileChange}
      />
      {activeError && <div className="invalid-feedback d-block">{activeError}</div>}
    </div>
  );
}