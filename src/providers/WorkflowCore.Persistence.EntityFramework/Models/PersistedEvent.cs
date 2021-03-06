﻿using System;
using System.ComponentModel.DataAnnotations;

namespace WorkflowCore.Persistence.EntityFramework.Models
{
    public class PersistedEvent
    {
        [Key]
        public long PersistenceId { get; set; }
        [MaxLength(2000)]
        public string EventId { get; set; }                

        [MaxLength(200)]
        public string EventName { get; set; }

        [MaxLength(200)]
        public string EventKey { get; set; }

        public string EventData { get; set; }

        public DateTime EventTime { get; set; }

        public bool IsProcessed { get; set; }
    }
}
