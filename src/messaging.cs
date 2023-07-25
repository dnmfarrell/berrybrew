using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BerryBrew.Messaging {
    public class Message {

        private readonly OrderedDictionary _msgMap = new OrderedDictionary();

        public void Add(dynamic json) {
            string content = null;

            foreach (string line in json.content) {
                content += String.Format("{0}\n", line);
            }

            _msgMap.Add(json.label.ToString(), content);
        }

        public void Error(string label) {
            string msg = Get(label);
            Console.Error.WriteLine(msg);
        }

        public string Get(string label) {
            return _msgMap[label].ToString();
        }

        public void Print(string label) {
            string msg = Get(label);
            Console.WriteLine(msg);
        }

        public void Say(string label) {
            string msg = Get(label);
            Console.WriteLine(msg);
            Environment.Exit(0);
        }
    }
}
