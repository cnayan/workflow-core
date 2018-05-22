﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WorkflowCore.Models;

namespace WorkflowCore.Persistence.EntityFramework.Models
{
    public class PersistedWorkflow
    {
        [Key]
        public long PersistenceId { get; set; }

        [MaxLength(200)]
        public string InstanceId { get; set; }

        [MaxLength(200)]
        public string WorkflowDefinitionId { get; set; }

        public int Version { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(200)]
        public string Reference { get; set; }

        public virtual List<PersistedExecutionPointer> ExecutionPointers { get; set; } = new List<PersistedExecutionPointer>();

        //[Index]
        public long? NextExecution { get; set; }

        public string Data { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? CompleteTime { get; set; }

        public WorkflowStatus Status { get; set; }
        
    }
}
