/** Claim type used by the API (`JwtTokenProvider`: `ClaimTypes.Name`). */
const CLAIM_NAME =
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

function base64UrlToUtf8String(base64Url) {
  const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
  const pad = (4 - (base64.length % 4)) % 4;
  const padded = base64 + (pad ? "=".repeat(pad) : "");
  const binary = atob(padded);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return new TextDecoder("utf-8").decode(bytes);
}

/**
 * @param {string | null | undefined} token
 * @returns {Record<string, unknown> | null}
 */
export function parseJwtPayload(token) {
  if (!token || typeof token !== "string") return null;
  const parts = token.split(".");
  if (parts.length < 2) return null;
  try {
    const json = base64UrlToUtf8String(parts[1]);
    return JSON.parse(json);
  } catch {
    return null;
  }
}

/**
 * Username as issued in the JWT (same source the API uses for ownership).
 * @param {string | null | undefined} token
 * @returns {string | null}
 */
export function usernameFromAccessToken(token) {
  const p = parseJwtPayload(token);
  if (!p || typeof p !== "object") return null;
  const raw =
    p.username ??
    p[CLAIM_NAME] ??
    p.unique_name ??
    p.name ??
    p.preferred_username ??
    p.sub;
  if (raw == null) return null;
  const s = String(raw).trim();
  return s.length ? s : null;
}
