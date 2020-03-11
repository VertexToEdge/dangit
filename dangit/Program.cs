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
        Commit,
        Tree,
        Blob

    }

    [Serializable]
    class GitObject
    {
        string sha1="";
        public gitObjectType type;


        public string getSHA1()
        {
            return sha1;
        }
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
        public string WriteObject(string gitPath)
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
            return this.sha1;
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

        public Commit()
        {
            this.type = gitObjectType.Commit;
        }
        
    }

    [Serializable]
    class Blob: GitObject
    {
        string name;
        byte[] content;
        public Blob()
        {
            this.type = gitObjectType.Blob;
        }

        public static string CreateBlob(string gitPath, FileInfo file)
        {
            Blob b = new Blob();
            if (file.Exists){
                FileStream fs = file.OpenRead();
                b.name = file.Name;
                b.content = File.ReadAllBytes(file.FullName);
                return b.WriteObject(gitPath);
            }
            return "";
        }

    }

    [Serializable]
    class Tree : GitObject
    {
        List<string> trees;
        List<string> blobs;
        public Tree()
        {
            this.type = gitObjectType.Tree;
        }
        public void AddTree(string sha1)
        {
            trees.Add(sha1);
        }
        public void AddBlob(string sha1)
        {
            blobs.Add(sha1);
        }
    }

    class GitResource
    {
        public string resPath;
        public bool isExists()
        {
            return File.Exists(this.resPath);
        }

        public GitResource(string resPath)
        {
            this.resPath = resPath;
        }

        public void touch()
        {
            FileStream fs = new FileStream(this.resPath, FileMode.Append);
            fs.Close();
        }

        public string getContent()
        {
            if (isExists())
            {
                StreamReader sr =  File.OpenText(resPath);
                string result = sr.ReadLine();
                sr.Close();
                return result;
            }
            return "";
        }
        public void setContent(string data)
        {
            FileStream fs = new FileStream(this.resPath, FileMode.Create);
            fs.Write(Encoding.Default.GetBytes(data));
            fs.Close();
        }
        public void AppendContent(string data)
        {
            FileStream fs = new FileStream(this.resPath, FileMode.Append);
            fs.Write(Encoding.Default.GetBytes(data.Trim() + "\n"));
        }
        public void init(string defaultValue)
        {
            if (!isExists())
            {
                setContent(defaultValue);
            }
        }
    }
    class GitHead:GitResource
    {
        string head="";

        public string getHead()
        {
            return head;
        }

        public GitHead(string gitPath) : base(gitPath+"\\HEAD")
        {
            init("ref: refs/heads/master");

            string data = getContent();
            if(data.IndexOf("ref: ") == 0)
            {
                head = data.Substring("ref: ".Length).Trim();
            }
        }
    }

    class GitLog : GitResource
    {
        string head = "";

        public string getHead()
        {
            return head;
        }

        public GitLog(string gitLogPath) : base(gitLogPath)
        {
            touch();
        }
        public void AddLog(string parentSHA1, string commitSHA1, string commitor, string email, long time, string type, string message )
        {

        }
    }

    class GitIndex : GitResource
    {
        string indexSHA1;

        public GitIndex(string gitPath): base(gitPath+"\\Index")
        {

        }
    }

    class Git
    {
        // .git 경로
        string gitPath="";
        
        // git 실행한 경로
        string workPath="";

        // .git 한 단계 위, localRepo의 rootPath
        string rootPath = "";

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

        Action<string> createNotexistsFolder(string path)
        {
            string basePath = path;
            if (Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Action<string> p = delegate (string subpath)
               {
                   if (Directory.Exists(basePath + "\\" + subpath))
                   {
                       Directory.CreateDirectory(basePath + "\\" + subpath);
                   }
               };
            return p;
        }


        public void GitInit()
        {
            this.gitPath = this.workPath + "\\.git";
            var DirectoryCreator = createNotexistsFolder(this.gitPath);
            DirectoryCreator("objects");
            DirectoryCreator("logs");
            DirectoryCreator("logs\\refs");
            DirectoryCreator("logs\\refs\\heads");
            DirectoryCreator("refs");
            DirectoryCreator("refs\\heads");
            DirectoryCreator("refs\\tags");

            GitHead head = new GitHead(this.gitPath);
        }


        public long getTimeStamp()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        //Todo: filter 체커 만들기
        public void GitAdd(List<string> filters)
        {
            Func<DirectoryInfo, string> TreeCreator = null;
            TreeCreator =  delegate (DirectoryInfo path)
             {
                 Tree t = new Tree();
                 foreach (DirectoryInfo d in path.GetDirectories())
                 {
                    //gitPath면 스킵
                    if (d.FullName == this.gitPath)
                     {
                         continue;
                     }

                    //여기서 filter 돌리기


                    t.AddTree(TreeCreator(d));

                 }
                 foreach (FileInfo f in path.GetFiles())
                 {
                     //여기서 filter 돌리기

                     t.AddBlob(Blob.CreateBlob(this.gitPath, f));
                 }

                 
                 return t.WriteObject(this.gitPath);
             };
            string indexSHA1 = TreeCreator(new DirectoryInfo(this.rootPath));
            GitIndex index = new GitIndex(this.gitPath);
            index.setContent(indexSHA1);
        }

        public GitCommit()
        {
            GitHead head = new GitHead();
            
            
        }

        public Git(string workPath)
        {
            this.workPath = workPath;
            if (! findGitPath())
            {
                throw new System.SystemException(".git을 찾을 수 없습니다.");
            }
            //"\.git" => 5글자
            this.rootPath = this.gitPath.Substring(0,this.gitPath.Length - 5);
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
            git.Git a = new git.Git(cDir);
            a.GitInit();

        }
    }
}
