import { useState, useEffect } from "react";

export default function EditAppointment({ appointment, onSave, onCancel }) {
  const [dogName, setDogName] = useState("");
  const [date, setDate] = useState("");

  useEffect(() => {
    setDogName(appointment.dogName);
    setDate(appointment.date);
  }, [appointment]);

  const handleSave = () => {
    onSave(appointment.id, dogName, date);
  };

  return (
    <div className="modal-overlay">
      <div className="modal">

        <h3>✏️ Edit Appointment</h3>

        <input
          value={dogName}
          onChange={(e) => setDogName(e.target.value)}
          placeholder="Dog name"
        />

        <input
          type="datetime-local"
          value={date}
          onChange={(e) => setDate(e.target.value)}
        />

        <div className="modal-buttons">
          <button onClick={handleSave}>Save 💾</button>
          <button onClick={onCancel}>Cancel ❌</button>
        </div>

      </div>
    </div>
  );
}