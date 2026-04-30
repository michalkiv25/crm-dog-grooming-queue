import { useState } from "react";

export default function Register() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");

  const handleRegister = async () => {
    const response = await fetch("http://localhost:5285/api/auth/register", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        Username: username,
        Password: password,
        FullName: fullName,
      }),
    });

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
    <div style={{ border: "1px solid #ccc", padding: 20, marginTop: 20 }}>
      <h2>Register 🐶</h2>

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

      <input
        placeholder="Full Name"
        value={fullName}
        onChange={(e) => setFullName(e.target.value)}
      />

      <br /><br />

      <button onClick={handleRegister}>
        Register
      </button>
    </div>
  );
}