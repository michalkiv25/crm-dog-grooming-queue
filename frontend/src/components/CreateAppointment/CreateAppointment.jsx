import { useState } from "react";
import DatePicker, { registerLocale } from "react-datepicker";
import { enUS } from "date-fns/locale/en-US";
import "react-datepicker/dist/react-datepicker.css";
import { appointmentsService } from "../../services/api";
import { sanitizeDogNameInput } from "../../utils/inputSanitize";

registerLocale("enUS", enUS);

export default function CreateAppointment({ onSuccess }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [appointmentDateTime, setAppointmentDateTime] = useState("");
  const [isPickerOpen, setIsPickerOpen] = useState(false);
  const [modalSelected, setModalSelected] = useState(null);
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
    } else if (!/^[\p{L}\s'\-]+$/u.test(dogName.trim())) {
      newErrors.push("Dog name must contain letters only");
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
        setModalSelected(null);
        setErrors([]);
        onSuccess?.();
      } else {
        const raw = data?.errors?.[0] ?? data?.message ?? "";
        const errorMessage = raw || "Failed to create appointment ❌";
        setErrors([errorMessage]);
      }
    } catch (err) {
      setErrors(["Network error. Please try again."]);
    } finally {
      setLoading(false);
    }
  };

  const openDateTimePicker = () => {
    if (appointmentDateTime) {
      const d = new Date(appointmentDateTime);
      setModalSelected(Number.isNaN(d.getTime()) ? null : d);
    } else {
      setModalSelected(null);
    }
    setIsPickerOpen(true);
  };

  /** Hide past times and times already booked in this salon (same local minute). */
  const filterTime = (time) => {
    if (time.getTime() <= Date.now()) return false;
    return true;
  };

  const applyPickerSelection = async () => {
    if (!modalSelected) {
      setErrors(["Please choose a date and time"]);
      return;
    }
    if (modalSelected.getTime() <= Date.now()) {
      setErrors(["Please choose a future date and time"]);
      return;
    }
    const iso = modalSelected.toISOString();
    setAppointmentDateTime(iso);
    setErrors([]);
    setIsPickerOpen(false);
  };

  const appointmentDisplayValue = appointmentDateTime
    ? new Date(appointmentDateTime).toLocaleString("en-US", {
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
      <p className="loyalty-discount-note" dir="ltr">
        First three saved appointments are full list price; from your <strong>fourth</strong> saved appointment
        onward you get <strong>10% off</strong> that size’s list price (fifth, sixth, … as well). If you cancel
        until you have fewer than four in total, all remaining ones return to full price. Order is by
        <strong> scheduled date and time</strong> (earliest first). 
      </p>

      {errors.length > 0 && (
        <div className="error-box">
          {errors.map((error, idx) => (
            <p key={idx} className="error-message">
              ❌ {error}
            </p>
          ))}
        </div>
      )}

      <label>
        Dog Name
        <input
          placeholder="Dog name"
          inputMode="text"
          autoComplete="off"
          value={dogName}
          onChange={(e) => setDogName(sanitizeDogNameInput(e.target.value))}
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
          placeholder="Click to open calendar"
          onClick={openDateTimePicker}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              e.preventDefault();
              openDateTimePicker();
            }
          }}
          role="button"
          tabIndex={0}
          disabled={loading}
        />
      </label>

      <button className="primary-button" onClick={createAppointment} disabled={loading}>
        {loading ? "Creating..." : "Create Appointment"}
      </button>

      {isPickerOpen && (
        <div
          className="modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="appointment-picker-title"
        >
          <div className="modal modal--datepicker appointment-modal">
            <h3 id="appointment-picker-title">Choose date and time</h3>
            <p className="picker-hint" dir="rtl" style={{ fontSize: "0.85rem", color: "#555", marginTop: 0 }}>
              אפשר לבחור כל שעה פנויה בעתיד.
            </p>

            <div className="appointment-datepicker-wrap">
              <DatePicker
                inline
                selected={modalSelected}
                onChange={(date) => setModalSelected(date)}
                showTimeSelect
                timeIntervals={15}
                timeCaption="Time"
                dateFormat="Pp"
                locale="enUS"
                minDate={new Date()}
                filterTime={filterTime}
                calendarClassName="appointment-calendar-inner"
              />
            </div>

            <div className="modal-buttons">
              <button type="button" onClick={() => setIsPickerOpen(false)}>
                Cancel
              </button>
              <button type="button" className="primary-button" onClick={applyPickerSelection}>
                Apply
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
