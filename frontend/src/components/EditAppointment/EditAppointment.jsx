import { useState, useEffect } from "react";
import { appointmentsService } from "../../services/api";

export default function EditAppointment({ appointment, onSave, onCancel }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [date, setDate] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setDogName(appointment.dogName);
    setDogSize(appointment.dogSize);
    setDate(appointment.date);
  }, [appointment]);

  const validateInput = () => {
    const newErrors = [];

    if (!dogName.trim()) {
      newErrors.push("Dog name is required");
    } else if (dogName.trim().length < 2) {
      newErrors.push("Dog name must be at least 2 characters");
    } else if (dogName.trim().length > 50) {
      newErrors.push("Dog name must not exceed 50 characters");
    }

    if (!dogSize) {
      newErrors.push("Dog size is required");
    } else if (!["small", "medium", "large"].includes(dogSize)) {
      newErrors.push("Invalid dog size selected");
    }

    if (!date) {
      newErrors.push("Appointment date is required");
    } else if (new Date(date) <= new Date()) {
      newErrors.push("Appointment date must be in the future");
    }

    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const handleSave = async () => {
    if (!validateInput()) return;
    setLoading(true);

    try {
      const { ok, data } = await appointmentsService.update(appointment.id, dogName, dogSize, date);

      if (ok) {
        alert("Appointment updated 💾");
        onSave?.(appointment.id, dogName, dogSize, date);
      } else {
        const errorMessage = data?.errors?.length 
          ? data.errors[0] 
          : data?.message || "Failed to update appointment ❌";
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
          onChange={(e) => setDogName(e.target.value)}
          placeholder="Dog name"
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