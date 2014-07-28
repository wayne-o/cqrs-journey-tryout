// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://go.microsoft.com/fwlink/p/?LinkID=258575
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

namespace Sonatribe.Cqrs.WorkerRole
{
	public static class Topics
	{
		public static class Commands
		{
			/// <summary>
			/// conversations/commands
			/// </summary>
			public const string Path = "conversations/commands";

			public static class Subscriptions
			{
				/// <summary>
				/// CreateNewConversationCommandHandler
				/// </summary>
                public const string CreateNewConversationCommandHandler = "CreateNewConversationCommandHandler";
                public const string Sessionless = "Sessionless";
				/// <summary>
				/// log
				/// </summary>
				public const string Log = "log";
			}
		}

		public static class Events
		{
			/// <summary>
			/// conversations/events
			/// </summary>
			public const string Path = "conversations/events";

			public static class Subscriptions
			{
				/// <summary>
				/// log
				/// </summary>
				public const string Log = "log";
			}
		}

	}
}
