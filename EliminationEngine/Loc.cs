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
        private static string _currentLang = "en";
        public static string CurrentLang { get => _currentLang; }
        public static void InitLoc(string lang)
        {
            List<LocFile> files = new List<LocFile>();
            foreach (var file in Directory.GetFiles(Path.Combine("loc/", lang))) {
                files.Add(FileParser.Deserialize<LocFile>(File.ReadAllText(file)));
            }
            
            Processed = new LocProcessed(files);
            _currentLang = lang;
        }

        public static string Get(string key)
        {
            if (!Processed.local.ContainsKey(key)) return key;
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
