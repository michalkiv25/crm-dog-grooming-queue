/** שם כלב: רק אותיות, רווחים, מקף ואפוסטרופ */
export const sanitizeDogNameInput = (value) =>
  value.replace(/[^\p{L}\s'\-]/gu, "");
