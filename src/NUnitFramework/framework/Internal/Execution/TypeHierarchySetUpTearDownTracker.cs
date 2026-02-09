// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Collections.Concurrent;
using System.Threading;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework.Internal.Execution
{
    internal static class TypeHierarchySetUpTearDownTracker
    {
        private sealed class Entry
        {
            public Entry(TypeHierarchySetUpTearDownItem item)
            {
                Item = item;
            }

            public TypeHierarchySetUpTearDownItem Item { get; }
            public int RefCount;
            public bool SetupStarted;
            public bool SetupCompleted;
            public ResultState? SetupResult;
            public string? Message;
            public string? StackTrace;
            public ManualResetEventSlim SetupCompletedEvent { get; } = new(false);
            public object SyncRoot { get; } = new();
        }

        private static readonly ConcurrentDictionary<Type, Entry> Entries = new();

        public static void RunSetUp(TypeHierarchySetUpTearDownItem item, TestExecutionContext context)
        {
            var entry = Entries.GetOrAdd(item.DeclaringType, _ => new Entry(item));
            bool runSetup = false;

            lock (entry.SyncRoot)
            {
                entry.RefCount++;
                if (!entry.SetupStarted)
                {
                    entry.SetupStarted = true;
                    runSetup = true;
                }
            }

            if (runSetup)
            {
                try
                {
                    entry.Item.RunSetUp(context);
                }
                catch (Exception ex)
                {
                    context.CurrentResult.RecordException(ex, FailureSite.SetUp);
                }

                lock (entry.SyncRoot)
                {
                    entry.SetupResult = context.CurrentResult.ResultState;
                    entry.Message = context.CurrentResult.Message;
                    entry.StackTrace = context.CurrentResult.StackTrace;
                    entry.SetupCompleted = true;
                    entry.SetupCompletedEvent.Set();
                }
            }
            else
            {
                entry.SetupCompletedEvent.Wait();
                ApplySetupOutcome(entry, context);
            }
        }

        public static void RunTearDown(TypeHierarchySetUpTearDownItem item, TestExecutionContext context)
        {
            if (!Entries.TryGetValue(item.DeclaringType, out var entry))
                return;

            bool runTearDown = false;

            lock (entry.SyncRoot)
            {
                if (entry.RefCount > 0)
                    entry.RefCount--;

                if (entry.RefCount == 0 && entry.SetupStarted)
                    runTearDown = true;
            }

            if (runTearDown)
            {
                entry.Item.RunTearDown(context);
                entry.SetupCompletedEvent.Dispose();
                Entries.TryRemove(item.DeclaringType, out _);
            }
        }

        private static void ApplySetupOutcome(Entry entry, TestExecutionContext context)
        {
            if (entry.SetupResult is null)
                return;

            if (entry.SetupResult.Status != TestStatus.Passed)
            {
                context.CurrentResult.SetResult(entry.SetupResult, entry.Message ?? string.Empty, entry.StackTrace);
            }
        }
    }
}
