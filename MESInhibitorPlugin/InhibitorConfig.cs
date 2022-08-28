using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.FileSystem;

namespace avaness.MESInhibitorPlugin
{
    public class InhibitorConfig
    {
        private const string ModFolder = "MESInhibitorConfig";
        private const string FileName = "inhibitors.xml";

        [XmlArray]
        [XmlArrayItem("Inhibitor")]
        public Inhibitor[] Inhibitors
        {
            get
            {
                return inhibitors.Values.ToArray();
            }
            set
            {
                inhibitors = new Dictionary<string, Inhibitor>();
                foreach (Inhibitor inhibitor in value)
                {
                    if(!string.IsNullOrWhiteSpace(inhibitor.TypeName))
                        inhibitors[inhibitor.TypeName] = inhibitor;
                }
            }
        }

        private Dictionary<string, Inhibitor> inhibitors = new Dictionary<string, Inhibitor>();

        public static InhibitorConfig Load()
        {

            try
            {
                string path = GetFilePath();
                if (File.Exists(path))
                {
                    Stream stream = MyFileSystem.OpenRead(path);
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            InhibitorConfig config = MyAPIGateway.Utilities.SerializeFromXML<InhibitorConfig>(reader.ReadToEnd());
                            if (config == null)
                                throw new NullReferenceException("Failed to serialize from xml.");
                            return config;
                        }
                    }

                }
            } catch { }

            InhibitorConfig result = new InhibitorConfig();
            result.Save();
            return result;
        }

        public void Save()
        {
            Stream stream = MyFileSystem.OpenWrite(GetFilePath());
            if (stream != null)
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(this));
                }
            }
        }

        private static string GetFilePath()
        {
            return Path.Combine(MySession.Static.CurrentPath, "Storage", ModFolder, FileName);
        }

        public void AddInhibitor(string inhibitorName)
        {
            if(!inhibitors.ContainsKey(inhibitorName))
            {
                inhibitors[inhibitorName] = new Inhibitor()
                {
                    TypeName = inhibitorName,
                    Enabled = false
                };
            }
        }

        public bool IsEnabled(string inhibitorName)
        {
            if(inhibitors.TryGetValue(inhibitorName, out Inhibitor inhibitor))
                return inhibitor.Enabled;
            return true;
        }

        public class Inhibitor
        {
            public string TypeName;
            public bool Enabled;
        }
    }
}
