import { useEffect, useRef } from "react";
import { Modal } from "bootstrap";

interface ConfirmModalProps {
  show: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmModal({
  show,
  title,
  message,
  confirmLabel = "Eliminar",
  onConfirm,
  onCancel,
}: ConfirmModalProps) {
  const modalRef = useRef<HTMLDivElement>(null);
  const instanceRef = useRef<Modal | null>(null);

  useEffect(() => {
    if (modalRef.current) {
      instanceRef.current = new Modal(modalRef.current, { backdrop: "static" });
      modalRef.current.addEventListener("hidden.bs.modal", onCancel);
    }
    return () => {
      instanceRef.current?.dispose();
    };
  }, []);

  useEffect(() => {
    if (show) instanceRef.current?.show();
    else instanceRef.current?.hide();
  }, [show]);

  return (
    <div className="modal fade confirm-modal" ref={modalRef} tabIndex={-1}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header position-relative">
            <h5 className="modal-title">{title}</h5>
            <button
              type="button"
              className="btn-xs btn-danger position-absolute top-0 end-0 m-2"
              onClick={onCancel}
            >
              ❌
            </button>
          </div>
          <div className="modal-body">
            <p className="mb-0">{message}</p>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn-sm btn-secondary" onClick={onCancel}>
              Cancelar
            </button>
            <button type="button" className="btn-sm btn-danger" onClick={onConfirm}>
              {confirmLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}