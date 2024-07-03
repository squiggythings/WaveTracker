using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker {
    public class TestClass {
        public SubClass subclass;
        public TestClass() {

        }
        public class SubClass {
            public string name;
        }

        public static class SubClass2 {
            public static string name;
        }
    }
}
