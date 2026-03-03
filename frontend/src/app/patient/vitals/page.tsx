"use client";

import { useState } from "react";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { VitalForm } from "@/components/vitals/VitalForm";

export default function SubmitVitalsPage() {
  const [submitted, setSubmitted] = useState(false);

  return (
    <>
      <PageHeader
        title="Submit vitals"
        description="Record your vital signs. All entries are logged and visible to your care team."
      />
      <Card className="animate-fade-in-up opacity-0" style={{ animationFillMode: "forwards" }}>
        <CardHeader
          title="New reading"
          description="Enter values and optional timestamp. Source will be recorded as manual."
        />
        {submitted ? (
          <div className="animate-scale-in rounded-xl bg-emerald-50 p-4 text-sm text-emerald-800 ring-1 ring-emerald-200/60">
            Reading recorded. It will appear in your history and any alerts will
            be evaluated by your care team.
          </div>
        ) : (
          <VitalForm onSuccess={() => setSubmitted(true)} />
        )}
      </Card>
    </>
  );
}
