"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { alertRulesApi } from "@/lib/api";

export default function AdminThresholdsPage() {
  const queryClient = useQueryClient();
  const [vitalType, setVitalType] = useState("");
  const [severity, setSeverity] = useState("high");
  const [operator, setOperator] = useState("above");
  const [thresholdMin, setThresholdMin] = useState("");
  const [thresholdMax, setThresholdMax] = useState("");

  const { data: rules = [], isLoading, error } = useQuery({
    queryKey: ["alertrules"],
    queryFn: () => alertRulesApi.list({ activeOnly: false }),
  });

  const createMutation = useMutation({
    mutationFn: () =>
      alertRulesApi.create({
        vitalType: vitalType || "blood_pressure",
        severity,
        operator,
        thresholdMin: thresholdMin ? Number(thresholdMin) : undefined,
        thresholdMax: thresholdMax ? Number(thresholdMax) : undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["alertrules"] });
      setThresholdMin("");
      setThresholdMax("");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => alertRulesApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["alertrules"] }),
  });

  return (
    <>
      <PageHeader
        title="Alert thresholds"
        description="Rule-based thresholds. E.g. Systolic BP &gt; 180 → Critical."
      />
      <Card>
        <CardHeader
          title="Rules"
          description="Configurable by Admin. SLA: Critical 1h, High 4h."
        />
        <div className="mb-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
          <div>
            <label className="block text-sm font-medium text-slate-700">Vital type</label>
            <select
              value={vitalType}
              onChange={(e) => setVitalType(e.target.value)}
              className="input-field mt-1"
            >
              <option value="blood_pressure">blood_pressure</option>
              <option value="heart_rate">heart_rate</option>
              <option value="blood_glucose">blood_glucose</option>
              <option value="oxygen_saturation">oxygen_saturation</option>
              <option value="weight">weight</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Severity</label>
            <select
              value={severity}
              onChange={(e) => setSeverity(e.target.value)}
              className="input-field mt-1"
            >
              <option value="critical">critical</option>
              <option value="high">high</option>
              <option value="moderate">moderate</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Operator</label>
            <select
              value={operator}
              onChange={(e) => setOperator(e.target.value)}
              className="input-field mt-1"
            >
              <option value="above">above</option>
              <option value="below">below</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Min</label>
            <input
              type="number"
              value={thresholdMin}
              onChange={(e) => setThresholdMin(e.target.value)}
              className="input-field mt-1"
              placeholder="e.g. 180"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Max</label>
            <input
              type="number"
              value={thresholdMax}
              onChange={(e) => setThresholdMax(e.target.value)}
              className="input-field mt-1"
              placeholder="e.g. 90"
            />
          </div>
        </div>
        <button
          type="button"
          onClick={() => createMutation.mutate()}
          disabled={createMutation.isPending}
          className="btn-primary mb-6"
        >
          {createMutation.isPending ? "Adding…" : "Add rule"}
        </button>

        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load rules."}
          </p>
        )}
        {!isLoading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-left text-slate-600">
                  <th className="pb-3 font-medium">Vital type</th>
                  <th className="pb-3 font-medium">Severity</th>
                  <th className="pb-3 font-medium">Operator</th>
                  <th className="pb-3 font-medium">Min</th>
                  <th className="pb-3 font-medium">Max</th>
                  <th className="pb-3 font-medium">Active</th>
                  <th className="pb-3 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rules.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="py-4 text-slate-500">
                      No rules. Add one above.
                    </td>
                  </tr>
                ) : (
                  rules.map((r) => (
                    <tr
                      key={r.id}
                      className="border-b border-slate-100 transition-colors hover:bg-slate-50/80"
                    >
                      <td className="py-3 text-slate-900">{r.vitalType}</td>
                      <td className="py-3 text-slate-600">{r.severity}</td>
                      <td className="py-3 text-slate-600">{r.operator}</td>
                      <td className="py-3 text-slate-600">{r.thresholdMin ?? "—"}</td>
                      <td className="py-3 text-slate-600">{r.thresholdMax ?? "—"}</td>
                      <td className="py-3">{r.isActive ? "Yes" : "No"}</td>
                      <td className="py-3">
                        <button
                          type="button"
                          onClick={() => deleteMutation.mutate(r.id)}
                          disabled={deleteMutation.isPending}
                          className="text-red-600 hover:underline"
                        >
                          Deactivate
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </>
  );
}
