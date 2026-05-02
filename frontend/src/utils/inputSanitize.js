/**
 * Dog name input: keep Unicode letters, spaces, hyphen, and apostrophe only.
 * Used while typing to strip digits and other symbols.
 */
export const sanitizeDogNameInput = (value) =>
  value.replace(/[^\p{L}\s'\-]/gu, "");
