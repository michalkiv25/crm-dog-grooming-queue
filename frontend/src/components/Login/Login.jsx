import { useState } from "react";

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

const handleLogin = async () => {
  const response = await fetch("http://localhost:5285/api/auth/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      username: username,   // 👈 קטן!
      password: password    // 👈 קטן!
    }),
  });

  const data = await response.json().catch(() => null);

  console.log("LOGIN RESPONSE:", data);

  if (response.ok) {
    localStorage.setItem("token", data.token);
    alert("Login success 🎉");
  } else {
    alert(data?.message || "Login failed ❌");
  }
};

  return (
    <div style={{ border: "1px solid #ccc", padding: 20 }}>
      <h2>Login 🔐</h2>

      <input
        placeholder="Username"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />

      <br /><br />

      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />

      <br /><br />

      <button onClick={handleLogin}>
        Login
      </button>
    </div>
  );
}