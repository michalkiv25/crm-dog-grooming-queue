import { useState } from "react";

export default function Register() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");

  const handleRegister = async () => {
    console.log({ username, password, fullName });

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

    const text = await response.text();
    console.log("SERVER RESPONSE:", text);

    if (response.ok) {
      alert("User registered 🎉");

      setUsername("");
      setPassword("");
      setFullName("");
    } else {
      alert("Register failed ❌");
    }
  };

  return (
    <div className="auth-card">
      <h2>Register 🐶</h2>

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

      <label>
        Full Name
        <input
          placeholder="Full Name"
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
        />
      </label>

      <button className="primary-button" onClick={handleRegister}>
        Register
      </button>
    </div>
  );
}