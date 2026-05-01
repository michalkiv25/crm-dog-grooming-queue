import { useState } from "react";

export default function Register() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  const validateInput = () => {
    const newErrors = [];

    if (!username.trim()) {
      newErrors.push("Username is required");
    } else if (username.trim().length < 3) {
      newErrors.push("Username must be at least 3 characters");
    } else if (username.trim().length > 30) {
      newErrors.push("Username must not exceed 30 characters");
    }

    if (!password.trim()) {
      newErrors.push("Password is required");
    } else if (password.trim().length < 6) {
      newErrors.push("Password must be at least 6 characters");
    }

    if (!fullName.trim()) {
      newErrors.push("Full name is required");
    } else if (fullName.trim().length < 2) {
      newErrors.push("Full name must be at least 2 characters");
    } else if (fullName.trim().length > 100) {
      newErrors.push("Full name must not exceed 100 characters");
    }

    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const handleRegister = async () => {
    if (!validateInput()) return;

    setLoading(true);
    try {
      const response = await fetch("http://localhost:5285/api/auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Username: username.trim(),
          Password: password.trim(),
          FullName: fullName.trim(),
        }),
      });

      const data = await response.json().catch(() => null);
      console.log("SERVER RESPONSE:", data);

      if (response.ok) {
        alert("User registered 🎉");
        setUsername("");
        setPassword("");
        setFullName("");
        setErrors([]);
      } else {
        const errorMessage = data?.errors?.length 
          ? data.errors[0] 
          : data?.message || "Register failed ❌";
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
      <h2>Register 🐶</h2>

      {errors.length > 0 && (
        <div className="error-box">
          {errors.map((error, idx) => (
            <p key={idx} className="error-message">❌ {error}</p>
          ))}
        </div>
      )}

      <label>
        Username
        <input
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          disabled={loading}
        />
      </label>
      <label>
        Password
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={loading}
        />
      </label>

      <label>
        Full Name
        <input
          placeholder="Full Name"
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
          disabled={loading}
        />
      </label>

      <button className="primary-button" onClick={handleRegister} disabled={loading}>
        {loading ? "Registering..." : "Register"}
      </button>
    </div>
  );
}