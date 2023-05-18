using EliminationEngine.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine
{
    // Localisation
    // Класс локализации
    public static class Loc
    {
        private static LocProcessed Processed;
        public static void InitLoc(string lang)
        {
            List<LocFile> files = new List<LocFile>();
            foreach (var file in Directory.GetFiles(Path.Combine("loc/", lang))) {
                files.Add(FileParser.Deserialize<LocFile>(File.ReadAllText(file)));
            }
            
            Processed = new LocProcessed(files);
        }

        public static string Get(string key)
        {
            return Processed.local[key];
        }




        public class LocFile
        {
            public List<List<string>> local { get; set; }
        }

        public class LocProcessed
        {
            public Dictionary<string, string> local = new();
            public LocProcessed(LocFile file)
            {
                foreach (var instance in file.local)
                {
                    local.Add(instance[0], instance[1]);
                }
            }

            public LocProcessed(List<LocFile> locFiles)
            {
                foreach (var file in locFiles)
                {
                    foreach (var instance in file.local)
                    {
                        local.Add(instance[0], instance[1]);
                    }
                }
            }
        }
    }
}
