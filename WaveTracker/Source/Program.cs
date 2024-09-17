
using System;

namespace WaveTracker {

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args) {
            using (App game = new App(args)) {
                game.Run();
            }
        }
    }
}
