﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowCore.Persistence.EntityFramework.Models;
using WorkflowCore.Persistence.EntityFramework.Services;

namespace WorkflowCore.Persistence.SqlServer
{
    public class SqlServerPersistenceProvider : EntityFrameworkPersistenceProvider
    {
        private readonly string _connectionString;

        public SqlServerPersistenceProvider(string connectionString, bool canCreateDB, bool canMigrateDB)
            : base(canCreateDB, canMigrateDB)
        {
            if (!connectionString.Contains("MultipleActiveResultSets"))
                connectionString += ";MultipleActiveResultSets=True";

            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_connectionString);
        }

        protected override void ConfigureSubscriptionStorage(EntityTypeBuilder<PersistedSubscription> builder)
        {
            builder.ToTable("Subscription", "wfc");
            builder.Property(x => x.PersistenceId).UseSqlServerIdentityColumn();
        }

        protected override void ConfigureWorkflowStorage(EntityTypeBuilder<PersistedWorkflow> builder)
        {
            builder.ToTable("Workflow", "wfc");
            builder.Property(x => x.PersistenceId).UseSqlServerIdentityColumn();
        }
        
        protected override void ConfigureExecutionPointerStorage(EntityTypeBuilder<PersistedExecutionPointer> builder)
        {
            builder.ToTable("ExecutionPointer", "wfc");
            builder.Property(x => x.PersistenceId).UseSqlServerIdentityColumn();
        }

        protected override void ConfigureExecutionErrorStorage(EntityTypeBuilder<PersistedExecutionError> builder)
        {
            builder.ToTable("ExecutionError", "wfc");
            builder.Property(x => x.PersistenceId).UseSqlServerIdentityColumn();
        }

        protected override void ConfigureExetensionAttributeStorage(EntityTypeBuilder<PersistedExtensionAttribute> builder)
        {
            builder.ToTable("ExtensionAttribute", "wfc");
            builder.Property(x => x.PersistenceId).UseSqlServerIdentityColumn();
        }

        protected override void ConfigureEventStorage(EntityTypeBuilder<PersistedEvent> builder)
        {
            builder.ToTable("Event", "wfc");
            builder.Property(x => x.PersistenceId).UseSqlServerIdentityColumn();
        }
    }
}
