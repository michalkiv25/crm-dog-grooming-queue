import { useState } from "react";

export default function CreateAppointment({ onSuccess }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [date, setDate] = useState("");

  const createAppointment = async () => {
    const token = localStorage.getItem("token");

    const response = await fetch("http://localhost:5285/api/appointments", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: token ? `Bearer ${token}` : ""
      },
      body: JSON.stringify({
        dogName,
        dogSize,
        date
      })
    });

    if (response.ok) {
      alert("Appointment created 🐶🎉");
      setDogName("");
      setDogSize("");
      setDate("");
      onSuccess?.();
    } else {
      alert("Failed ❌");
    }
  };

  return (
    <div className="auth-card">
      <h3>Create Appointment 🐶</h3>

      <label>
        Dog Name
        <input
          placeholder="Dog name"
          value={dogName}
          onChange={(e) => setDogName(e.target.value)}
        />
      </label>

      <label>
        Dog Size
        <select value={dogSize} onChange={(e) => setDogSize(e.target.value)}>
          <option value="">Select dog size</option>
          <option value="small">Small (30 min, ₪100)</option>
          <option value="medium">Medium (45 min, ₪150)</option>
          <option value="large">Large (60 min, ₪200)</option>
        </select>
      </label>

      <label>
        Appointment Date & Time
        <input
          type="datetime-local"
          value={date}
          onChange={(e) => setDate(e.target.value)}
        />
      </label>

      <button className="primary-button" onClick={createAppointment}>
        Create Appointment
      </button>
    </div>
  );
}