import { useEffect, useState } from "react";
import EditAppointment from "../EditAppointment/EditAppointment";

export default function MyAppointments() {
  const [appointments, setAppointments] = useState([]);
  const [editing, setEditing] = useState(null);
  const [loading, setLoading] = useState(true);
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  const [filterDate, setFilterDate] = useState("");
  const [filterCustomer, setFilterCustomer] = useState("");

  useEffect(() => {
    loadAppointments();
  }, [filterDate, filterCustomer]);

  const loadAppointments = async () => {
    const token = localStorage.getItem("token");

    if (!token) {
      setLoading(false);
      return;
    }

    let url = "http://localhost:5285/api/appointments";
    if (filterDate || filterCustomer) {
      url += "/filter?";
      if (filterDate) url += `date=${filterDate}&`;
      if (filterCustomer) url += `customerName=${filterCustomer}`;
    }

    try {
      const res = await fetch(url, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

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

  const saveEdit = async (id, dogName, dogSize, date) => {
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
          dogSize,
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

      <div className="filters">
        <input
          type="date"
          value={filterDate}
          onChange={(e) => setFilterDate(e.target.value)}
          placeholder="Filter by date"
        />
        <input
          type="text"
          value={filterCustomer}
          onChange={(e) => setFilterCustomer(e.target.value)}
          placeholder="Filter by customer name"
        />
        <button onClick={() => { setFilterDate(""); setFilterCustomer(""); }}>Clear Filters</button>
      </div>

      {appointments.length === 0 && (
        <p className="empty">
          No appointments yet 🐶
        </p>
      )}

      <div className="cards-container">
        {appointments.map((a) => (
          <div key={a.id} className="card" onClick={() => setSelectedAppointment(a)}>
            <h3>👤 Customer: {a.username}</h3>
            <p>🐾 Dog: {a.dogName}</p>
            <p>📏 Size: {a.dogSize}</p>
            <p>📅 Date: {new Date(a.date).toLocaleString()}</p>

            <button onClick={(e) => { e.stopPropagation(); setEditing(a.id); }}>
              Edit ✏️
            </button>

            <button onClick={(e) => { e.stopPropagation(); deleteAppointment(a.id); }}>
              Delete ❌
            </button>
          </div>
        ))}
      </div>

      {editing && (
        <EditAppointment
          appointment={appointments.find(a => a.id === editing)}
          onSave={saveEdit}
          onCancel={() => setEditing(null)}
        />
      )}

      {selectedAppointment && (
        <div className="popup-overlay" onClick={() => setSelectedAppointment(null)}>
          <div className="popup" onClick={(e) => e.stopPropagation()}>
            <h3>Appointment Details</h3>
            <p><strong>Customer:</strong> {selectedAppointment.username}</p>
            <p><strong>Dog Name:</strong> {selectedAppointment.dogName}</p>
            <p><strong>Dog Size:</strong> {selectedAppointment.dogSize}</p>
            <p><strong>Date:</strong> {new Date(selectedAppointment.date).toLocaleString()}</p>
            <p><strong>Created At:</strong> {new Date(selectedAppointment.createdAt).toLocaleString()}</p>
            <p><strong>Duration:</strong> {selectedAppointment.durationMinutes} minutes</p>
            <p><strong>Price:</strong> ₪{selectedAppointment.price}</p>
            <button onClick={() => setSelectedAppointment(null)}>Close</button>
          </div>
        </div>
      )}
    </div>
  );
}