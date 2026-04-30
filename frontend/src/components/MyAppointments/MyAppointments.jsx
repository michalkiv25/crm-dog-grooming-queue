import { useEffect, useState } from "react";

export default function MyAppointments() {
  const [appointments, setAppointments] = useState([]);

  useEffect(() => {
    const token = localStorage.getItem("token");

    fetch("http://localhost:5285/api/appointments", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
      .then((res) => res.json())
      .then((data) => setAppointments(data));
  }, []);

  const deleteAppointment = async (id) => {
  const token = localStorage.getItem("token");

  const res = await fetch(`http://localhost:5285/api/appointments/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (res.ok) {
    setAppointments((prev) =>
      prev.filter((a) => a.id !== id)
    );
  } else {
    alert("Failed to delete ❌");
  }
};

  return (
    <div>
      <h2>🐶 My Appointments</h2>

      <div className="cards-container">
        {appointments.length === 0 && <p>No appointments yet</p>}

        {appointments.map((a) => (
          <div key={a.id} className="card">
            <h3>🐾 Dog: {a.dogName}</h3>
            <p>📅 Date: {a.date}</p>

            <button onClick={() => deleteAppointment(a.id)}>
              Delete ❌
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}