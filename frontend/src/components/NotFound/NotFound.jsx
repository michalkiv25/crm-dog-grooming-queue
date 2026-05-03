import { Link } from "react-router-dom";

export default function NotFound({ fallbackPath = "/" }) {
  return (
    <main className="not-found-page">
      <div className="not-found-card">
        <div className="not-found-inner">
          <h1>404</h1>
          <p>The page you requested was not found.</p>
          <Link className="primary-button not-found-link" to={fallbackPath}>
            Back to home
          </Link>
        </div>
      </div>
    </main>
  );
}
