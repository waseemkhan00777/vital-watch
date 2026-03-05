"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import type { Role } from "@/lib/types";

const PATIENT_NAV = [
  { href: "/patient", label: "Dashboard" },
  { href: "/patient/vitals", label: "Submit vitals" },
  { href: "/patient/history", label: "History" },
  { href: "/patient/alerts", label: "Alerts" },
  { href: "/patient/caregivers", label: "Caregiver access" },
  { href: "/patient/audit", label: "Audit log" },
];

const CLINICIAN_NAV = [
  { href: "/clinician", label: "Alerts" },
  { href: "/clinician/patients", label: "Patients" },
  { href: "/clinician/vitals", label: "Vitals" },
];

const ADMIN_NAV = [
  { href: "/admin", label: "Dashboard" },
  { href: "/admin/users", label: "Users" },
  { href: "/admin/thresholds", label: "Alert thresholds" },
  { href: "/admin/audit", label: "Audit logs" },
];

const CAREGIVER_NAV = [
  { href: "/caregiver", label: "Dashboard" },
  { href: "/caregiver/vitals", label: "Vitals (view only)" },
  { href: "/caregiver/alerts", label: "Alerts (view only)" },
];

function getNav(role: Role): { href: string; label: string }[] {
  switch (role) {
    case "patient":
      return PATIENT_NAV;
    case "clinician":
      return CLINICIAN_NAV;
    case "admin":
      return ADMIN_NAV;
    case "caregiver":
      return CAREGIVER_NAV;
    default:
      return [];
  }
}

export function AppShell({
  children,
  role,
}: {
  children: React.ReactNode;
  role: Role;
}) {
  const pathname = usePathname();
  const router = useRouter();
  const { user, logout } = useAuth();
  const nav = getNav(role);

  const handleLogout = () => {
    logout().then(() => router.push("/login"));
  };

  return (
    <div className="flex min-h-screen bg-surface-paper">
      <aside className="flex w-56 shrink-0 flex-col border-r border-slate-200/80 bg-white shadow-sm">
        <div className="flex h-16 items-center gap-2 border-b border-slate-200/80 px-4">
          <Link
            href="/"
            className="flex items-center gap-2.5 transition-opacity hover:opacity-90"
          >
            <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-primary-500 text-white shadow-sm transition-transform duration-200 hover:scale-105">
              <svg
                className="h-4 w-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
                />
              </svg>
            </div>
            <span className="font-semibold text-slate-900">VitalWatch</span>
          </Link>
        </div>
        <nav className="flex-1 overflow-auto p-2">
          {nav.map((item) => {
            const active = pathname === item.href;
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`block rounded-xl px-3 py-2.5 text-sm font-medium transition-all duration-200 ${
                  active
                    ? "bg-primary-50 text-primary-700 shadow-sm"
                    : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
                }`}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>
        <div className="mt-auto border-t border-slate-200/80 bg-slate-50/50 p-3">
          <p className="truncate px-2 py-1 text-xs font-medium text-slate-600">
            {user?.name}
          </p>
          <p className="truncate px-2 py-0.5 text-xs capitalize text-slate-400">
            {user?.role}
          </p>
          <button
            type="button"
            onClick={handleLogout}
            className="mt-2 w-full rounded-xl px-3 py-2 text-left text-sm text-slate-600 transition-colors hover:bg-slate-100"
          >
            Sign out
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-auto">
        <div
          key={pathname}
          className="mx-auto max-w-5xl p-6 pb-24 animate-fade-in"
        >
          {children}
        </div>
      </main>
    </div>
  );
}
