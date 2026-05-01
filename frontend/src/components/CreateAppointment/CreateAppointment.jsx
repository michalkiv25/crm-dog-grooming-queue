import { useState } from "react";
import { appointmentsService } from "../../services/api";

export default function CreateAppointment({ onSuccess }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [appointmentDate, setAppointmentDate] = useState("");
  const [appointmentTime, setAppointmentTime] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  const buildAppointmentDateTime = () => {
    if (!appointmentDate || !appointmentTime) {
      return "";
    }

    return `${appointmentDate}T${appointmentTime}`;
  };

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

    const appointmentDateTime = buildAppointmentDateTime();

    if (!appointmentDate || !appointmentTime) {
      newErrors.push("Appointment date is required");
    } else if (new Date(appointmentDateTime) <= new Date()) {
      newErrors.push("Appointment date must be in the future");
    }

    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const createAppointment = async () => {
    if (!validateInput()) return;

    setLoading(true);
    try {
      const { ok, data } = await appointmentsService.create(
        dogName,
        dogSize,
        buildAppointmentDateTime()
      );

      if (ok) {
        alert("Appointment created 🐶🎉");
        setDogName("");
        setDogSize("");
        setAppointmentDate("");
        setAppointmentTime("");
        setErrors([]);
        onSuccess?.();
      } else {
        const errorMessage = data?.errors?.length 
          ? data.errors[0] 
          : data?.message || "Failed to create appointment ❌";
        setErrors([errorMessage]);
      }
    } catch (err) {
      setErrors(["Network error. Please try again."]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-card">
      <h3>Create Appointment 🐶</h3>

      {errors.length > 0 && (
        <div className="error-box">
          {errors.map((error, idx) => (
            <p key={idx} className="error-message">❌ {error}</p>
          ))}
        </div>
      )}

      <label>
        Dog Name
        <input
          placeholder="Dog name"
          value={dogName}
          onChange={(e) => setDogName(e.target.value)}
          disabled={loading}
        />
      </label>

      <label>
        Dog Size
        <select value={dogSize} onChange={(e) => setDogSize(e.target.value)} disabled={loading}>
          <option value="">Select dog size</option>
          <option value="small">Small (30 min, ₪100)</option>
          <option value="medium">Medium (45 min, ₪150)</option>
          <option value="large">Large (60 min, ₪200)</option>
        </select>
      </label>

      <label>
        Appointment Date
        <input
          type="date"
          value={appointmentDate}
          onChange={(e) => setAppointmentDate(e.target.value)}
          disabled={loading}
        />
      </label>

      <label>
        Appointment Time
        <input
          type="time"
          value={appointmentTime}
          onChange={(e) => setAppointmentTime(e.target.value)}
          disabled={loading}
        />
      </label>

      <button className="primary-button" onClick={createAppointment} disabled={loading}>
        {loading ? "Creating..." : "Create Appointment"}
      </button>
    </div>
  );
}