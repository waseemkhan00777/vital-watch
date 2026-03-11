using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalCare.Abp.Migrations
{
    public partial class AddRowLevelSecurity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""VitalReadings"" ENABLE ROW LEVEL SECURITY;
                CREATE POLICY vital_readings_policy ON ""VitalReadings""
                    USING (
                        current_setting('app.user_role', true) IS NULL
                        OR current_setting('app.user_role', true) = 'admin'
                        OR (current_setting('app.user_role', true) = 'patient' AND ""PatientId""::text = current_setting('app.user_id', true))
                        OR current_setting('app.user_role', true) IN ('clinician', 'care_coordinator')
                    );

                ALTER TABLE ""AuditLogs"" ENABLE ROW LEVEL SECURITY;
                CREATE POLICY audit_logs_policy ON ""AuditLogs""
                    USING (
                        current_setting('app.user_role', true) IS NULL
                        OR current_setting('app.user_role', true) = 'admin'
                        OR (current_setting('app.user_role', true) = 'patient' AND (""UserId""::text = current_setting('app.user_id', true) OR ""PatientId""::text = current_setting('app.user_id', true)))
                        OR current_setting('app.user_role', true) IN ('clinician', 'care_coordinator')
                    );

                ALTER TABLE ""Alerts"" ENABLE ROW LEVEL SECURITY;
                CREATE POLICY alerts_policy ON ""Alerts""
                    USING (
                        current_setting('app.user_role', true) IS NULL
                        OR current_setting('app.user_role', true) = 'admin'
                        OR (current_setting('app.user_role', true) = 'patient' AND ""PatientId""::text = current_setting('app.user_id', true))
                        OR current_setting('app.user_role', true) IN ('clinician', 'care_coordinator')
                    );

                ALTER TABLE ""CaregiverLinks"" ENABLE ROW LEVEL SECURITY;
                CREATE POLICY caregiver_links_policy ON ""CaregiverLinks""
                    USING (
                        current_setting('app.user_role', true) IS NULL
                        OR current_setting('app.user_role', true) = 'admin'
                        OR ""PatientId""::text = current_setting('app.user_id', true)
                        OR ""CaregiverId""::text = current_setting('app.user_id', true)
                        OR current_setting('app.user_role', true) IN ('clinician', 'care_coordinator')
                    );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS vital_readings_policy ON ""VitalReadings"";
                ALTER TABLE ""VitalReadings"" DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS audit_logs_policy ON ""AuditLogs"";
                ALTER TABLE ""AuditLogs"" DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS alerts_policy ON ""Alerts"";
                ALTER TABLE ""Alerts"" DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS caregiver_links_policy ON ""CaregiverLinks"";
                ALTER TABLE ""CaregiverLinks"" DISABLE ROW LEVEL SECURITY;
            ");
        }
    }
}
