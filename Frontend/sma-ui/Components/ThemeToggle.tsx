"use client";

import { useEffect, useState } from "react";

const themeKey = "sma-theme";

export default function ThemeToggle() {
  const [dark, setDark] = useState(false);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const initializeTheme = window.setTimeout(() => {
      const savedTheme = window.localStorage.getItem(themeKey);
      const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
      const shouldUseDark = savedTheme ? savedTheme === "dark" : prefersDark;

      document.documentElement.classList.toggle("dark", shouldUseDark);
      setDark(shouldUseDark);
      setReady(true);
    }, 0);

    return () => window.clearTimeout(initializeTheme);
  }, []);

  function toggleTheme() {
    const nextDark = !dark;
    document.documentElement.classList.toggle("dark", nextDark);
    window.localStorage.setItem(themeKey, nextDark ? "dark" : "light");
    setDark(nextDark);
  }

  return (
    <button
      type="button"
      onClick={toggleTheme}
      disabled={!ready}
      aria-label={dark ? "Switch to light mode" : "Switch to dark mode"}
      title={dark ? "Switch to light mode" : "Switch to dark mode"}
      className="fixed right-4 top-4 z-50 flex h-10 w-10 items-center justify-center rounded-full border border-slate-300 bg-white text-lg text-slate-700 shadow-sm transition hover:bg-slate-100 disabled:cursor-wait dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100 dark:hover:bg-slate-700"
    >
      <span aria-hidden="true">{dark ? "☀" : "☾"}</span>
    </button>
  );
}
