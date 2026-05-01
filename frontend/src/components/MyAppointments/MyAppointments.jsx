import { useEffect, useState } from "react";
import EditAppointment from "../EditAppointment/EditAppointment";

export default function MyAppointments() {
  const [appointments, setAppointments] = useState([]);
  const [editing, setEditing] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem("token");

    console.log("TOKEN:", token);

    if (!token) {
      console.log("NO TOKEN FOUND");
      setLoading(false);
      return;
    }

    const loadAppointments = async () => {
      try {
        const res = await fetch("http://localhost:5285/api/appointments", {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        console.log("STATUS:", res.status);

        if (!res.ok) {
          const text = await res.text();
          throw new Error(text);
        }

        const data = await res.json();
        setAppointments(data);
      } catch (err) {
        console.log("ERROR:", err);
      } finally {
        setLoading(false);
      }
    };

    loadAppointments();
  }, []);

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

  if (loading) {
    return <h3>Loading appointments...</h3>;
  }

  return (
    <div>
      <h2>🐶 My Appointments</h2>

      {appointments.length === 0 && (
        <p className="empty">
          No appointments yet 🐶
        </p>
      )}

      <div className="cards-container">
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