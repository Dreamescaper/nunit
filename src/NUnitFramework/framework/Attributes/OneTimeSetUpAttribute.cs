// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

namespace NUnit.Framework
{
    using System;

    /// <summary>
    /// Identifies a method that is called once to perform setup before any child tests are run.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class OneTimeSetUpAttribute : NUnitAttribute
    {
        /// <summary>
        /// Gets or sets the execution scope for the method.
        /// </summary>
        public OneTimeScope Scope { get; set; } = OneTimeScope.Fixture;
    }
}
