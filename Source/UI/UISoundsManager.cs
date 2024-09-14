using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveTracker.UI {
    public static class UISoundsManager {
        public static void PlaySound(MessageDialog.Icon icon) {
            if (icon == MessageDialog.Icon.Information) {
                System.Media.SystemSounds.Asterisk.Play();
            }
            else if (icon == MessageDialog.Icon.Error) {
                System.Media.SystemSounds.Hand.Play();
            }
            else if (icon == MessageDialog.Icon.Warning) {
                System.Media.SystemSounds.Exclamation.Play();
            }
            else if (icon == MessageDialog.Icon.Question) {
                System.Media.SystemSounds.Question.Play();
            }
        }
    }
}
