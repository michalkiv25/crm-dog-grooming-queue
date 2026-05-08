import "./AuthenticatedChrome.css";

export default function AuthenticatedChrome({ onLogout, children }) {
  return (
    <>
      <div className="logout-container">
        <button className="logout-button" type="button" onClick={onLogout}>
          Log out
        </button>
        <p className="logout-greeting">
          Hello {localStorage.getItem("fullname") || "User"}
        </p>
      </div>

      {children}
    </>
  );
}
