import { useState } from "react";
import DatePicker, { registerLocale } from "react-datepicker";
import { format } from "date-fns";
import { he } from "date-fns/locale";
import "react-datepicker/dist/react-datepicker.css";
import { appointmentsService } from "../../services/api";

registerLocale("he", he);

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
    if (appointmentDateTime) {
      const d = new Date(appointmentDateTime);
      setModalSelected(Number.isNaN(d.getTime()) ? null : d);
    } else {
      setModalSelected(null);
    }
    setIsPickerOpen(true);
  };

  /** Hide times earlier than “now” when the chosen day is today */
  const filterPassedTime = (time) => time.getTime() > Date.now();

  const applyPickerSelection = () => {
    if (!modalSelected) {
      setErrors(["נא לבחור תאריך ושעה"]);
      return;
    }
    if (modalSelected.getTime() <= Date.now()) {
      setErrors(["נא לבחור תאריך ושעה בעתיד"]);
      return;
    }
    setAppointmentDateTime(format(modalSelected, "yyyy-MM-dd'T'HH:mm:ss"));
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
          placeholder="לחצו לפתיחת יומן"
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
          <div className="modal modal--datepicker appointment-modal" dir="rtl">
            <h3 id="appointment-picker-title">בחרו תאריך ושעה</h3>

            <div className="appointment-datepicker-wrap">
              <DatePicker
                inline
                selected={modalSelected}
                onChange={(date) => setModalSelected(date)}
                showTimeSelect
                timeIntervals={15}
                timeCaption="שעה"
                dateFormat="Pp"
                locale="he"
                minDate={new Date()}
                filterTime={filterPassedTime}
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
