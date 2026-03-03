import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function AdminThresholdsPage() {
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
        <p className="text-sm text-slate-600">
          Threshold configuration UI will connect to backend alert_rules.
        </p>
      </Card>
    </>
  );
}
