export const consentStorageKey = "boecl-consent";
export const consentChangeEvent = "boecl:consent-change";

export function hasOptionalConsent(storage: Pick<Storage, "getItem">) {
  return storage.getItem(consentStorageKey) === "granted";
}
