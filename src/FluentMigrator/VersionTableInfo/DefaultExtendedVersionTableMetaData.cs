using System;

namespace FluentMigrator.VersionTableInfo
{
    public class DefaultExtendedVersionTableMetaData : DefaultVersionTableMetaData, IExtendedVersionTableMetadata
    {
        public string DescriptionColumnName 
        {
            get { return "Description"; }
        }

        public string DateAppliedColumnName
        {
            get { return "DateApplied"; }
        }

        public string ElapsedTimeColumnName
        {
            get { return "ElapsedTime"; }
        }
    }
}