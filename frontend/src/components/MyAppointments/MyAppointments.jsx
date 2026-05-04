import { useEffect, useMemo, useState } from "react";
import { appointmentsService } from "../../services/api";
import EditAppointment from "../EditAppointment/EditAppointment";
import {
  appointmentNumericId,
  cardPrincipalPrice,
  displayCatalogListPrice,
  formatShekelUi,
  loyaltyUiApplies,
} from "../../utils/loyaltyPrice";
import { usernameFromAccessToken } from "../../utils/jwtClaims";
import "./MyAppointments.css";

function canonicalUser(u) {
  return String(u ?? "").trim().toLowerCase();
}

/** Match server loyalty repricing: earliest scheduled appointment first, then by id. */
function compareAppointmentsChronological(a, b) {
  const ta = new Date(a.date ?? a.Date).getTime();
  const tb = new Date(b.date ?? b.Date).getTime();
  if (Number.isFinite(ta) && Number.isFinite(tb) && ta !== tb) return ta - tb;
  return (appointmentNumericId(a) ?? 0) - (appointmentNumericId(b) ?? 0);
}

function sortMineForLoyalty(rows) {
  return [...rows].sort(compareAppointmentsChronological);
}

/** Same local calendar day as now, and the appointment start time has already passed. */
function isPastTodaySlot(isoOrDate) {
  const t = new Date(isoOrDate).getTime();
  if (!Number.isFinite(t) || t >= Date.now()) return false;
  const d = new Date(isoOrDate);
  const n = new Date();
  return (
    d.getFullYear() === n.getFullYear() &&
    d.getMonth() === n.getMonth() &&
    d.getDate() === n.getDate()
  );
}

/**
 * Full salon: GET /appointments/all-appointments. If the running API is an older build (404/405 on that route),
 * fall back to /appointments/upcoming-queue so customers still see everyone’s upcoming slots.
 */
async function fetchSalonBoard() {
  const full = await appointmentsService.allAppointmentsSalon();
  if (full.ok && Array.isArray(full.data)) {
    return { data: full.data, upcomingOnly: false, error: null };
  }
  if (full.status === 404 || full.status === 405) {
    const up = await appointmentsService.upcomingQueue();
    if (up.ok && Array.isArray(up.data)) {
      return { data: up.data, upcomingOnly: true, error: null };
    }
  }
  const hint =
    full.status === 404 || full.status === 405
      ? "If you just added the full-salon route, rebuild and restart the API process so it picks up the new endpoint."
      : "";
  return {
    data: [],
    upcomingOnly: false,
    error:
      [full.data?.message, hint].filter(Boolean).join(" ") ||
      "Could not load the salon appointment list.",
  };
}

export default function MyAppointments({ refreshTrigger }) {
  /** Full salon board — every customer’s appointments. */
  const [salonAppointments, setSalonAppointments] = useState([]);
  const [myAppointmentsChronological, setMyAppointmentsChronological] = useState([]);
  const [editing, setEditing] = useState(null);
  const [loading, setLoading] = useState(true);
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  const [loadError, setLoadError] = useState(null);
  const [mineWarning, setMineWarning] = useState(null);
  /** True when the API has no `all-appointments` route and we showed upcoming-queue instead. */
  const [salonUpcomingOnly, setSalonUpcomingOnly] = useState(false);
  const [myAppointmentCount, setMyAppointmentCount] = useState(undefined);

  /** Prefer JWT (matches API); fall back to login response in localStorage. */
  const me = canonicalUser(
    usernameFromAccessToken(localStorage.getItem("token")) ??
      localStorage.getItem("username")
  );

  useEffect(() => {
    loadAppointments();
  }, [refreshTrigger]);

  const loadAppointments = async () => {
    const token = localStorage.getItem("token");
    if (!token) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setLoadError(null);
    setMineWarning(null);
    setSalonUpcomingOnly(false);
    try {
      const [mineRes, salonBoard] = await Promise.all([
        appointmentsService.getAll(),
        fetchSalonBoard(),
      ]);

      if (mineRes.ok && Array.isArray(mineRes.data)) {
        setMyAppointmentCount(mineRes.data.length);
        setMyAppointmentsChronological(sortMineForLoyalty(mineRes.data));
      } else {
        setMyAppointmentCount(undefined);
        setMyAppointmentsChronological([]);
        const hint =
          mineRes.status === 401 || mineRes.status === 403
            ? "Check your login (token)."
            : "";
        setMineWarning(
          [mineRes.data?.message, hint].filter(Boolean).join(" ") ||
            "Could not load your appointments — loyalty display for your rows may be wrong until you reload."
        );
      }

      setSalonAppointments(salonBoard.data);
      setSalonUpcomingOnly(salonBoard.upcomingOnly);
      setLoadError(salonBoard.error);
    } catch (err) {
      console.error("ERROR:", err);
      setLoadError("Network error while loading appointments.");
      setMineWarning(null);
      setMyAppointmentCount(undefined);
      setMyAppointmentsChronological([]);
      setSalonAppointments([]);
      setSalonUpcomingOnly(false);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Salon board + any of **your** rows from GET /appointments that are missing from the salon payload.
   * When the server only exposes upcoming-queue (fallback), other customers’ past slots are omitted server-side;
   * your own past/future still appear because getAll() is merged in by id.
   */
  const appointments = useMemo(() => {
    const byId = new Map();
    for (const a of salonAppointments) {
      const id = appointmentNumericId(a);
      if (id != null) byId.set(id, a);
    }
    /** Rows from GET /appointments overwrite salon for the same id — correct username + fields for ownership/delete. */
    for (const a of myAppointmentsChronological) {
      const id = appointmentNumericId(a);
      if (id != null) byId.set(id, a);
    }
    return [...byId.values()].sort(
      (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
    );
  }, [salonAppointments, myAppointmentsChronological]);

  /** Ids returned by GET /appointments — authoritative “your rows” even if username text differs on the salon payload. */
  const myAppointmentIds = useMemo(
    () =>
      new Set(
        myAppointmentsChronological
          .map((x) => appointmentNumericId(x))
          .filter((n) => n != null)
      ),
    [myAppointmentsChronological]
  );

  const rowUsername = (a) => a?.username ?? a?.Username;

  const isMine = (a) => {
    if (!me) return false;
    const id = appointmentNumericId(a);
    if (id != null && myAppointmentIds.has(id)) return true;
    return canonicalUser(rowUsername(a)) === me;
  };

  const appointmentById = (id) =>
    salonAppointments.find((x) => appointmentNumericId(x) === Number(id)) ??
    myAppointmentsChronological.find((x) => appointmentNumericId(x) === Number(id));

  /** Close edit modal if this row became a “past today” slot (e.g. after refresh). */
  useEffect(() => {
    if (editing == null) return;
    const ap = appointmentById(editing);
    if (ap && isPastTodaySlot(ap.date) && isMine(ap)) setEditing(null);
  }, [editing, salonAppointments, myAppointmentsChronological]);

  const loyaltyCtx = (a) => {
    const mine = isMine(a);
    let mineSlotIndex;
    if (mine && myAppointmentsChronological.length > 0) {
      const idxId = appointmentNumericId(a);
      const idx = myAppointmentsChronological.findIndex((x) => appointmentNumericId(x) === idxId);
      if (idx >= 0) mineSlotIndex = idx;
    }
    return {
      isMine: mine,
      mineTotalAppointments: mine ? myAppointmentCount : undefined,
      mineSlotIndex,
    };
  };

  const refreshAfterMutation = async (deletedId) => {
    const [mineRes, salonBoard] = await Promise.all([
      appointmentsService.getAll(),
      fetchSalonBoard(),
    ]);
    if (mineRes.ok && Array.isArray(mineRes.data)) {
      setMyAppointmentCount(mineRes.data.length);
      setMyAppointmentsChronological(sortMineForLoyalty(mineRes.data));
    } else {
      setMyAppointmentCount(undefined);
      setMyAppointmentsChronological([]);
    }
    if (!salonBoard.error && Array.isArray(salonBoard.data)) {
      setSalonAppointments(salonBoard.data);
      setSalonUpcomingOnly(salonBoard.upcomingOnly);
    } else {
      const nid = Number(deletedId);
      if (Number.isFinite(nid)) {
        setSalonAppointments((prev) => prev.filter((a) => appointmentNumericId(a) !== nid));
      }
    }
  };

  /** Remove your own booking (blocked in UI for “today” slot whose time has passed). */
  const deleteAppointment = async (id) => {
    const nid = Number(id);
    if (!Number.isFinite(nid)) {
      alert("Invalid appointment id.");
      return;
    }
    const appt = appointmentById(nid);
    if (appt && isPastTodaySlot(appt.date) && isMine(appt)) {
      alert("אי אפשר לערוך או למחוק תור שעברה שעתו באותו יום.");
      return;
    }
    const { ok, data, status } = await appointmentsService.delete(nid);
    if (ok) {
      setSelectedAppointment((s) =>
        appointmentNumericId(s) === nid ? null : s
      );
      await refreshAfterMutation(nid);
      alert("Appointment deleted ✓");
    } else {
      const reason = data?.errors?.[0] ?? data?.message;
      const http = status ? ` (HTTP ${status})` : "";
      alert(
        typeof reason === "string" && reason.trim()
          ? `Could not delete: ${reason}${http}`
          : `Failed to delete${http} ❌`
      );
    }
  };

  /** Called after EditAppointment successfully PUTs — refreshes lists (no second PUT). */
  const saveEdit = async (updated) => {
    if (!updated) return;
    const id = appointmentNumericId(updated);
    setEditing(null);
    setSelectedAppointment((s) =>
      appointmentNumericId(s) === id ? updated : s
    );
    const [mineRes, salonBoard] = await Promise.all([
      appointmentsService.getAll(),
      fetchSalonBoard(),
    ]);
    if (mineRes.ok && Array.isArray(mineRes.data)) {
      setMyAppointmentCount(mineRes.data.length);
      setMyAppointmentsChronological(sortMineForLoyalty(mineRes.data));
    } else {
      setMyAppointmentCount(undefined);
      setMyAppointmentsChronological([]);
    }
    if (!salonBoard.error && Array.isArray(salonBoard.data)) {
      setSalonAppointments(salonBoard.data);
      setSalonUpcomingOnly(salonBoard.upcomingOnly);
    } else {
      setSalonAppointments((prev) =>
        prev.map((a) => (appointmentNumericId(a) === id ? updated : a))
      );
    }
  };

  if (loading) {
    return <h3 className="my-appts-loading">Loading appointments...</h3>;
  }

  return (
    <div className="my-appointments-page">
      <h2>🐶 The appointments of Michaela's dog grooming salon</h2>
   
      {!me && (
        <p className="my-appointments-warning">
          Username not saved in this browser — log in again to see edit/delete on your appointments.
        </p>
      )}

      {mineWarning && (
        <p className="my-appointments-warning" role="status">
          {mineWarning}
        </p>
      )}


      {loadError && (
        <p className="my-appointments-warning" role="alert">
          {loadError}
        </p>
      )}

      {appointments.length === 0 && !loadError && (
        <p className="empty">No appointments in the system yet 🐶</p>
      )}

      <div className="cards-container">
        {appointments.map((a) => {
          const ctx = loyaltyCtx(a);
          const mine = isMine(a);
          const pastToday = mine && isPastTodaySlot(a.date);
          return (
            <div
              key={a.id}
              className={`card ${mine ? "card--mine" : "card--readonly"}`}
              onClick={() => setSelectedAppointment(a)}
              role="presentation"
            >
              <h3>👤 {rowUsername(a)}</h3>
              <p>🐾 Dog: {a.dogName}</p>
              <p>📏 Size: {a.dogSize}</p>
              <p>📅 Date: {new Date(a.date).toLocaleString()}</p>
              <p>💰 Price: ₪{formatShekelUi(cardPrincipalPrice(a, ctx))}</p>
              {loyaltyUiApplies(a, ctx) && (
                <p className="card-loyalty-note">
                  List price ₪{formatShekelUi(displayCatalogListPrice(a))} — 10% loyalty discount applied
                </p>
              )}

              {!mine && (
                <p className="card-readonly-hint">Another customer’s appointment — view only</p>
              )}
              {pastToday && (
                <div className="card-past-today">
                  <p className="card-past-today__line">התור עבר</p>
                  <p className="card-past-today__sub">אי אפשר לערוך או למחוק</p>
                </div>
              )}
              {mine && me && !pastToday && (
                <div className="card-actions-own">
                  <button
                    type="button"
                    className="btn-edit-own"
                    onClick={(e) => {
                      e.stopPropagation();
                      setEditing(appointmentNumericId(a) ?? a.id);
                    }}
                  >
                    Edit ✏️
                  </button>
                  <button
                    type="button"
                    className="btn-delete-own"
                    onClick={(e) => {
                      e.stopPropagation();
                      const id = appointmentNumericId(a) ?? a.id;
                      if (id != null) deleteAppointment(id);
                    }}
                  >
                    Delete ❌
                  </button>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {editing &&
        (() => {
          const ap = appointmentById(editing);
          if (!ap) return null;
          if (isPastTodaySlot(ap.date) && isMine(ap)) return null;
          return (
            <EditAppointment
              appointment={ap}
              onSave={saveEdit}
              onCancel={() => setEditing(null)}
            />
          );
        })()}

      {selectedAppointment && (
        <div className="popup-overlay" onClick={() => setSelectedAppointment(null)}>
          <div className="popup" onClick={(e) => e.stopPropagation()}>
            <h3>Appointment details</h3>
            <p>
              <strong>Customer:</strong> {rowUsername(selectedAppointment)}
            </p>
            <p>
              <strong>Dog:</strong> {selectedAppointment.dogName}
            </p>
            <p>
              <strong>Size:</strong> {selectedAppointment.dogSize}
            </p>
            <p>
              <strong>Date:</strong>{" "}
              {new Date(selectedAppointment.date).toLocaleString()}
            </p>
            <p>
              <strong>Created:</strong>{" "}
              {new Date(selectedAppointment.createdAt).toLocaleString()}
            </p>
            <p>
              <strong>Duration:</strong> {selectedAppointment.durationMinutes}{" "}
              minutes
            </p>
            <p>
              <strong>Price:</strong> ₪
              {formatShekelUi(
                cardPrincipalPrice(selectedAppointment, loyaltyCtx(selectedAppointment))
              )}
            </p>
            {loyaltyUiApplies(selectedAppointment, loyaltyCtx(selectedAppointment)) && (
              <p className="card-loyalty-note">
                List price ₪
                {formatShekelUi(displayCatalogListPrice(selectedAppointment))} — 10% loyalty discount applied
              </p>
            )}
            {!isMine(selectedAppointment) && (
              <p className="card-readonly-hint popup-readonly-hint">
                Another customer’s appointment — view only (log in as that customer to edit or delete).
              </p>
            )}
            {isMine(selectedAppointment) && isPastTodaySlot(selectedAppointment.date) && (
              <div className="card-past-today popup-past-today">
                <p className="card-past-today__line">התור עבר</p>
                <p className="card-past-today__sub">אי אפשר לערוך או למחוק</p>
              </div>
            )}
            <div className="popup-actions-row">
              {me &&
                isMine(selectedAppointment) &&
                !isPastTodaySlot(selectedAppointment.date) && (
                <>
                  <button
                    type="button"
                    className="btn-edit-own"
                    onClick={() => {
                      const sid = appointmentNumericId(selectedAppointment);
                      setSelectedAppointment(null);
                      if (sid != null) setEditing(sid);
                    }}
                  >
                    Edit ✏️
                  </button>
                  <button
                    type="button"
                    className="btn-delete-own"
                    onClick={() => {
                      const sid = appointmentNumericId(selectedAppointment);
                      if (sid != null) deleteAppointment(sid);
                    }}
                  >
                    Delete ❌
                  </button>
                </>
              )}
              <button type="button" className="popup-close-btn" onClick={() => setSelectedAppointment(null)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
