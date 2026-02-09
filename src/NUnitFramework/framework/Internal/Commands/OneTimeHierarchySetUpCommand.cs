// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using NUnit.Framework.Internal.Execution;

namespace NUnit.Framework.Internal.Commands
{
    internal sealed class OneTimeHierarchySetUpCommand : BeforeTestCommand
    {
        public OneTimeHierarchySetUpCommand(TestCommand innerCommand, TypeHierarchySetUpTearDownItem setUpTearDownItem)
            : base(innerCommand)
        {
            Guard.ArgumentValid(Test is TestSuite, "OneTimeHierarchySetUpCommand must reference a TestSuite", nameof(innerCommand));

            BeforeTest = context => TypeHierarchySetUpTearDownTracker.RunSetUp(setUpTearDownItem, context);
        }
    }
}
