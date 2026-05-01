import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authService } from "../../services/api";

export default function Login({ onLogin, onSwitchToRegister }) {
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  const validateInput = () => {
    const newErrors = [];
    
    if (!username.trim()) {
      newErrors.push("Username is required");
    }
    
    if (!password.trim()) {
      newErrors.push("Password is required");
    }
    
    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const handleLogin = async () => {
    if (!validateInput()) return;

    setLoading(true);
    try {
      const { ok, data } = await authService.login(username, password);

      console.log("LOGIN RESPONSE:", data);

      if (ok && data?.token) {
        localStorage.setItem("token", data.token);
        localStorage.setItem("fullname", data.fullname);
        onLogin?.(data.token);
        navigate("/appointments");
      } else {
        const errorMessage = data?.errors?.length 
          ? data.errors[0] 
          : data?.message || "Login failed ❌";
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
      <h2>Login 🔐</h2>

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

      <button className="primary-button" onClick={handleLogin} disabled={loading}>
        {loading ? "Logging in..." : "Login"}
      </button>

      <p className="auth-switch-text">
        אין לך חשבון?{" "}
        <button
          type="button"
          className="auth-link-button"
          onClick={() => onSwitchToRegister?.()}
        >
          להרשמה
        </button>
      </p>
    </div>
  );
}