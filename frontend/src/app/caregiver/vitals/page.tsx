import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";

export default function CaregiverVitalsPage() {
  return (
    <>
      <PageHeader
        title="Vitals (view only)"
        description="Read-only. No modifications or alert actions."
      />
      <Card>
        <CardHeader
          title="Patient vitals"
          description="Consent-based. Caregiver actions logged separately."
        />
        <p className="text-sm text-slate-600">
          Vitals for consented patients will appear here when backend is
          connected.
        </p>
      </Card>
    </>
  );
}
