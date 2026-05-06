// Centralized API service for all backend calls.
// Local: default below. Production (e.g. Render): set VITE_API_URL to API origin without trailing slash (https://your-api.onrender.com).
const API_ORIGIN =
  import.meta.env.VITE_API_URL?.trim().replace(/\/$/, "") ||
  "http://localhost:5285";
const API_BASE_URL = `${API_ORIGIN}/api`;
const REQUEST_TIMEOUT_MS = 20000;

// Helper function to get auth token
const getAuthToken = () => localStorage.getItem("token");

// Helper function for API calls with auth
const apiCall = async (endpoint, options = {}) => {
  const token = getAuthToken();
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);
  
  const headers = {
    "Content-Type": "application/json",
    ...(token && { Authorization: `Bearer ${token}` }),
    ...options.headers,
  };

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
      signal: controller.signal,
    });

    /** DELETE often returns 204 / empty body — avoid assuming JSON. */
    let data = null;
    try {
      const text = await response.text();
      if (text?.trim()) data = JSON.parse(text);
    } catch {
      data = null;
    }

    return {
      response,
      data,
      ok: response.ok,
      status: response.status,
    };
  } catch (error) {
    const isTimeout = error?.name === "AbortError";
    return {
      response: null,
      data: {
        message: isTimeout
          ? "Request timed out. Please try again."
          : "Network error. Please try again.",
      },
      ok: false,
      status: 0,
    };
  } finally {
    clearTimeout(timeoutId);
  }
};

// ==================== AUTH ENDPOINTS ====================
export const authService = {
  async register(username, password, fullName) {
    return apiCall("/auth/register", {
      method: "POST",
      body: JSON.stringify({
        username: username.trim(),
        password: password.trim(),
        fullName: fullName.trim(),
      }),
    });
  },

  async login(username, password) {
    return apiCall("/auth/login", {
      method: "POST",
      body: JSON.stringify({
        username: username.trim(),
        password: password.trim(),
      }),
    });
  },

  logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("fullname");
  },
};

// ==================== APPOINTMENTS ENDPOINTS ====================
export const appointmentsService = {
  async getAll() {
    return apiCall("/appointments");
  },

  /** All customers’ future appointments (salon queue) */
  async upcomingQueue() {
    return apiCall("/appointments/upcoming-queue");
  },

  /** Every customer’s appointments — past and future (full salon board). */
  async allAppointmentsSalon() {
    return apiCall("/appointments/all-appointments");
  },

  async create(dogName, dogSize, date) {
    return apiCall("/appointments", {
      method: "POST",
      body: JSON.stringify({
        dogName: dogName.trim(),
        dogSize,
        date,
      }),
    });
  },

  async update(id, dogName, dogSize, date) {
    return apiCall(`/appointments/${id}`, {
      method: "PUT",
      body: JSON.stringify({
        dogName: dogName.trim(),
        dogSize,
        date,
      }),
    });
  },

  async delete(id) {
    return apiCall(`/appointments/${id}`, {
      method: "DELETE",
    });
  },

  async filter(date, customerName) {
    const params = new URLSearchParams();
    if (date) params.append("date", date);
    if (customerName) params.append("customerName", customerName);

    return apiCall(`/appointments/filter?${params.toString()}`);
  },
};

export default {
  authService,
  appointmentsService,
};
