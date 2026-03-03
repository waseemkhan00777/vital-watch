import Link from "next/link";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function AdminDashboardPage() {
  return (
    <>
      <PageHeader
        title="Admin dashboard"
        description="User management, alert thresholds, and audit logs."
      />
      <div className="grid gap-6 sm:grid-cols-2">
        <Card
          hoverLift
          className="animate-fade-in-up opacity-0"
          style={{ animationDelay: "50ms", animationFillMode: "forwards" }}
        >
          <CardHeader
            title="Users"
            description="Create and manage users (RBAC)."
            action={
              <Link href="/admin/users" className="btn-secondary text-sm">
                Manage users
              </Link>
            }
          />
        </Card>
        <Card
          hoverLift
          className="animate-fade-in-up opacity-0"
          style={{ animationDelay: "120ms", animationFillMode: "forwards" }}
        >
          <CardHeader
            title="Alert thresholds"
            description="Configure rule-based thresholds (e.g. systolic &gt; 180 → Critical)."
            action={
              <Link href="/admin/thresholds" className="btn-secondary text-sm">
                Configure
              </Link>
            }
          />
        </Card>
        <Card
          hoverLift
          className="sm:col-span-2 animate-fade-in-up opacity-0"
          style={{ animationDelay: "190ms", animationFillMode: "forwards" }}
        >
          <CardHeader
            title="Audit logs"
            description="System-wide audit trail. Immutable logs."
            action={
              <Link href="/admin/audit" className="btn-secondary text-sm">
                View logs
              </Link>
            }
          />
        </Card>
      </div>
    </>
  );
}
