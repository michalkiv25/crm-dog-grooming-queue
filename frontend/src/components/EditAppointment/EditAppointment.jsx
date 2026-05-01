import { useState, useEffect } from "react";

export default function EditAppointment({ appointment, onSave, onCancel }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [date, setDate] = useState("");

  useEffect(() => {
    setDogName(appointment.dogName);
    setDogSize(appointment.dogSize);
    setDate(appointment.date);
  }, [appointment]);

  const handleSave = () => {
    onSave(appointment.id, dogName, dogSize, date);
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

        <select value={dogSize} onChange={(e) => setDogSize(e.target.value)}>
          <option value="">Select size</option>
          <option value="small">Small</option>
          <option value="medium">Medium</option>
          <option value="large">Large</option>
        </select>

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