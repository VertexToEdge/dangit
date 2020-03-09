using System;
using System.IO;

namespace git
{
    class GitObject
    {
        string sha1;
    }

    class Commit : GitObject
    {
        GitObject parent;
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
        Tree[] trees;
        Blob[] blobs;
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
