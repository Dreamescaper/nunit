// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal.Execution;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// Holds setup and teardown methods for one level of a type hierarchy,
    /// executed once for the entire hierarchy.
    /// </summary>
    internal sealed class TypeHierarchySetUpTearDownItem
    {
        private readonly IMethodValidator? _methodValidator;
        private readonly IList<IMethodInfo> _setUpMethods;
        private readonly IList<IMethodInfo> _tearDownMethods;
        private bool _setUpWasRun;

        public TypeHierarchySetUpTearDownItem(
            Type declaringType,
            IList<IMethodInfo> setUpMethods,
            IList<IMethodInfo> tearDownMethods,
            IMethodValidator? methodValidator = null)
        {
            DeclaringType = declaringType;
            _setUpMethods = setUpMethods;
            _tearDownMethods = tearDownMethods;
            _methodValidator = methodValidator;
        }

        public Type DeclaringType { get; }

        public bool HasMethods => _setUpMethods.Count > 0 || _tearDownMethods.Count > 0;

        public void RunSetUp(TestExecutionContext context)
        {
            _setUpWasRun = true;

            Action<TestExecutionContext, IMethodInfo> runMethod = context.ExecutionHooksEnabled
                ? RunSetUpMethodWithHooks
                : RunSetUpOrTearDownMethod;

            try
            {
                foreach (IMethodInfo setUpMethod in _setUpMethods)
                    runMethod(context, setUpMethod);
            }
            catch (Exception ex)
            {
                context.CurrentResult.RecordException(ex, FailureSite.SetUp);
            }
        }

        public void RunTearDown(TestExecutionContext context)
        {
            if (_setUpWasRun)
            {
                try
                {
                    Action<TestExecutionContext, IMethodInfo> runMethod = context.ExecutionHooksEnabled
                        ? RunTearDownMethodWithHooks
                        : RunSetUpOrTearDownMethod;

                    var oldCount = context.CurrentResult.AssertionResultCount;
                    var index = _tearDownMethods.Count;

                    while (--index >= 0)
                        runMethod(context, _tearDownMethods[index]);

                    if (context.CurrentResult.AssertionResultCount > oldCount)
                        context.CurrentResult.RecordTestCompletion();
                }
                catch (Exception ex)
                {
                    context.CurrentResult.RecordException(ex, FailureSite.TearDown);
                }
            }
        }

        private void RunSetUpMethodWithHooks(TestExecutionContext context, IMethodInfo setUpMethod)
        {
            try
            {
                context.ExecutionHooks.OnBeforeEverySetUp(context, setUpMethod);
                RunSetUpOrTearDownMethod(context, setUpMethod);
            }
            catch (Exception ex)
            {
                context.ExecutionHooks.OnAfterEverySetUp(context, setUpMethod, ex);
                throw;
            }
            context.ExecutionHooks.OnAfterEverySetUp(context, setUpMethod);
        }

        private void RunTearDownMethodWithHooks(TestExecutionContext context, IMethodInfo tearDownMethod)
        {
            try
            {
                context.ExecutionHooks.OnBeforeEveryTearDown(context, tearDownMethod);
                RunSetUpOrTearDownMethod(context, tearDownMethod);
            }
            catch (Exception ex)
            {
                context.ExecutionHooks.OnAfterEveryTearDown(context, tearDownMethod, ex);
                throw;
            }
            context.ExecutionHooks.OnAfterEveryTearDown(context, tearDownMethod);
        }

        private void RunSetUpOrTearDownMethod(TestExecutionContext context, IMethodInfo method)
        {
            Guard.ArgumentNotAsyncVoid(method.MethodInfo, nameof(method));
            _methodValidator?.Validate(method.MethodInfo);

            var methodInfo = MethodInfoCache.Get(method);

            if (methodInfo.IsAsyncOperation)
                AsyncToSyncAdapter.Await(context, () => InvokeMethod(method, context));
            else
                InvokeMethod(method, context);
        }

        private static object InvokeMethod(IMethodInfo method, TestExecutionContext context)
        {
            return method.Invoke(method.IsStatic ? null : context.TestObject, null)!;
        }
    }
}
