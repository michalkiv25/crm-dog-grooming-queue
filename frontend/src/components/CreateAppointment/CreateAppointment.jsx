import { useState } from "react";
import { appointmentsService } from "../../services/api";

export default function CreateAppointment({ onSuccess }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [appointmentDateTime, setAppointmentDateTime] = useState("");
  const [isPickerOpen, setIsPickerOpen] = useState(false);
  const [pickerDate, setPickerDate] = useState("");
  const [pickerTime, setPickerTime] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

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

    if (!appointmentDateTime) {
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
        appointmentDateTime
      );

      if (ok) {
        alert("Appointment created 🐶🎉");
        setDogName("");
        setDogSize("");
        setAppointmentDateTime("");
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

  const openDateTimePicker = () => {
    if (appointmentDateTime.includes("T")) {
      const [datePart, timePart] = appointmentDateTime.split("T");
      setPickerDate(datePart || "");
      setPickerTime((timePart || "").slice(0, 5));
    } else {
      setPickerDate("");
      setPickerTime("");
    }
    setIsPickerOpen(true);
  };

  const handleDateTimeTriggerPointerDown = (event) => {
    event.preventDefault();
    openDateTimePicker();
  };

  const handleDateTimeTriggerKeyDown = (event) => {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      openDateTimePicker();
    }
  };

  const applyPickerSelection = () => {
    if (!pickerDate || !pickerTime) {
      setErrors(["Please choose both date and time"]);
      return;
    }

    setAppointmentDateTime(`${pickerDate}T${pickerTime}`);
    setErrors([]);
    setIsPickerOpen(false);
  };

  const appointmentDisplayValue = appointmentDateTime
    ? new Date(appointmentDateTime).toLocaleString("he-IL", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      })
    : "";

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
        Appointment Date & Time
        <input
          className="date-time-trigger"
          type="text"
          value={appointmentDisplayValue}
          readOnly
          placeholder="dd.mm.yyyy, --:--"
          onClick={openDateTimePicker}
          onMouseDown={handleDateTimeTriggerPointerDown}
          onKeyDown={handleDateTimeTriggerKeyDown}
          role="button"
          tabIndex={0}
          disabled={loading}
        />
      </label>

      <button className="primary-button" onClick={createAppointment} disabled={loading}>
        {loading ? "Creating..." : "Create Appointment"}
      </button>

      {isPickerOpen && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Choose appointment time</h3>

            <label>
              Date
              <input
                type="date"
                value={pickerDate}
                onChange={(e) => setPickerDate(e.target.value)}
              />
            </label>

            <label>
              Time
              <input
                type="time"
                value={pickerTime}
                onChange={(e) => setPickerTime(e.target.value)}
              />
            </label>

            <div className="modal-buttons">
              <button type="button" onClick={() => setIsPickerOpen(false)}>
                Cancel
              </button>
              <button type="button" onClick={applyPickerSelection}>
                Apply
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}