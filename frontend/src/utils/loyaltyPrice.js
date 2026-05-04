/** Align with backend Appointment.CalculatePriceAndDuration */
export const BASE_PRICE_BY_SIZE = {
  small: 100,
  medium: 150,
  large: 200,
};

/** JSON ids must normalize — JS Map distinguishes 12 from "12". */
export function appointmentNumericId(appointment) {
  const raw = appointment?.id ?? appointment?.Id;
  const n = Number(raw);
  return Number.isFinite(n) ? n : null;
}

export function basePriceForDogSize(dogSize) {
  if (!dogSize) return null;
  const key = String(dogSize).toLowerCase();
  return BASE_PRICE_BY_SIZE[key] ?? null;
}

/** List price from dog size when known; otherwise stored price from API. */
export function displayCatalogListPrice(appointment) {
  const base = basePriceForDogSize(appointment?.dogSize);
  if (base != null) return base;
  const stored = Number(appointment?.price);
  return Number.isFinite(stored) ? stored : "—";
}

/** 0-based slot by ascending appointment date, then id (same as server): indices 0–2 full price; 3+ = 10% off list. */
export const FIRST_LOYALTY_SLOT_INDEX = 3;

const MIN_TOTAL_APPOINTMENTS_FOR_LOYALTY = 4;

/** If stored price matches 10% off any catalog tier, return that tier’s list price (else null). */
function catalogListPriceIfStoredLooksLikeLoyaltyDiscount(appointment) {
  const stored = Number(appointment?.price);
  if (!Number.isFinite(stored)) return null;
  for (const catalog of Object.values(BASE_PRICE_BY_SIZE)) {
    const discounted = Math.round(catalog * 0.9 * 100) / 100;
    if (Math.abs(stored - discounted) < 0.03) return catalog;
  }
  return null;
}

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
 * @param {boolean} [ctx.isMine]
 * @param {number|undefined} [ctx.mineSlotIndex]
 * @param {number|undefined} [ctx.mineTotalAppointments]
 */
export function loyaltyUiApplies(appointment, ctx = {}) {
  const { isMine, mineSlotIndex, mineTotalAppointments } = ctx;

  if (
    isMine === true &&
    typeof mineTotalAppointments === "number" &&
    mineTotalAppointments < MIN_TOTAL_APPOINTMENTS_FOR_LOYALTY
  ) {
    return false;
  }

  if (isMine === true && typeof mineSlotIndex === "number") {
    return mineSlotIndex >= FIRST_LOYALTY_SLOT_INDEX;
  }

  if (isMine === true) {
    return false;
  }

  return hasLoyaltyTenPercentCharge(appointment);
}

/** Amount due: catalog or 10% off when loyalty slot applies. */
export function cardPrincipalPrice(appointment, ctx = {}) {
  const { isMine, mineSlotIndex, mineTotalAppointments } = ctx;
  if (
    isMine === true &&
    typeof mineTotalAppointments === "number" &&
    mineTotalAppointments < MIN_TOTAL_APPOINTMENTS_FOR_LOYALTY
  ) {
    const base = basePriceForDogSize(appointment?.dogSize);
    if (base != null) return base;
    const fromStoredGuess = catalogListPriceIfStoredLooksLikeLoyaltyDiscount(appointment);
    if (fromStoredGuess != null) return fromStoredGuess;
    return displayCatalogListPrice(appointment);
  }
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
