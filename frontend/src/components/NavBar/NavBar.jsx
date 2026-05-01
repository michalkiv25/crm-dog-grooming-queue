import { Link, useNavigate } from "react-router-dom";

export default function NavBar() {
  const navigate = useNavigate();

 const logout = () => {
  localStorage.removeItem("token");
  window.location.reload(); 
};

  return (
    <div className="navbar">
      <Link to="/appointments">My Appointments</Link>
      <Link to="/create">Create</Link>

      <button onClick={logout}>Logout</button>
    </div>
  );
}