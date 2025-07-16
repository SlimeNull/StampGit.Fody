using System;
using System.Collections.Generic;
using System.Text;

namespace MetaStamp
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GitCommitIDAttribute : Attribute
    {
        
    }
}
