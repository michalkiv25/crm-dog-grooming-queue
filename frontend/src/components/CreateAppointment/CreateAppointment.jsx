import { useState } from "react";

export default function CreateAppointment() {
  const [dogName, setDogName] = useState("");
  const [date, setDate] = useState("");

  const createAppointment = async () => {
    const token = localStorage.getItem("token");

    const response = await fetch("http://localhost:5285/api/appointments", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify({
        dogName,
        date
      })
    });

    if (response.ok) {
      alert("Appointment created 🐶🎉");
      setDogName("");
      setDate("");
    } else {
      alert("Failed ❌");
    }
  };

  return (
    <div style={{ border: "1px solid #ccc", padding: 20, marginTop: 20 }}>
      <h3>Create Appointment 🐶</h3>

      <input
        placeholder="Dog name"
        value={dogName}
        onChange={(e) => setDogName(e.target.value)}
      />

      <br /><br />

      <input
        type="date"
        value={date}
        onChange={(e) => setDate(e.target.value)}
      />

      <br /><br />

      <button onClick={createAppointment}>
        Create
      </button>
    </div>
  );
}