import Link from "next/link";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function PatientDashboardPage() {
  return (
    <>
      <PageHeader
        title="Dashboard"
        description="Overview of your vitals and recent alerts."
      />
      <div className="grid gap-6 sm:grid-cols-2">
        <Card
          hoverLift
          className="animate-fade-in-up opacity-0"
          style={{ animationDelay: "50ms", animationFillMode: "forwards" }}
        >
          <CardHeader
            title="Submit vitals"
            description="Record blood pressure, heart rate, glucose, and more."
            action={
              <Link href="/patient/vitals" className="btn-primary text-sm">
                Record vitals
              </Link>
            }
          />
          <p className="text-sm text-slate-600">
            Last submitted: — (demo). Submit regularly for the best monitoring.
          </p>
        </Card>
        <Card
          hoverLift
          className="animate-fade-in-up opacity-0"
          style={{ animationDelay: "120ms", animationFillMode: "forwards" }}
        >
          <CardHeader
            title="Alerts"
            description="View any alerts from your readings."
            action={
              <Link href="/patient/alerts" className="btn-secondary text-sm">
                View alerts
              </Link>
            }
          />
          <p className="text-sm text-slate-600">
            No active alerts. Your care team will see any flagged readings.
          </p>
        </Card>
        <Card
          hoverLift
          className="sm:col-span-2 animate-fade-in-up opacity-0"
          style={{ animationDelay: "190ms", animationFillMode: "forwards" }}
        >
          <CardHeader
            title="Quick actions"
            description="Health data and preferences"
          />
          <div className="flex flex-wrap gap-3">
            <Link
              href="/patient/history"
              className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 transition-all duration-200 hover:border-primary-200 hover:bg-primary-50/50 hover:text-primary-700"
            >
              View history
            </Link>
            <Link
              href="/patient/caregivers"
              className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 transition-all duration-200 hover:border-primary-200 hover:bg-primary-50/50 hover:text-primary-700"
            >
              Manage caregiver access
            </Link>
            <Link
              href="/patient/audit"
              className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 transition-all duration-200 hover:border-primary-200 hover:bg-primary-50/50 hover:text-primary-700"
            >
              Access log
            </Link>
          </div>
        </Card>
      </div>
    </>
  );
}
