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
using System.IO;
using System.Linq;
using FluentMigrator.Runner.Announcers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
    public class PendingListAnnouncerTests
	{
        private PendingListAnnouncer _announcer;

		[SetUp]
		public void SetUp()
		{
		    _announcer = new PendingListAnnouncer();
        }

		[Test]
        public void CanAddMigratingVersionToPendingList()
        {
            _announcer.Migrating(200909060935);
            _announcer.PendingMigrations.Single().ShouldBe(200909060935);
        }
	}
}
