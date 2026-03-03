"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import type { Role } from "@/lib/types";

const ROLES: { value: Role; label: string }[] = [
  { value: "patient", label: "Patient" },
  { value: "clinician", label: "Clinician / Nurse" },
  { value: "admin", label: "Admin" },
  { value: "caregiver", label: "Caregiver" },
];

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<Role>("patient");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login(email || "demo@vitalwatch.demo", password, role);
    const redirect =
      role === "admin"
        ? "/admin"
        : role === "clinician"
          ? "/clinician"
          : role === "caregiver"
            ? "/caregiver"
            : "/patient";
    router.push(redirect);
  };

  return (
    <div className="flex min-h-screen flex-col bg-[#f0fdfa]">
      <div className="pointer-events-none fixed inset-0 overflow-hidden">
        <div className="absolute -left-32 top-0 h-72 w-72 rounded-full bg-primary-200/30 blur-3xl" />
        <div className="absolute bottom-0 right-0 h-80 w-80 rounded-full bg-primary-100/40 blur-3xl" />
      </div>

      <header className="relative border-b border-slate-200/60 bg-white/70 backdrop-blur-md">
        <div className="mx-auto flex h-14 max-w-6xl items-center px-4 sm:px-6">
          <Link
            href="/"
            className="flex items-center gap-2 transition-opacity hover:opacity-90"
          >
            <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-primary-500 text-white shadow-sm">
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
      </header>

      <main className="relative flex flex-1 items-center justify-center px-4 py-12">
        <div
          className="w-full max-w-md animate-scale-in opacity-0"
          style={{ animationFillMode: "forwards" }}
        >
          <div className="card-clinical overflow-hidden p-8 shadow-elevated">
            <h1 className="text-2xl font-semibold tracking-tight text-slate-900">
              Sign in
            </h1>
            <p className="mt-1.5 text-sm text-slate-600">
              Choose your role to enter the demo portal.
            </p>

            <form onSubmit={handleSubmit} className="mt-6 space-y-5">
              <div className="space-y-1.5">
                <label
                  htmlFor="email"
                  className="block text-sm font-medium text-slate-700"
                >
                  Email
                </label>
                <input
                  id="email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@example.com"
                  className="input-field mt-1"
                  autoComplete="email"
                />
              </div>
              <div className="space-y-1.5">
                <label
                  htmlFor="password"
                  className="block text-sm font-medium text-slate-700"
                >
                  Password
                </label>
                <input
                  id="password"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="input-field mt-1"
                  autoComplete="current-password"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">
                  Sign in as
                </label>
                <div className="mt-2 flex flex-wrap gap-2">
                  {ROLES.map((r) => (
                    <button
                      key={r.value}
                      type="button"
                      onClick={() => setRole(r.value)}
                      className={`rounded-xl border px-3.5 py-2 text-sm font-medium transition-all duration-200 ${
                        role === r.value
                          ? "border-primary-500 bg-primary-50 text-primary-700 shadow-sm"
                          : "border-slate-300 bg-white text-slate-600 hover:border-slate-400 hover:bg-slate-50"
                      }`}
                    >
                      {r.label}
                    </button>
                  ))}
                </div>
              </div>
              <button type="submit" className="btn-primary w-full">
                Continue
              </button>
            </form>

            <p className="mt-6 text-center text-xs text-slate-500">
              Demo: no password required. Role determines dashboard.
            </p>
          </div>
        </div>
      </main>
    </div>
  );
}
