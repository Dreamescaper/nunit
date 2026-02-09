// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

namespace NUnit.Framework
{
    /// <summary>
    /// Defines the execution scope for one-time setup and teardown methods.
    /// </summary>
    public enum OneTimeScope
    {
        /// <summary>
        /// Run once per fixture (default behavior).
        /// </summary>
        Fixture = 0,

        /// <summary>
        /// Run once per type hierarchy (the class and all descendants).
        /// </summary>
        TypeHierarchy = 1
    }
}
