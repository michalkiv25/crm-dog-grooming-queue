import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { authService } from "../../services/api";
import { validateLoginInput } from "../../utils/formValidation";
import "./Login.css";

export default function Login({
  onLogin,
  onSwitchToRegister,
  defaultUsername = "",
}) {
  const navigate = useNavigate();

  const [username, setUsername] = useState(defaultUsername);
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setUsername(defaultUsername);
  }, [defaultUsername]);

  const validateInput = () => {
    const newErrors = validateLoginInput({ username, password });
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
        localStorage.setItem("fullname", data.fullname ?? "");
        if (data.username != null) {
          localStorage.setItem("username", String(data.username).trim());
        }
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
        Don&apos;t have an account?{" "}
        <button
          type="button"
          className="auth-link-button"
          onClick={() => onSwitchToRegister?.()}
        >
          Register
        </button>
      </p>
    </div>
  );
}