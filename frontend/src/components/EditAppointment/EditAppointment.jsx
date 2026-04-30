import { useState } from "react";

export default function EditAppointment({ appointment, onSave, onCancel }) {
  const [dogName, setDogName] = useState(appointment.dogName);
  const [date, setDate] = useState(appointment.date);

  const handleSave = () => {
    onSave(appointment.id, dogName, date);
  };

  return (
    <div>
      <h4>Edit Appointment ✏️</h4>

      <input
        value={dogName}
        onChange={(e) => setDogName(e.target.value)}
        placeholder="Dog name"
      />

      <input
        value={date}
        onChange={(e) => setDate(e.target.value)}
        placeholder="Date"
      />

      <button onClick={handleSave}>Save 💾</button>
      <button onClick={onCancel}>Cancel ❌</button>
    </div>
  );
}