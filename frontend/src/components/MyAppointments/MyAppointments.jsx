import { useEffect, useMemo, useState } from "react";
import DatePicker, { registerLocale } from "react-datepicker";
import { enUS } from "date-fns/locale/en-US";
import "react-datepicker/dist/react-datepicker.css";
import { appointmentsService } from "../../services/api";
import EditAppointment from "../EditAppointment/EditAppointment";
import {
  appointmentNumericId,
  cardPrincipalPrice,
  displayCatalogListPrice,
  formatShekelUi,
  loyaltyUiApplies,
} from "../../utils/loyaltyPrice";
import "./MyAppointments.css";

registerLocale("enUS", enUS);

function canonicalUser(u) {
  return String(u ?? "").trim().toLowerCase();
}

export default function MyAppointments({ refreshTrigger }) {
  const [allUpcoming, setAllUpcoming] = useState([]);
  const [editing, setEditing] = useState(null);
  const [loading, setLoading] = useState(true);
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  /** Show appointments starting at or after this local date-time (from calendar picker). */
  const [filterFromDateTime, setFilterFromDateTime] = useState(null);
  const [filterPickerOpen, setFilterPickerOpen] = useState(false);
  const [filterModalSelected, setFilterModalSelected] = useState(null);
  const [filterCustomer, setFilterCustomer] = useState("");
  const [loadError, setLoadError] = useState(null);
  /** Total appointments for the logged-in user (getAll); used when slot index is unknown (stale-DB guard). */
  const [myAppointmentCount, setMyAppointmentCount] = useState(undefined);
  /** Same user’s rows from getAll(), sorted by id ascending — matches server loyalty slot order. */
  const [myAppointmentsSortedById, setMyAppointmentsSortedById] = useState([]);

  const me = canonicalUser(localStorage.getItem("username"));

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
    try {
      const [result, mineRes] = await Promise.all([
        appointmentsService.upcomingQueue(),
        appointmentsService.getAll(),
      ]);

      if (mineRes.ok && Array.isArray(mineRes.data)) {
        setMyAppointmentCount(mineRes.data.length);
        setMyAppointmentsSortedById(
          [...mineRes.data].sort(
            (a, b) => (appointmentNumericId(a) ?? 0) - (appointmentNumericId(b) ?? 0)
          )
        );
      } else {
        setMyAppointmentCount(undefined);
        setMyAppointmentsSortedById([]);
      }

      if (result.ok && Array.isArray(result.data)) {
        setAllUpcoming(result.data);
        return;
      }

      const hint =
        result.status === 401 || result.status === 403
          ? "Check your login (token)."
          : result.status === 404 || result.status === 405
            ? "This API does not expose the shared queue route — restart the API from the latest code."
            : "";
      setLoadError(
        [result.data?.message, hint].filter(Boolean).join(" ") ||
          "Could not load the salon upcoming queue (upcoming-queue)."
      );
      setAllUpcoming([]);
    } catch (err) {
      console.error("ERROR:", err);
      setLoadError("Network error while loading appointments.");
      setAllUpcoming([]);
      setMyAppointmentCount(undefined);
      setMyAppointmentsSortedById([]);
    } finally {
      setLoading(false);
    }
  };

  const appointments = useMemo(() => {
    let list = allUpcoming;
    if (filterFromDateTime) {
      const from = filterFromDateTime.getTime();
      list = list.filter((a) => new Date(a.date).getTime() >= from);
    }
    if (filterCustomer.trim()) {
      const q = filterCustomer.trim().toLowerCase();
      list = list.filter((a) =>
        String(a.username ?? "")
          .toLowerCase()
          .includes(q)
      );
    }
    return list;
  }, [allUpcoming, filterFromDateTime, filterCustomer]);

  const isMine = (a) => canonicalUser(a.username) === me;

  const filterPassedTime = (time) => time.getTime() > Date.now();

  const filterDateTimeDisplay = filterFromDateTime
    ? filterFromDateTime.toLocaleString("en-US", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      })
    : "";

  const openFilterDatePicker = () => {
    setFilterModalSelected(
      filterFromDateTime ? new Date(filterFromDateTime.getTime()) : new Date()
    );
    setFilterPickerOpen(true);
  };

  const applyFilterDatePicker = () => {
    if (!filterModalSelected) return;
    setFilterFromDateTime(new Date(filterModalSelected.getTime()));
    setFilterPickerOpen(false);
  };

  const hasActiveFilters = Boolean(filterFromDateTime) || filterCustomer.trim().length > 0;

  const loyaltyCtx = (a) => {
    const mine = isMine(a);
    let mineSlotIndex;
    if (mine && myAppointmentsSortedById.length > 0) {
      const id = appointmentNumericId(a);
      const idx = myAppointmentsSortedById.findIndex((x) => appointmentNumericId(x) === id);
      if (idx >= 0) mineSlotIndex = idx;
    }
    return {
      isMine: mine,
      mineTotalAppointments: mine ? myAppointmentCount : undefined,
      mineSlotIndex,
    };
  };

  const deleteAppointment = async (id) => {
    const { ok } = await appointmentsService.delete(id);
    if (ok) {
      setSelectedAppointment((s) =>
        appointmentNumericId(s) === Number(id) ? null : s
      );
      const [queue, mineRes] = await Promise.all([
        appointmentsService.upcomingQueue(),
        appointmentsService.getAll(),
      ]);
      if (mineRes.ok && Array.isArray(mineRes.data)) {
        setMyAppointmentCount(mineRes.data.length);
        setMyAppointmentsSortedById(
          [...mineRes.data].sort(
            (a, b) => (appointmentNumericId(a) ?? 0) - (appointmentNumericId(b) ?? 0)
          )
        );
      } else {
        setMyAppointmentCount(undefined);
        setMyAppointmentsSortedById([]);
      }
      if (queue.ok && Array.isArray(queue.data)) {
        setAllUpcoming(queue.data);
      } else {
        const nid = Number(id);
        setAllUpcoming((prev) =>
          prev.filter((a) => appointmentNumericId(a) !== nid)
        );
      }
      alert("Appointment deleted ✓");
    } else {
      alert("Failed to delete ❌");
    }
  };

  const saveEdit = async (id, dogName, dogSize, date) => {
    const { ok, data } = await appointmentsService.update(id, dogName, dogSize, date);
    if (ok) {
      setEditing(null);
      setSelectedAppointment((s) => (s?.id === id ? data : s));
      const [queue, mineRes] = await Promise.all([
        appointmentsService.upcomingQueue(),
        appointmentsService.getAll(),
      ]);
      if (mineRes.ok && Array.isArray(mineRes.data)) {
        setMyAppointmentCount(mineRes.data.length);
        setMyAppointmentsSortedById(
          [...mineRes.data].sort(
            (a, b) => (appointmentNumericId(a) ?? 0) - (appointmentNumericId(b) ?? 0)
          )
        );
      } else {
        setMyAppointmentCount(undefined);
        setMyAppointmentsSortedById([]);
      }
      if (queue.ok && Array.isArray(queue.data)) {
        setAllUpcoming(queue.data);
      } else {
        setAllUpcoming((prev) => prev.map((a) => (a.id === id ? data : a)));
      }
    } else {
      console.error("Failed to update:", data);
    }
  };

  if (loading) {
    return <h3 className="my-appts-loading">Loading appointments...</h3>;
  }

  return (
    <div className="my-appointments-page">
      <h2>🐶 My Appointments</h2>
      <p className="my-appointments-intro">
        This page lists <strong>all customers’</strong> upcoming appointments. You can edit or delete{" "}
        <strong>only</strong> appointments that belong to your account.
      </p>
      <p className="my-appointments-intro my-appointments-intro--loyalty">
        <strong>Loyalty pricing:</strong> your first three bookings are always full price. From your fourth
        booking onward, those appointments are 10% off. If you delete appointments and only three (or fewer)
        remain, every remaining booking returns to full price until you book a fourth again.
      </p>

      {!me && (
        <p className="my-appointments-warning">
          Username not saved in this browser — log in again to see edit/delete on your appointments.
        </p>
      )}

      {loadError && (
        <p className="my-appointments-warning" role="alert">
          {loadError}
        </p>
      )}

      <div className="filters">
        <label className="filters__date-label">
          <span className="filters__date-caption">From date &amp; time</span>
          <input
            className="date-time-trigger"
            type="text"
            value={filterDateTimeDisplay}
            readOnly
            placeholder="Click to open calendar"
            onClick={openFilterDatePicker}
            onKeyDown={(e) => {
              if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                openFilterDatePicker();
              }
            }}
            role="button"
            tabIndex={0}
            aria-label="Filter from date and time — open calendar"
          />
        </label>
        <input
          type="text"
          value={filterCustomer}
          onChange={(e) => setFilterCustomer(e.target.value)}
          placeholder="Filter by customer name"
        />
        <button
          type="button"
          onClick={() => {
            setFilterFromDateTime(null);
            setFilterModalSelected(null);
            setFilterCustomer("");
            setFilterPickerOpen(false);
          }}
        >
          Clear filters
        </button>
      </div>

      {appointments.length === 0 && (
        <p className={`empty${allUpcoming.length > 0 && hasActiveFilters ? " empty--filter" : ""}`}>
          {allUpcoming.length > 0 && hasActiveFilters
            ? "No matches — clear filters or change the date."
            : "No upcoming appointments to show 🐶"}
        </p>
      )}

      <div className="cards-container">
        {appointments.map((a) => {
          const mine = isMine(a);
          return (
            <div
              key={a.id}
              className={`card ${mine ? "card--mine" : "card--readonly"}`}
              onClick={() => setSelectedAppointment(a)}
              role="presentation"
            >
              <h3>👤 {a.username}</h3>
              <p>🐾 Dog: {a.dogName}</p>
              <p>📏 Size: {a.dogSize}</p>
              <p>📅 Date: {new Date(a.date).toLocaleString()}</p>
              <p>💰 Price: ₪{formatShekelUi(cardPrincipalPrice(a, loyaltyCtx(a)))}</p>
              {loyaltyUiApplies(a, loyaltyCtx(a)) && (
                <p className="card-loyalty-note">
                  List price ₪{formatShekelUi(displayCatalogListPrice(a))} — 10%
                  loyalty discount applied
                </p>
              )}

              {!mine && (
                <p className="card-readonly-hint">
                  Another customer’s appointment — view only
                </p>
              )}
              {mine && (
                <div className="card-actions-own">
                  <button
                    type="button"
                    className="btn-edit-own"
                    onClick={(e) => {
                      e.stopPropagation();
                      setEditing(a.id);
                    }}
                  >
                    Edit ✏️
                  </button>
                  <button
                    type="button"
                    className="btn-delete-own"
                    onClick={(e) => {
                      e.stopPropagation();
                      deleteAppointment(a.id);
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

      {editing && (
        <EditAppointment
          appointment={allUpcoming.find(
            (a) => appointmentNumericId(a) === Number(editing)
          )}
          onSave={saveEdit}
          onCancel={() => setEditing(null)}
        />
      )}

      {selectedAppointment && (
        <div className="popup-overlay" onClick={() => setSelectedAppointment(null)}>
          <div className="popup" onClick={(e) => e.stopPropagation()}>
            <h3>Appointment details</h3>
            <p>
              <strong>Customer:</strong> {selectedAppointment.username}
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
                {formatShekelUi(displayCatalogListPrice(selectedAppointment))} — 10%
                loyalty discount applied
              </p>
            )}
            <button type="button" onClick={() => setSelectedAppointment(null)}>
              Close
            </button>
          </div>
        </div>
      )}

      {filterPickerOpen && (
        <div
          className="modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="filter-picker-title"
          onClick={() => setFilterPickerOpen(false)}
        >
          <div
            className="modal modal--datepicker appointment-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <h3 id="filter-picker-title">Filter from date and time</h3>
            <p className="filter-picker-hint">
              Only appointments at or after the selected moment are shown.
            </p>
            <div className="appointment-datepicker-wrap">
              <DatePicker
                inline
                selected={filterModalSelected}
                onChange={(date) => setFilterModalSelected(date)}
                showTimeSelect
                timeIntervals={15}
                timeCaption="Time"
                dateFormat="Pp"
                locale="enUS"
                filterTime={filterPassedTime}
                calendarClassName="appointment-calendar-inner"
              />
            </div>
            <div className="modal-buttons">
              <button type="button" onClick={() => setFilterPickerOpen(false)}>
                Cancel
              </button>
              <button type="button" className="primary-button" onClick={applyFilterDatePicker}>
                Apply
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
