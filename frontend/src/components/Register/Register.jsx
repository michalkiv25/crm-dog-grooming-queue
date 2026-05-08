import { useState } from "react";
import { authService } from "../../services/api";
import { validateRegisterInput } from "../../utils/formValidation";
import "./Register.css";

/** Username: Unicode letters only (no digits or punctuation). */
const sanitizeUsernameLettersOnly = (value) =>
  value.replace(/[^\p{L}]/gu, "");

/** Full name: letters, spaces, hyphen, apostrophe. */
const sanitizeFullNameTextOnly = (value) =>
  value.replace(/[^\p{L}\s'\-]/gu, "");

export default function Register({ onSwitchToLogin }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [errors, setErrors] = useState([]);
  const [loading, setLoading] = useState(false);

  const validateInput = () => {
    const newErrors = validateRegisterInput({ username, password, fullName });
    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const handleRegister = async () => {
    if (!validateInput()) return;

    setLoading(true);
    try {
      const { ok, data } = await authService.register(username, password, fullName);

      if (ok) {
        const savedUsername = username.trim();
        setUsername("");
        setPassword("");
        setFullName("");
        setErrors([]);
        onSwitchToLogin?.(savedUsername);
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
          inputMode="text"
          autoComplete="username"
          value={username}
          onChange={(e) => setUsername(sanitizeUsernameLettersOnly(e.target.value))}
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
          inputMode="text"
          autoComplete="name"
          value={fullName}
          onChange={(e) => setFullName(sanitizeFullNameTextOnly(e.target.value))}
          disabled={loading}
        />
      </label>

      <button className="primary-button" onClick={handleRegister} disabled={loading}>
        {loading ? "Registering..." : "Register"}
      </button>

      <p className="auth-switch-text">
        Already registered?{" "}
        <button
          type="button"
          className="auth-link-button"
          onClick={() => onSwitchToLogin?.()}
        >
          Log in
        </button>
      </p>
    </div>
  );
}