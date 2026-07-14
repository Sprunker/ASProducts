interface ErrorMessageProps {
  message?: string;
}

export function ErrorMessage({ message = "Ocurrió un error al cargar la información." }: ErrorMessageProps) {
  return (
    <div className="alert alert-danger error-message fw-semibold" role="alert">
      ❌ {message}
    </div>
  );
}