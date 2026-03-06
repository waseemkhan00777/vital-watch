namespace VitalCare.Abp;

public static class VitalCareAbpConstants
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Clinician = "clinician";
        public const string Patient = "patient";
        public const string Caregiver = "caregiver";
    }

    public static class VitalTypes
    {
        public const string BloodPressure = "blood_pressure";
        public const string HeartRate = "heart_rate";
        public const string BloodGlucose = "blood_glucose";
        public const string OxygenSaturation = "oxygen_saturation";
        public const string Weight = "weight";
    }

    public static class AlertRuleOperators
    {
        public const string Above = "above";
        public const string Below = "below";
        public const string Between = "between";
    }

    public static class AlertSeverity
    {
        public const string Critical = "critical";
        public const string High = "high";
        public const string Moderate = "moderate";
        public const string Normal = "normal";
    }

    public static class AlertState
    {
        public const string Flagged = "flagged";
        public const string Acknowledged = "acknowledged";
        public const string Escalated = "escalated";
        public const string Resolved = "resolved";
        public const string Archived = "archived";
    }
}
