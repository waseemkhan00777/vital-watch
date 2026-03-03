import Link from "next/link";

export default function HomePage() {
  return (
    <div className="min-h-screen bg-[#f0fdfa]">
      {/* Subtle gradient orbs */}
      <div className="pointer-events-none fixed inset-0 overflow-hidden">
        <div className="absolute -left-40 -top-40 h-80 w-80 rounded-full bg-primary-200/40 blur-3xl" />
        <div className="absolute right-0 top-1/3 h-96 w-96 rounded-full bg-primary-100/50 blur-3xl" />
        <div className="absolute bottom-0 left-1/2 h-64 w-96 -translate-x-1/2 rounded-full bg-primary-50/60 blur-3xl" />
      </div>

      <header className="relative border-b border-slate-200/60 bg-white/70 backdrop-blur-md">
        <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
          <div className="flex items-center gap-2.5">
            <div
              className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary-500 text-white shadow-sm transition-transform duration-300 hover:scale-105"
              aria-hidden
            >
              <svg
                className="h-5 w-5"
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
            <span className="text-xl font-semibold tracking-tight text-slate-900">
              VitalWatch
            </span>
          </div>
          <nav className="flex items-center gap-1">
            <Link
              href="/login"
              className="rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition-colors hover:text-primary-600 hover:bg-primary-50/50"
            >
              Sign in
            </Link>
            <Link
              href="/login"
              className="btn-primary rounded-xl text-sm"
            >
              Get started
            </Link>
          </nav>
        </div>
      </header>

      <main className="relative mx-auto max-w-6xl px-4 py-20 sm:px-6 sm:py-28">
        <div className="mx-auto max-w-3xl text-center">
          <h1
            className="animate-fade-in-up text-4xl font-bold tracking-tight text-slate-900 sm:text-5xl lg:text-6xl"
            style={{ animationFillMode: "both" }}
          >
            Remote patient monitoring that keeps care in focus
          </h1>
          <p
            className="mt-5 animate-fade-in-up text-lg text-slate-600 sm:text-xl"
            style={{ animationDelay: "120ms", animationFillMode: "both" }}
          >
            VitalWatch helps clinicians monitor vitals, respond to alerts within
            SLA, and keeps every action auditable—HIPAA-aligned and built for
            clinical accountability.
          </p>
          <div
            className="mt-10 flex flex-wrap items-center justify-center gap-3 animate-fade-in-up"
            style={{ animationDelay: "220ms", animationFillMode: "both" }}
          >
            <Link
              href="/login"
              className="btn-primary rounded-xl px-6 py-3 text-base"
            >
              Sign in to VitalWatch
            </Link>
            <Link
              href="/login"
              className="btn-secondary rounded-xl px-6 py-3 text-base"
            >
              Demo: Patient portal
            </Link>
          </div>
        </div>

        <div className="mt-24 grid gap-6 sm:grid-cols-3 sm:gap-8">
          {[
            {
              icon: "M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z",
              title: "Vitals & alerts",
              desc: "Submit vitals, rule-based thresholds, and SLA-driven escalation.",
            },
            {
              icon: "M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z",
              title: "HIPAA-aligned",
              desc: "Access control, audit logs, and encryption in transit and at rest.",
            },
            {
              icon: "M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z",
              title: "Caregiver access",
              desc: "Consent-based proxy access; patients control who sees their data.",
            },
          ].map((item, i) => (
            <div
              key={item.title}
              className="card-clinical card-hover-lift animate-fade-in-up p-6 text-center opacity-0"
              style={{
                animationDelay: `${320 + i * 80}ms`,
                animationFillMode: "forwards",
              }}
            >
              <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-primary-100 to-primary-50 text-primary-600 shadow-sm transition-transform duration-300 hover:scale-105">
                <svg
                  className="h-7 w-7"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d={item.icon}
                  />
                </svg>
              </div>
              <h3 className="font-semibold text-slate-900">{item.title}</h3>
              <p className="mt-2 text-sm leading-relaxed text-slate-600">
                {item.desc}
              </p>
            </div>
          ))}
        </div>
      </main>

      <footer className="relative border-t border-slate-200/80 bg-white/80 py-8 backdrop-blur-sm">
        <div className="mx-auto max-w-6xl px-4 text-center text-sm text-slate-500 sm:px-6">
          VitalWatch – HIPAA-aligned Remote Patient Monitoring. For demo use.
        </div>
      </footer>
    </div>
  );
}
