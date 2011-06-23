#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using System.Reflection;

namespace FluentMigrator.Infrastructure
{
	public class MigrationContext : IMigrationContext
	{
		public virtual IMigrationConventions Conventions { get; set; }
		public virtual ICollection<IMigrationExpression> Expressions { get; set; }
		public virtual IQuerySchema QuerySchema { get; set; }
        public virtual Assembly MigrationAssembly { get; set; }
        public bool PreviewOnly { get; private set; }

		public MigrationContext(IMigrationConventions conventions, IMigrationProcessor migrationProcessor, Assembly migrationAssembly)
		{
			Conventions = conventions;
			Expressions = new List<IMigrationExpression>();
			QuerySchema = migrationProcessor;
		    PreviewOnly = migrationProcessor.Options.PreviewOnly;
            MigrationAssembly = migrationAssembly;
		}
	}
}