import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

const MOCK_PATIENTS = [
  { id: "1", name: "John Patient", lastVital: "2025-03-02", activeAlerts: 1 },
  { id: "2", name: "Jane Doe", lastVital: "2025-03-01", activeAlerts: 0 },
];

export default function ClinicianPatientsPage() {
  return (
    <>
      <PageHeader
        title="Patients"
        description="Assigned patients only. Role-based filtering enforced at API."
      />
      <Card>
        <CardHeader
          title="Patient list"
          description="Assigned to you for monitoring."
        />
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-left text-slate-600">
                <th className="pb-3 font-medium">Name</th>
                <th className="pb-3 font-medium">Last vital</th>
                <th className="pb-3 font-medium">Active alerts</th>
              </tr>
            </thead>
            <tbody>
              {MOCK_PATIENTS.map((p) => (
                <tr
                  key={p.id}
                  className="border-b border-slate-100 transition-colors hover:bg-slate-50/80"
                >
                  <td className="py-3 font-medium text-slate-900">{p.name}</td>
                  <td className="py-3 text-slate-600">{p.lastVital}</td>
                  <td className="py-3">
                    {p.activeAlerts > 0 ? (
                      <span className="text-amber-600">{p.activeAlerts}</span>
                    ) : (
                      <span className="text-slate-500">0</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </>
  );
}
