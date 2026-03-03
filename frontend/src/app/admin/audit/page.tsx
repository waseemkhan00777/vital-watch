import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function AdminAuditPage() {
  return (
    <>
      <PageHeader
        title="Audit logs"
        description="System-wide audit trail. Immutable; all actions logged."
      />
      <Card>
        <CardHeader
          title="Audit log"
          description="User ID, role, resource, timestamp, IP, action, before/after."
        />
        <p className="text-sm text-slate-600">
          Audit log table will connect to backend. Patient has access to own
          access log only.
        </p>
      </Card>
    </>
  );
}
