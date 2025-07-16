using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MetaStamp.Internal
{
    internal static class GitUtils
    {
        public static string? GetRepoCommitId(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!Directory.Exists(path))
            {
                return null;
            }

            while (path != null)
            {
                string gitFolderPath = Path.Combine(path, ".git");
                if (Directory.Exists(gitFolderPath))
                {
                    string headFilePath = Path.Combine(gitFolderPath, "HEAD");
                    if (File.Exists(headFilePath))
                    {
                        string headContent = File.ReadAllText(headFilePath).Trim();
                        if (headContent.StartsWith("ref:"))
                        {
                            string branchRef = headContent.Substring(5).Trim();
                            string branchRefFilePath = Path.Combine(gitFolderPath, branchRef.Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (File.Exists(branchRefFilePath))
                            {
                                return File.ReadAllText(branchRefFilePath).Trim();
                            }
                        }
                        else
                        {
                            return headContent;
                        }
                    }
                }
                path = Directory.GetParent(path)?.FullName;
            }

            return null;
        }

        public static string? GetRepoBranchName(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!Directory.Exists(path))
            {
                return null;
            }

            while (path != null)
            {
                string gitFolderPath = Path.Combine(path, ".git");
                if (Directory.Exists(gitFolderPath))
                {
                    string headFilePath = Path.Combine(gitFolderPath, "HEAD");
                    if (File.Exists(headFilePath))
                    {
                        string headContent = File.ReadAllText(headFilePath).Trim();
                        if (headContent.StartsWith("ref:"))
                        {
                            string branchRef = headContent.Substring(5).Trim();
                            return branchRef.Split('/').Last(); // 返回分支名称
                        }
                    }
                }

                path = Directory.GetParent(path)?.FullName;
            }

            return null;
        }
    }
}
