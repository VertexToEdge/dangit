using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace git
{
    [Serializable]
    enum gitObjectType{
        commit,
        Tree,
        Blob

    }

    [Serializable]
    class GitObject
    {
        string sha1="";
        gitObjectType type;



        public MemoryStream ObjectToMemory()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter s = new BinaryFormatter();
            s.Serialize(ms, this);
            return ms;
        }
        string makeSha1(MemoryStream ms)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(ms.ToArray());
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
        public void WriteObject(string gitPath)
        {
            MemoryStream ms = ObjectToMemory();
            string sha1 = makeSha1(ms);
            string objPath = gitPath + "\\objects\\" + sha1.Substring(0, 2);
            DirectoryInfo di = new DirectoryInfo(objPath);

            if (!di.Exists)
            {
                di.Create();
            }
            FileInfo fi = new FileInfo(objPath + "\\" + sha1.Substring(2));
            if (!fi.Exists)
            {
                Stream ws = fi.Create();
                ws.Write(ms.ToArray());
                ws.Close();
            }
            this.sha1 = sha1;
        }

        public static GitObject ReadObject(string gitPath, string sha1)
        {
            FileInfo fi = new FileInfo(gitPath + "\\objects\\" + sha1.Substring(0, 2) + "\\" + sha1.Substring(2));
            if (fi.Exists)
            {
                BinaryFormatter s = new BinaryFormatter();
                return (GitObject)s.Deserialize(fi.OpenRead());
            }
            return new GitObject();
        }
    }

    [Serializable]
    class Commit : GitObject
    {
        List<string> parent;
        string commiter;
        string author;
        string message;
        
    }

    [Serializable]
    class Blob: GitObject
    {
        string name;
        byte[] content;
    }

    [Serializable]
    class Tree : GitObject
    {

        List<string> trees;
        List<string> blobs;
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
                    this.gitPath = tmpPath + "\\.git";
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
