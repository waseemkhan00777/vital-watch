"use client";

import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { Badge } from "@/components/ui/Badge";
import { alertsApi } from "@/lib/api";
import type { AlertSeverity, AlertState } from "@/lib/types";

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

export default function PatientAlertsPage() {
  const { data: alerts = [], isLoading, error } = useQuery({
    queryKey: ["alerts", "patient"],
    queryFn: () => alertsApi.list({ limit: 100 }),
  });

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
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load alerts."}
          </p>
        )}
        {!isLoading && !error && (
          <>
            {alerts.length === 0 ? (
              <p className="text-sm text-slate-600">
                No alerts on record. Any out-of-range readings will appear here.
              </p>
            ) : (
              <div className="space-y-4">
                {alerts.map((a) => (
                  <div
                    key={a.id}
                    className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-200 bg-slate-50/50 p-4 transition-colors hover:border-slate-300 hover:bg-slate-100/50"
                  >
                    <div>
                      <p className="font-medium text-slate-900">{a.vitalType}</p>
                      <p className="text-sm text-slate-600">
                        {formatValue(a)} · {formatDate(a.createdAt)}
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge severity={a.severity as AlertSeverity}>
                        {a.severity}
                      </Badge>
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
          </>
        )}
      </Card>
    </>
  );
}
