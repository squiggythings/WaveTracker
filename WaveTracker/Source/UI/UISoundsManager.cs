namespace WaveTracker.UI {
    public static class UISoundsManager {
        public static void PlaySound(MessageDialog.Icon icon) {
#if WINDOWS
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
#endif
        }
    }
}
