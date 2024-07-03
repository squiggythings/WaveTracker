using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WaveTracker {
    public class IniStructure {
        public IniStructure() {
            data = new Dictionary<string, Dictionary<string, string>>();
        }

        private Dictionary<string, Dictionary<string, string>> data;

        public string this[string section, string key] {
            get {
                if (data.ContainsKey(section)) {
                    if (data[section].ContainsKey(key)) {
                        return data[section][key];
                    }
                    else {
                        throw new KeyNotFoundException("The category structure does not contain the category \'" + section + "\'");
                    }
                }
                else {
                    throw new KeyNotFoundException("The Ini structure does not contain the category \'" + section + "\'");
                }
            }
            set {
                if (!data.ContainsKey(section)) {
                    data.Add(section, new Dictionary<string, string>());
                    if (!data[section].ContainsKey(key)) {
                        data[section].Add(key, value);
                    }
                }
                data[section][key] = value;
            }
        }

        public void WriteToFile(string path, string fileName) {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, fileName))) {

                foreach (KeyValuePair<string, Dictionary<string, string>> section in data) {
                    outputFile.WriteLine("[" + section.Key + "]");
                    foreach (KeyValuePair<string, string> token in section.Value) {
                        outputFile.WriteLine(token.Key + "=" + token.Value);
                    }
                    outputFile.WriteLine();
                }
            }
        }


    }
}
