namespace FluentMigrator.VersionTableInfo
{
    public class DefaultExtendedVersionTableMetaData : DefaultVersionTableMetaData, IExtendedVersionTableMetadata
    {
        public string DescriptionColumnName 
        {
            get { return "Description"; }
        }
    }
}