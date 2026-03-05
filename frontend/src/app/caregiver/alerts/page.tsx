"use client";

import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { Badge } from "@/components/ui/Badge";
import { alertsApi } from "@/lib/api";

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
    timeStyle: "short",
  });
}

function formatValue(a: { value: number; valueSecondary?: number | null; unit: string; vitalType: string }) {
  if (a.vitalType === "blood_pressure" && a.valueSecondary != null)
    return `${a.value}/${a.valueSecondary}`;
  return `${a.value} ${a.unit}`;
}

export default function CaregiverAlertsPage() {
  const { data: alerts = [], isLoading, error } = useQuery({
    queryKey: ["alerts", "caregiver"],
    queryFn: () => alertsApi.list({ limit: 100 }),
  });

  return (
    <>
      <PageHeader
        title="Alerts (view only)"
        description="Alerts for consented patients. View-only; clinicians manage resolution."
      />
      <Card>
        <CardHeader
          title="Alerts"
          description="Read-only. You cannot acknowledge or resolve alerts."
        />
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load alerts."}
          </p>
        )}
        {!isLoading && !error && (
          <>
            {alerts.length === 0 ? (
              <p className="text-sm text-slate-600">No alerts for your consented patients.</p>
            ) : (
              <div className="space-y-4">
                {alerts.map((a) => (
                  <div
                    key={a.id}
                    className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-200 bg-slate-50/50 p-4"
                  >
                    <div>
                      <p className="font-medium text-slate-900">{a.patientName ?? a.patientId}</p>
                      <p className="text-sm text-slate-600">
                        {a.vitalType} {formatValue(a)} · {formatDate(a.createdAt)}
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge severity={a.severity as "critical" | "high" | "moderate" | "normal"}>
                        {a.severity}
                      </Badge>
                      <Badge severity="neutral">{a.state}</Badge>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </>
        )}
      </Card>
    </>
  );
}
