import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

// Demo data for frontend
const MOCK_READINGS = [
  {
    id: "1",
    type: "blood_pressure",
    label: "Blood pressure",
    value: "128/82",
    unit: "mmHg",
    recordedAt: "2025-03-02T14:30:00Z",
    source: "manual",
  },
  {
    id: "2",
    type: "heart_rate",
    label: "Heart rate",
    value: "72",
    unit: "bpm",
    recordedAt: "2025-03-02T14:30:00Z",
    source: "manual",
  },
  {
    id: "3",
    type: "blood_glucose",
    label: "Blood glucose",
    value: "112",
    unit: "mg/dL",
    recordedAt: "2025-03-01T08:00:00Z",
    source: "manual",
  },
];

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
    timeStyle: "short",
  });
}

export default function PatientHistoryPage() {
  return (
    <>
      <PageHeader
        title="Vitals history"
        description="Your recorded vital signs. Export available for your records."
      />
      <Card>
        <CardHeader
          title="Readings"
          description="Chronological list of submitted vitals. Export as JSON (patient data download) coming from API."
        />
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-left text-slate-600">
                <th className="pb-3 font-medium">Type</th>
                <th className="pb-3 font-medium">Value</th>
                <th className="pb-3 font-medium">Recorded</th>
                <th className="pb-3 font-medium">Source</th>
              </tr>
            </thead>
            <tbody>
              {MOCK_READINGS.map((r) => (
                <tr
                  key={r.id}
                  className="border-b border-slate-100 transition-colors hover:bg-slate-50/80"
                >
                  <td className="py-3 text-slate-900">{r.label}</td>
                  <td className="py-3 font-medium">
                    {r.value} <span className="text-slate-500">{r.unit}</span>
                  </td>
                  <td className="py-3 text-slate-600">
                    {formatDate(r.recordedAt)}
                  </td>
                  <td className="py-3 capitalize text-slate-600">{r.source}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <p className="mt-4 text-xs text-slate-500">
          Download health data (JSON export) will appear here once API is
          connected.
        </p>
      </Card>
    </>
  );
}
