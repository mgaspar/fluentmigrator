using System;

namespace FluentMigrator.Runner.Versioning
{
    public class AppliedMigration
    {
        public long Version { get; set; }
        public string Description { get; set; }
        public DateTime DateApplied { get; set; }
        public double ElapsedTime { get; set; }
    }
}