import { useState } from "react";

export default function CreateAppointment({ onSuccess }) {
  const [dogName, setDogName] = useState("");
  const [dogSize, setDogSize] = useState("");
  const [date, setDate] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  const validateInput = () => {
    const newErrors = [];

    if (!dogName.trim()) {
      newErrors.push("Dog name is required");
    } else if (dogName.trim().length < 2) {
      newErrors.push("Dog name must be at least 2 characters");
    } else if (dogName.trim().length > 50) {
      newErrors.push("Dog name must not exceed 50 characters");
    }

    if (!dogSize) {
      newErrors.push("Dog size is required");
    } else if (!["small", "medium", "large"].includes(dogSize)) {
      newErrors.push("Invalid dog size selected");
    }

    if (!date) {
      newErrors.push("Appointment date is required");
    } else if (new Date(date) <= new Date()) {
      newErrors.push("Appointment date must be in the future");
    }

    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const createAppointment = async () => {
    if (!validateInput()) return;

    setLoading(true);
    try {
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

      const data = await response.json().catch(() => null);

      if (response.ok) {
        alert("Appointment created 🐶🎉");
        setDogName("");
        setDogSize("");
        setDate("");
        setErrors([]);
        onSuccess?.();
      } else {
        const errorMessage = data?.errors?.length 
          ? data.errors[0] 
          : data?.message || "Failed to create appointment ❌";
        setErrors([errorMessage]);
      }
    } catch (err) {
      setErrors(["Network error. Please try again."]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-card">
      <h3>Create Appointment 🐶</h3>

      {errors.length > 0 && (
        <div className="error-box">
          {errors.map((error, idx) => (
            <p key={idx} className="error-message">❌ {error}</p>
          ))}
        </div>
      )}

      <label>
        Dog Name
        <input
          placeholder="Dog name"
          value={dogName}
          onChange={(e) => setDogName(e.target.value)}
          disabled={loading}
        />
      </label>

      <label>
        Dog Size
        <select value={dogSize} onChange={(e) => setDogSize(e.target.value)} disabled={loading}>
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
          disabled={loading}
        />
      </label>

      <button className="primary-button" onClick={createAppointment} disabled={loading}>
        {loading ? "Creating..." : "Create Appointment"}
      </button>
    </div>
  );
}