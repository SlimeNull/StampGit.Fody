using System;
using System.Collections.Generic;
using System.Text;

namespace StampGit.Fody
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class GitStampAttribute : Attribute
    {
        public string? ID { get; set; }
    }
}
