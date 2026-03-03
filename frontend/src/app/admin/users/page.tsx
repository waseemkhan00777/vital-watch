import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function AdminUsersPage() {
  return (
    <>
      <PageHeader
        title="Users"
        description="Create users and assign roles. Enforcement at API and service layer."
      />
      <Card>
        <CardHeader
          title="User list"
          description="Admin only. Deny-by-default RBAC."
        />
        <p className="text-sm text-slate-600">
          User management UI will connect to backend. Roles: Admin, Clinician,
          Patient, Caregiver.
        </p>
      </Card>
    </>
  );
}
