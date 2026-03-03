import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

const MOCK_AUDIT = [
  {
    id: "1",
    action: "vitals.submit",
    timestamp: "2025-03-02T14:30:00Z",
    resourceType: "vitals",
    ip: "192.168.1.1",
  },
  {
    id: "2",
    action: "auth.login",
    timestamp: "2025-03-02T14:00:00Z",
    resourceType: "session",
    ip: "192.168.1.1",
  },
];

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
    timeStyle: "short",
  });
}

export default function PatientAuditPage() {
  return (
    <>
      <PageHeader
        title="Access log"
        description="Your access log for transparency. All access to your data is recorded."
      />
      <Card>
        <CardHeader
          title="Audit log"
          description="Patient-visible access log per HIPAA alignment."
        />
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-left text-slate-600">
                <th className="pb-3 font-medium">Time</th>
                <th className="pb-3 font-medium">Action</th>
                <th className="pb-3 font-medium">Resource</th>
                <th className="pb-3 font-medium">IP</th>
              </tr>
            </thead>
            <tbody>
              {MOCK_AUDIT.map((r) => (
                <tr key={r.id} className="border-b border-slate-100">
                  <td className="py-3 text-slate-600">
                    {formatDate(r.timestamp)}
                  </td>
                  <td className="py-3 font-medium text-slate-900">{r.action}</td>
                  <td className="py-3 text-slate-600">{r.resourceType}</td>
                  <td className="py-3 text-slate-500">{r.ip}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </>
  );
}
