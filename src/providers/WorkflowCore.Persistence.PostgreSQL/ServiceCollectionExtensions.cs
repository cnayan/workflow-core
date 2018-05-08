using WorkflowCore.Models;
using WorkflowCore.Persistence.PostgreSQL;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static WorkflowOptions UsePostgreSQL(this WorkflowOptions options, string connectionString, bool canCreateDB, bool canMigrateDB)
        {
            options.UsePersistence(sp => new PostgresPersistenceProvider(connectionString, canCreateDB, canMigrateDB));
            return options;
        }
    }
}
