/** Align with backend Appointment.CalculatePriceAndDuration */
export const BASE_PRICE_BY_SIZE = {
  small: 100,
  medium: 150,
  large: 200,
};

/** JSON ids must normalize — JS Map distinguishes 12 from "12". */
export function appointmentNumericId(appointment) {
  const n = Number(appointment?.id);
  return Number.isFinite(n) ? n : null;
}

export function basePriceForDogSize(dogSize) {
  if (!dogSize) return null;
  const key = String(dogSize).toLowerCase();
  return BASE_PRICE_BY_SIZE[key] ?? null;
}

/** UI: always show full catalog price when size is known; otherwise fall back to stored. */
export function displayCatalogListPrice(appointment) {
  const base = basePriceForDogSize(appointment?.dogSize);
  if (base != null) return base;
  const stored = Number(appointment?.price);
  return Number.isFinite(stored) ? stored : "—";
}

/** 0-based slot by ascending id (same as server): indices 0–2 full price; 3 = fourth booking = first with 10% off. */
export const FIRST_LOYALTY_SLOT_INDEX = 3;

/** Fewer than this many total bookings → no loyalty UI when slot index is unknown (stale DB guard). */
const MIN_TOTAL_APPOINTMENTS_FOR_LOYALTY = 4;

/** True when DB charge is the 10%-off catalog price for this dog size. */
export function hasLoyaltyTenPercentCharge(appointment) {
  const base = basePriceForDogSize(appointment?.dogSize);
  const stored = Number(appointment?.price);
  if (base == null || Number.isNaN(stored)) return false;
  const discounted = Math.round(base * 0.9 * 100) / 100;
  return Math.abs(stored - discounted) < 0.03;
}

/** 10% off catalog for known dog size; otherwise stored price. */
export function loyaltySlotDiscountedCatalogPrice(appointment) {
  const base = basePriceForDogSize(appointment?.dogSize);
  if (base == null) {
    const stored = Number(appointment?.price);
    return Number.isFinite(stored) ? stored : "—";
  }
  return Math.round(base * 0.9 * 100) / 100;
}

/**
 * @param {object} [ctx]
 * @param {boolean} [ctx.isMine] — logged-in user owns this row
 * @param {number|undefined} [ctx.mineSlotIndex] — 0-based index in getAll() sorted by id (authoritative for “my” rows)
 * @param {number|undefined} [ctx.mineTotalAppointments] — total count from getAll(); used when slot unknown
 */
export function loyaltyUiApplies(appointment, ctx = {}) {
  const { isMine, mineSlotIndex, mineTotalAppointments } = ctx;
  if (isMine === true && typeof mineSlotIndex === "number") {
    return mineSlotIndex >= FIRST_LOYALTY_SLOT_INDEX;
  }
  if (
    isMine === true &&
    typeof mineTotalAppointments === "number" &&
    mineTotalAppointments < MIN_TOTAL_APPOINTMENTS_FOR_LOYALTY
  ) {
    return false;
  }
  return hasLoyaltyTenPercentCharge(appointment);
}

/** Main “Price” line: amount due when loyalty applies; otherwise catalog (or stored if size unknown). */
export function cardPrincipalPrice(appointment, ctx = {}) {
  const { isMine, mineSlotIndex } = ctx;
  if (isMine === true && typeof mineSlotIndex === "number") {
    return mineSlotIndex < FIRST_LOYALTY_SLOT_INDEX
      ? displayCatalogListPrice(appointment)
      : loyaltySlotDiscountedCatalogPrice(appointment);
  }
  if (loyaltyUiApplies(appointment, ctx)) {
    const stored = Number(appointment?.price);
    return Number.isFinite(stored) ? stored : "—";
  }
  return displayCatalogListPrice(appointment);
}

export function formatShekelUi(value) {
  if (value === "—") return "—";
  const n = Number(value);
  return Number.isFinite(n)
    ? n.toLocaleString("en-US", { maximumFractionDigits: 2 })
    : "—";
}
