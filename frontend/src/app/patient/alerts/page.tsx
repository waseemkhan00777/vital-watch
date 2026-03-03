import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { Badge } from "@/components/ui/Badge";

const MOCK_ALERTS = [
  {
    id: "1",
    vitalType: "Blood pressure",
    severity: "high" as const,
    state: "resolved",
    value: "182/95",
    createdAt: "2025-03-01T10:00:00Z",
    resolvedAt: "2025-03-01T11:30:00Z",
  },
];

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
    timeStyle: "short",
  });
}

export default function PatientAlertsPage() {
  return (
    <>
      <PageHeader
        title="Alert history"
        description="Alerts generated from your vitals. Your care team can acknowledge and resolve these."
      />
      <Card>
        <CardHeader
          title="Alerts"
          description="View-only. Clinicians manage acknowledgment and resolution."
        />
        {MOCK_ALERTS.length === 0 ? (
          <p className="text-sm text-slate-600">
            No alerts on record. Any out-of-range readings will appear here.
          </p>
        ) : (
          <div className="space-y-4">
            {MOCK_ALERTS.map((a) => (
              <div
                key={a.id}
                className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-200 bg-slate-50/50 p-4 transition-colors hover:border-slate-300 hover:bg-slate-100/50"
              >
                <div>
                  <p className="font-medium text-slate-900">{a.vitalType}</p>
                  <p className="text-sm text-slate-600">
                    {a.value} · {formatDate(a.createdAt)}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge severity={a.severity}>{a.severity}</Badge>
                  <Badge severity="neutral">{a.state}</Badge>
                </div>
                {a.resolvedAt && (
                  <p className="w-full text-xs text-slate-500">
                    Resolved {formatDate(a.resolvedAt)}
                  </p>
                )}
              </div>
            ))}
          </div>
        )}
      </Card>
    </>
  );
}
