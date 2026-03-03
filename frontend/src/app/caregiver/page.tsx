import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function CaregiverDashboardPage() {
  return (
    <>
      <PageHeader
        title="Caregiver dashboard"
        description="View-only access to consented patient vitals. You cannot modify data or acknowledge alerts."
      />
      <Card>
        <CardHeader
          title="Consented patients"
          description="Patients who have granted you access. Read-only."
        />
        <p className="text-sm text-slate-600">
          When a patient adds you as caregiver, their vitals will appear here.
          Revocation by patient takes effect immediately and is logged.
        </p>
      </Card>
    </>
  );
}
