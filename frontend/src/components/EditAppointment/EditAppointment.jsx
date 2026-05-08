import { useState, useEffect } from "react";
import { appointmentsService } from "../../services/api";
import { sanitizeDogNameInput } from "../../utils/inputSanitize";
import { appointmentNumericId } from "../../utils/loyaltyPrice";
import {
  sameBookingSlot,
  validateAppointmentInput,
} from "../../utils/formValidation";
import { withSeconds } from "../../utils/dateTime";
import "./EditAppointment.css";

/** `datetime-local` value for the same instant as an API ISO string (local wall time). */
function isoToDatetimeLocalValue(isoOrLocal) {
  if (isoOrLocal == null || isoOrLocal === "") return "";
  const d = new Date(isoOrLocal);
  if (Number.isNaN(d.getTime())) return "";
  const pad = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export default function EditAppointment({ appointment, onSave, onCancel }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [date, setDate] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setDogName(sanitizeDogNameInput(appointment.dogName ?? ""));
    setDogSize(appointment.dogSize);
    setDate(isoToDatetimeLocalValue(appointment.date ?? appointment.Date));
  }, [appointment]);

  const validateInput = () => {
    const newErrors = validateAppointmentInput({
      dogName,
      dogSize,
      date,
      originalDate: appointment.date,
    });
    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const handleSave = async () => {
    if (!validateInput()) return;
    setLoading(true);

    try {
      const dateIso = withSeconds(date);
      const id = appointmentNumericId(appointment);
      if (id == null) {
        setErrors(["Invalid appointment id."]);
        setLoading(false);
        return;
      }
      const { ok, data } = await appointmentsService.update(
        id,
        dogName,
        dogSize,
        dateIso
      );

      if (ok) {
        setLoading(false);
        alert("Appointment updated 💾");
        onSave?.(data);
      } else {
        const errorMessage =
          data?.errors?.[0] ?? data?.message ?? "Failed to update appointment ❌";
        setErrors([errorMessage]);
        setLoading(false);
      }
    } catch (err) {
      setErrors(["Network error. Please try again."]);
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal">

        <h3>✏️ Edit Appointment</h3>

        {errors.length > 0 && (
          <div className="error-box">
            {errors.map((error, idx) => (
              <p key={idx} className="error-message">❌ {error}</p>
            ))}
          </div>
        )}

        <input
          value={dogName}
          onChange={(e) => setDogName(sanitizeDogNameInput(e.target.value))}
          placeholder="Dog name"
          inputMode="text"
          autoComplete="off"
          disabled={loading}
        />

        <select value={dogSize} onChange={(e) => setDogSize(e.target.value)} disabled={loading}>
          <option value="">Select size</option>
          <option value="small">Small</option>
          <option value="medium">Medium</option>
          <option value="large">Large</option>
        </select>

        <input
          type="datetime-local"
          value={date}
          onChange={(e) => setDate(e.target.value)}
          disabled={loading}
        />

        <div className="modal-buttons">
          <button onClick={handleSave} disabled={loading}>{loading ? "Saving..." : "Save 💾"}</button>
          <button onClick={onCancel} disabled={loading}>Cancel ❌</button>
        </div>

      </div>
    </div>
  );
}