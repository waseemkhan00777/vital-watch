"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { Badge } from "@/components/ui/Badge";
import { alertsApi } from "@/lib/api";

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
  });
}

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

function getSlaStatus(slaDueAt: string) {
  const now = new Date();
  const due = new Date(slaDueAt);
  const minsLeft = (due.getTime() - now.getTime()) / (60 * 1000);
  if (minsLeft < 0) return { label: "SLA breached", severity: "critical" as const };
  if (minsLeft < 30) return { label: `${Math.round(minsLeft)}m left`, severity: "high" as const };
  return { label: `Due ${formatTime(slaDueAt)}`, severity: "neutral" as const };
}

export default function ClinicianAlertsPage() {
  const [filter, setFilter] = useState<"all" | "critical" | "high">("all");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [note, setNote] = useState("");
  const queryClient = useQueryClient();

  const { data: alerts = [], isLoading, error } = useQuery({
    queryKey: ["alerts", "clinician"],
    queryFn: () => alertsApi.list({ limit: 100 }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, state, clinicalNote }: { id: string; state?: string; clinicalNote?: string }) =>
      alertsApi.update(id, { state, clinicalNote }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["alerts"] });
      setNote("");
    },
  });

  const filteredAlerts = alerts.filter(
    (a) => filter === "all" || a.severity === filter
  );
  const selected = selectedId
    ? alerts.find((a) => a.id === selectedId)
    : null;

  return (
    <>
      <PageHeader
        title="Alerts"
        description="Assigned patients only. Acknowledge and resolve within SLA."
      />

      <div className="mb-6 flex flex-wrap gap-2">
        {(["all", "critical", "high"] as const).map((f) => (
          <button
            key={f}
            type="button"
            onClick={() => setFilter(f)}
            className={`rounded-xl border px-3.5 py-2 text-sm font-medium capitalize transition-all duration-200 ${
              filter === f
                ? "border-primary-500 bg-primary-50 text-primary-700 shadow-sm"
                : "border-slate-300 bg-white text-slate-600 hover:border-slate-400 hover:bg-slate-50"
            }`}
          >
            {f}
          </button>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="animate-fade-in-up opacity-0" style={{ animationDelay: "60ms", animationFillMode: "forwards" }}>
          <CardHeader
            title="Alert queue"
            description="Priority by severity; time to SLA breach shown."
          />
          {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
          {error && (
            <p className="text-sm text-red-600">
              {error instanceof Error ? error.message : "Failed to load alerts."}
            </p>
          )}
          {!isLoading && !error && (
            <div className="space-y-3">
              {filteredAlerts.length === 0 ? (
                <p className="text-sm text-slate-500">No alerts.</p>
              ) : (
                filteredAlerts.map((a, i) => {
                  const sla = getSlaStatus(a.slaDueAt);
                  return (
                    <div
                      key={a.id}
                      className={`cursor-pointer rounded-xl border p-4 transition-all duration-200 ${
                        selectedId === a.id
                          ? "border-primary-500 bg-primary-50/50 shadow-sm ring-1 ring-primary-500/20"
                          : "border-slate-200 hover:border-slate-300 hover:bg-slate-50/80"
                      }`}
                      style={{
                        animation: "fade-in-up 0.4s ease-out forwards",
                        animationDelay: `${100 + i * 50}ms`,
                        opacity: 0,
                      }}
                      onClick={() => setSelectedId(a.id)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter" || e.key === " ") {
                          e.preventDefault();
                          setSelectedId(a.id);
                        }
                      }}
                      role="button"
                      tabIndex={0}
                    >
                      <div className="flex flex-wrap items-start justify-between gap-2">
                        <div>
                          <p className="font-medium text-slate-900">{a.patientName ?? a.patientId}</p>
                          <p className="text-sm text-slate-600">
                            {a.vitalType} {formatValue(a)}
                          </p>
                          <p className="text-xs text-slate-500">
                            {formatDate(a.createdAt)}
                          </p>
                        </div>
                        <div className="flex flex-wrap gap-1.5">
                          <Badge severity={a.severity as "critical" | "high" | "moderate" | "normal"}>{a.severity}</Badge>
                          <Badge severity={sla.severity}>{sla.label}</Badge>
                          <Badge severity="neutral">{a.state}</Badge>
                        </div>
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          )}
        </Card>

        <Card className="animate-fade-in-up opacity-0" style={{ animationDelay: "120ms", animationFillMode: "forwards" }}>
          <CardHeader
            title="Alert detail"
            description="Add note, acknowledge, or mark resolved."
          />
          {selected ? (
            <div className="animate-slide-in-right space-y-4">
              <div>
                <p className="text-sm font-medium text-slate-700">
                  {selected.patientName ?? selected.patientId} · {selected.vitalType}
                </p>
                <p className="text-lg font-semibold text-slate-900">
                  {formatValue(selected)} {selected.unit}
                </p>
                <p className="text-xs text-slate-500">
                  Flagged {formatDate(selected.createdAt)} · SLA due{" "}
                  {formatDate(selected.slaDueAt)}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">
                  Clinical note
                </label>
                <textarea
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                  placeholder="Add note..."
                  className="input-field mt-1 min-h-[80px]"
                  rows={3}
                />
              </div>
              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  className="btn-primary"
                  disabled={updateMutation.isPending}
                  onClick={() =>
                    updateMutation.mutate({
                      id: selected.id,
                      state: "acknowledged",
                      clinicalNote: note || undefined,
                    })
                  }
                >
                  Acknowledge
                </button>
                <button
                  type="button"
                  className="btn-secondary"
                  disabled={updateMutation.isPending}
                  onClick={() =>
                    updateMutation.mutate({
                      id: selected.id,
                      state: "resolved",
                      clinicalNote: note || undefined,
                    })
                  }
                >
                  Mark resolved
                </button>
              </div>
            </div>
          ) : (
            <p className="text-sm text-slate-500">
              Select an alert to view details and take action.
            </p>
          )}
        </Card>
      </div>
    </>
  );
}
