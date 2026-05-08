export function validateLoginInput({ username, password }) {
  const errors = [];

  if (!String(username ?? "").trim()) {
    errors.push("Username is required");
  }

  if (!String(password ?? "").trim()) {
    errors.push("Password is required");
  }

  return errors;
}

export function validateRegisterInput({ username, password, fullName }) {
  const errors = [];
  const usernameTrimmed = String(username ?? "").trim();
  const passwordTrimmed = String(password ?? "").trim();
  const fullNameTrimmed = String(fullName ?? "").trim();

  if (!usernameTrimmed) {
    errors.push("Username is required");
  } else if (usernameTrimmed.length < 3) {
    errors.push("Username must be at least 3 characters");
  } else if (usernameTrimmed.length > 30) {
    errors.push("Username must not exceed 30 characters");
  } else if (!/^[\p{L}]+$/u.test(usernameTrimmed)) {
    errors.push("Username must contain letters only");
  }

  if (!passwordTrimmed) {
    errors.push("Password is required");
  } else if (passwordTrimmed.length < 6) {
    errors.push("Password must be at least 6 characters");
  }

  if (!fullNameTrimmed) {
    errors.push("Full name is required");
  } else if (fullNameTrimmed.length < 2) {
    errors.push("Full name must be at least 2 characters");
  } else if (fullNameTrimmed.length > 100) {
    errors.push("Full name must not exceed 100 characters");
  } else if (!/^[\p{L}\s'\-]+$/u.test(fullNameTrimmed)) {
    errors.push("Full name must contain letters, spaces, hyphen or apostrophe only");
  }

  return errors;
}

export function sameBookingSlot(isoOrLocalA, isoOrLocalB) {
  if (isoOrLocalA == null || isoOrLocalB == null) return false;
  const ta = new Date(isoOrLocalA).getTime();
  const tb = new Date(isoOrLocalB).getTime();
  if (!Number.isFinite(ta) || !Number.isFinite(tb)) return false;
  return Math.abs(ta - tb) < 6 * 60 * 1000;
}

export function validateAppointmentInput({
  dogName,
  dogSize,
  date,
  originalDate,
}) {
  const errors = [];
  const dogNameTrimmed = String(dogName ?? "").trim();

  if (!dogNameTrimmed) {
    errors.push("Dog name is required");
  } else if (dogNameTrimmed.length < 2) {
    errors.push("Dog name must be at least 2 characters");
  } else if (dogNameTrimmed.length > 50) {
    errors.push("Dog name must not exceed 50 characters");
  } else if (!/^[\p{L}\s'\-]+$/u.test(dogNameTrimmed)) {
    errors.push("Dog name must contain letters only");
  }

  if (!dogSize) {
    errors.push("Dog size is required");
  } else if (!["small", "medium", "large"].includes(dogSize)) {
    errors.push("Invalid dog size selected");
  }

  if (!date) {
    errors.push("Appointment date is required");
  } else {
    const selectedDate = new Date(date);
    const isSameSlotAsOriginal =
      originalDate != null ? sameBookingSlot(date, originalDate) : false;

    if (!isSameSlotAsOriginal && selectedDate <= new Date()) {
      errors.push("Appointment date must be in the future");
    }
  }

  return errors;
}
