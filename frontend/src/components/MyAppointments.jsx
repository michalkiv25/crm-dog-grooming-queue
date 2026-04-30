import { useEffect, useState } from "react";

export default function MyAppointments() {
  const [appointments, setAppointments] = useState([]);

  const getAppointments = async () => {
    const token = localStorage.getItem("token");

    const response = await fetch("http://localhost:5285/api/appointments", {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });

    const data = await response.json();
    console.log("APPOINTMENTS:", data);

    setAppointments(data);
  };

  useEffect(() => {
    getAppointments();
  }, []);

  return (
    <div style={{ marginTop: 20 }}>
      <h3>My Appointments 🐶</h3>

      {appointments.length === 0 ? (
        <p>No appointments yet</p>
      ) : (
        <ul>
          {appointments.map((a) => (
            <li key={a.id}>
              🐕 {a.dogName} | 📅 {new Date(a.date).toLocaleDateString()}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}