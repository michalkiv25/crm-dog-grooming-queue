import { useState } from "react";
import { useNavigate } from "react-router-dom";

export default function Login({ onLogin }) {
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

 const handleLogin = async () => {
  const response = await fetch("http://localhost:5285/api/auth/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      username,
      password,
    }),
  });

  const data = await response.json().catch(() => null);

  console.log("LOGIN RESPONSE:", data);

  if (response.ok && data?.token) {
    localStorage.setItem("token", data.token);
    onLogin?.(data.token);
    navigate("/appointments");
  } else {
    alert(data?.message || "Login failed ❌");
  }
};

  return (
    <div className="auth-card">
      <h2>Login 🔐</h2>

      <label>
        Username
        <input
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
      </label>

      <label>
        Password
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
      </label>

      <button className="primary-button" onClick={handleLogin}>Login</button>
    </div>
  );
}