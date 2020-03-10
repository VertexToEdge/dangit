using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace git
{
    enum gitObjectType{
        commit,
        Tree,
        Blob

    }
    class GitObject
    {
        string sha1;
        gitObjectType type;
    }

    class Commit : GitObject
    {
        string parentSha1;
        string commiter;
        string author;
        
    }

    class Blob: GitObject
    {
        string name;
        byte[] content;
    }

    class Tree : GitObject
    {
        GitObject[] data;
    }

    class Git
    {
        // .git 경로
        string gitPath="";
        
        // git 실행한 경로
        string workPath="";

        public bool findGitPath()
        {
            List<string> workPathToken = workPath.Split("\\").ToList();

            // .git 폴더 찾기
            for (int i = 0; i < workPathToken.Count; i++)
            {
                string tmpPath = String.Join("\\", workPathToken);
                DirectoryInfo dInfo = new DirectoryInfo(tmpPath);
                if (dInfo.GetDirectories().Any(i => i.Name == ".git"))
                {
                    gitPath = tmpPath + "\\.git";
                    return true;
                }
                else
                {
                    workPathToken.RemoveAt(workPathToken.Count - 1);
                }
            }
            return false;
        }

        public Git(string workPath)
        {
            this.workPath = workPath;
        }

    }
}

namespace dangit
{
    class Program
    {
        static void Main(string[] args)
        {
     
            String cDir = Directory.GetCurrentDirectory();
            Console.WriteLine(cDir);
        }
    }
}
