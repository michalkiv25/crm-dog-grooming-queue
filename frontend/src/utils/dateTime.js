function pad2(n) {
  return String(n).padStart(2, "0");
}

/** Local wall-time ISO without timezone marker (e.g. 2026-05-08T21:30:00). */
export function toLocalIsoWithoutZone(date) {
  if (!(date instanceof Date) || Number.isNaN(date.getTime())) return "";
  return `${date.getFullYear()}-${pad2(date.getMonth() + 1)}-${pad2(date.getDate())}T${pad2(date.getHours())}:${pad2(date.getMinutes())}:00`;
}

/** Ensure datetime-local value includes seconds for consistent server parsing. */
export function withSeconds(localDateTime) {
  if (typeof localDateTime !== "string") return "";
  const trimmed = localDateTime.trim();
  if (!trimmed) return "";
  return trimmed.length === 16 ? `${trimmed}:00` : trimmed;
}
