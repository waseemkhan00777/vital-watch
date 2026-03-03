import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function CaregiversPage() {
  return (
    <>
      <PageHeader
        title="Caregiver access"
        description="Grant or revoke consent for caregivers to view your vitals. All actions are logged."
      />
      <Card>
        <CardHeader
          title="Consent-based access"
          description="Caregivers can only view your vitals when you grant access. Revocation takes effect immediately."
        />
        <p className="text-sm text-slate-600">
          No caregivers linked. When you add a caregiver, they will see
          read-only vitals; they cannot modify data or acknowledge alerts.
        </p>
        <div className="mt-4 rounded-lg border border-slate-200 bg-slate-50/50 p-4">
          <p className="text-sm font-medium text-slate-700">
            Add caregiver (demo)
          </p>
          <p className="mt-1 text-xs text-slate-500">
            Backend will provide invite flow and consent storage. Revocation
            will be logged in audit.
          </p>
        </div>
      </Card>
    </>
  );
}
