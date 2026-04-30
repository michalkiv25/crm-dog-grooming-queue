import { useEffect, useState } from "react";
import "./NavBar.css";

export default function NavBar() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("token");
    setIsLoggedIn(!!token);
  }, []);

  const logout = () => {
    localStorage.removeItem("token");
    setIsLoggedIn(false);
    window.location.href = "/";
  };

  return (
    <nav className="nav">
      <h2 className="logo">🐶 Dog Queue</h2>

      <div className="links">
        <a href="/">Home</a>

        {!isLoggedIn && <a href="/login">Login</a>}

        {isLoggedIn && (
          <>
            <a href="/dashboard">Dashboard</a>
            <a href="/appointments">My Appointments</a>

            <button className="button" onClick={logout}>
              Logout
            </button>
          </>
        )}
      </div>
    </nav>
  );
}