namespace AppRoleSample
{
    public class Workflow
    {
        public enum WorkflowState
        {
            Authenticate,
            GetApplicationRole,
            CreateDatabaseConnection,
            SetApplicationRole,
            Query
        }
    }
}
