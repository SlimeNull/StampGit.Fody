using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControlSummary
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GitCommitAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GitBranchAttribute : Attribute
    {

    }
}
