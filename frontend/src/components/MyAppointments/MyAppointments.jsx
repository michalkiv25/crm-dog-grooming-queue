import { useEffect, useState } from "react";
import EditAppointment from "../EditAppointment/EditAppointment";

export default function MyAppointments() {
  const [appointments, setAppointments] = useState([]);
  const [editing, setEditing] = useState(null);

  // 📥 טעינת תורים
  useEffect(() => {
    const token = localStorage.getItem("token");

    if (!token) {
      console.log("NO TOKEN FOUND");
      return;
    }

    const loadAppointments = () => {
      fetch("http://localhost:5285/api/appointments", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      })
        .then((res) => {
          if (!res.ok) throw new Error("Unauthorized");
          return res.json();
        })
        .then((data) => setAppointments(data))
        .catch((err) => console.log(err));
    };

    loadAppointments();
  }, []);

  // 🗑️ מחיקה
  const deleteAppointment = async (id) => {
    const token = localStorage.getItem("token");

    const res = await fetch(
      `http://localhost:5285/api/appointments/${id}`,
      {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    if (res.ok) {
      setAppointments((prev) =>
        prev.filter((a) => a.id !== id)
      );
    } else {
      alert("Failed to delete ❌");
    }
  };

  // ✏️ שמירת עריכה
  const saveEdit = async (id, dogName, date) => {
    const token = localStorage.getItem("token");

    const res = await fetch(
      `http://localhost:5285/api/appointments/${id}`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          dogName,
          date,
        }),
      }
    );

    if (res.ok) {
      const updated = await res.json();

      setAppointments((prev) =>
        prev.map((a) =>
          a.id === id ? updated : a
        )
      );

      setEditing(null);
    } else {
      alert("Failed to update ❌");
    }
  };

  return (
    <div>
      <h2>🐶 My Appointments</h2>

      <div className="cards-container">
        {appointments.length === 0 && (
          <p className="empty">
            No appointments yet 🐶
          </p>
        )}

        {appointments.map((a) => (
          <div key={a.id} className="card">
            {editing === a.id ? (
              <EditAppointment
                appointment={a}
                onSave={saveEdit}
                onCancel={() => setEditing(null)}
              />
            ) : (
              <>
                <h3>🐾 Dog: {a.dogName}</h3>
                <p>📅 Date: {a.date}</p>

                <button onClick={() => setEditing(a.id)}>
                  Edit ✏️
                </button>

                <button onClick={() => deleteAppointment(a.id)}>
                  Delete ❌
                </button>
              </>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}