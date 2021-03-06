using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
	public class VersionLoader : IVersionLoader
	{
		public VersionLoader(IMigrationRunner runner, Assembly assembly, IMigrationConventions conventions)
		{
			Runner = runner;
			Processor = runner.Processor;
			Assembly = assembly;

			Conventions = conventions;
			VersionTableMetaData = GetVersionTableMetaData();
			VersionMigration = new VersionMigration(VersionTableMetaData);
		    VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
		    ExtendedVersionMigration = new ExtendedVersionMigration(VersionTableMetaData);
			LoadVersionInfo();
		}

	    protected VersionSchemaMigration VersionSchemaMigration { get; set; }

	    private VersionInfo _versionInfo;
		public IMigrationRunner Runner { get; set; }
		protected Assembly Assembly { get; set; }
		public IVersionTableMetaData VersionTableMetaData { get; set; }
		private IMigrationConventions Conventions { get; set; }
		private IMigrationProcessor Processor { get; set; }
		private IMigration VersionMigration { get; set; }
        private IMigration ExtendedVersionMigration { get; set; }
		
        public void UpdateVersionInfo(long version, Type type, TimeSpan elapsedTime)
		{
			var dataExpression = new InsertDataExpression();
			dataExpression.Rows.Add( CreateVersionInfoInsertionData( version, type, elapsedTime ) );
			dataExpression.TableName = VersionTableMetaData.TableName;
			dataExpression.SchemaName = VersionTableMetaData.SchemaName;
			dataExpression.ExecuteWith( Processor );
		}

		public IVersionTableMetaData GetVersionTableMetaData()
		{
			Type matchedType = Assembly.GetExportedTypes().Where(t => Conventions.TypeIsVersionTableMetaData(t)).FirstOrDefault();

			if (matchedType == null)
			{
                if (Processor.Options.StoreExtendedData)
                    return new DefaultExtendedVersionTableMetaData();

                return new DefaultVersionTableMetaData();
			}

			return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
		}

		protected virtual InsertionDataDefinition CreateVersionInfoInsertionData(long version, Type type, TimeSpan elapsedTime)
		{
			var insertData = new InsertionDataDefinition { new KeyValuePair<string, object>( VersionTableMetaData.ColumnName, version ) };
            if ((Processor.Options.StoreExtendedData) && (VersionTableMetaData is IExtendedVersionTableMetadata))
            {
                var metadata = Conventions.GetMetadataForMigration(type);
                insertData.Add(new KeyValuePair<string, object>(((IExtendedVersionTableMetadata)VersionTableMetaData).DescriptionColumnName, metadata.Description));
                insertData.Add(new KeyValuePair<string, object>(((IExtendedVersionTableMetadata)VersionTableMetaData).DateAppliedColumnName, DateTime.UtcNow));
                insertData.Add(new KeyValuePair<string, object>(((IExtendedVersionTableMetadata)VersionTableMetaData).ElapsedTimeColumnName, elapsedTime.TotalMilliseconds));
            }
		    return insertData;
		}

		public VersionInfo VersionInfo
		{
			get
			{
				return _versionInfo;
			}
			set
			{
				if (value == null)
					throw new ArgumentException("Cannot set VersionInfo to null");

				_versionInfo = value;
			}
		}

        public bool AlreadyCreatedVersionSchema
        {
            get
            {
                return Processor.SchemaExists(VersionTableMetaData.SchemaName);
            }
        }

		public bool AlreadyCreatedVersionTable
		{
			get
			{
				return Processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
			}
		}

        public bool AlreadyCreatedExtendedVersionTable
        {
            get
            {
                return AlreadyCreatedVersionTable &&
                    (!(VersionTableMetaData is IExtendedVersionTableMetadata)
                    || Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, ((IExtendedVersionTableMetadata)VersionTableMetaData).DescriptionColumnName));
            }
        }

		public void LoadVersionInfo()
		{
			if (!AlreadyCreatedVersionSchema)
                Runner.Up(VersionSchemaMigration);

		    bool isNewTable = false;
			if ( !AlreadyCreatedVersionTable )
			{
				Runner.Up( VersionMigration );
			    isNewTable = true;
			}

            if (Processor.Options.StoreExtendedData && !AlreadyCreatedExtendedVersionTable)
            {
                Runner.Up(ExtendedVersionMigration);
            }
            if (isNewTable)
            {
                _versionInfo = new VersionInfo();
                return;
            }
		    var dataSet = Processor.ReadTableData(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName );
			_versionInfo = new VersionInfo();

			foreach ( DataRow row in dataSet.Tables[ 0 ].Rows )
			{
			    var migrationVersion = long.Parse(row[0].ToString());
				_versionInfo.AddAppliedMigration( migrationVersion );
                if ((Processor.Options.StoreExtendedData) && (VersionTableMetaData is IExtendedVersionTableMetadata))
                {
                    var extendedTableMetadata = (IExtendedVersionTableMetadata) VersionTableMetaData;
                    _versionInfo.AddAppliedMigrationInfo(
                        migrationVersion, 
                        row[extendedTableMetadata.DescriptionColumnName].ToString(),
                        row[extendedTableMetadata.DateAppliedColumnName] is DateTime ? (DateTime) row[extendedTableMetadata.DateAppliedColumnName] : new DateTime(), 
                        row[extendedTableMetadata.ElapsedTimeColumnName] is double ? (double) row[extendedTableMetadata.ElapsedTimeColumnName] : 0d);
                }
			}
		}

		public void RemoveVersionTable()
		{
		    var expression = new DeleteTableExpression {TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName};
			expression.ExecuteWith( Processor );

            if (!string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
            {
                var schemaExpression = new DeleteSchemaExpression {SchemaName = VersionTableMetaData.SchemaName};
                schemaExpression.ExecuteWith(Processor);
            }
		}

		public void DeleteVersion(long version)
		{
			var expression = new DeleteDataExpression { TableName = VersionTableMetaData.TableName, SchemaName=VersionTableMetaData.SchemaName };
			expression.Rows.Add(new DeletionDataDefinition
									{
										new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
									});
			expression.ExecuteWith( Processor );
		}
	}
}