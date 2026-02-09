// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using NUnit.Framework.Internal.Execution;

namespace NUnit.Framework.Internal.Commands
{
    internal sealed class OneTimeHierarchyTearDownCommand : AfterTestCommand
    {
        public OneTimeHierarchyTearDownCommand(TestCommand innerCommand, TypeHierarchySetUpTearDownItem setUpTearDownItem)
            : base(innerCommand)
        {
            Guard.ArgumentValid(Test is TestSuite, "OneTimeHierarchyTearDownCommand may only apply to a TestSuite", nameof(innerCommand));

            AfterTest = context => TypeHierarchySetUpTearDownTracker.RunTearDown(setUpTearDownItem, context);
        }
    }
}
