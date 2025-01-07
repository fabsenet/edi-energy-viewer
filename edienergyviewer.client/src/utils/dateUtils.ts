import { DateTime, type DateTimeFormatOptions, type LocaleOptions, Settings } from "luxon";

const DATE: LocaleOptions & DateTimeFormatOptions = { year: "numeric", month: "2-digit", day: "2-digit" };
const TIME: LocaleOptions & DateTimeFormatOptions = { hour: "2-digit", minute: "2-digit", second: "2-digit" };
const DATETIME: LocaleOptions & DateTimeFormatOptions = { ...DATE, ...TIME };

Settings.defaultLocale = "de-de";

export const ZesFormats = { DATE, TIME, DATETIME };

export function formatDT(date: string | null | undefined): string {
  if (!date) return "";

  return DateTime.fromISO(date).toLocaleString(DATETIME);
}

export function formatD(date: string | null | undefined): string {
  if (!date) return "";

  return DateTime.fromISO(date).toLocaleString(DATE);
}

export function formatT(date: string | null | undefined): string {
  if (!date) return "";

  return DateTime.fromISO(date).toLocaleString(TIME);
}

export function formatCalendarDe(date: string | null | undefined): string {
  if (!date) return "";

  return DateTime.fromISO(date).toRelative() ?? "";
}

export function copyDateParts(sourceDateString: string, newDateString: string, elemsToCopy: "date" | "time"): string | null {
  let sourceDate = DateTime.fromISO(sourceDateString);
  const newDate = DateTime.fromISO(newDateString);

  if (elemsToCopy === "date") {
    sourceDate = sourceDate.set({ year: newDate.year, month: newDate.month, day: newDate.day });
  } else if (elemsToCopy === "time") {
    sourceDate = sourceDate.set({ hour: newDate.hour, minute: newDate.minute, second: newDate.second });
  }

  return sourceDate.toISO();
}
