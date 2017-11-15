using BoinWPF;

namespace PadSharp
{
    public static class Global
    {
        public const string APP_NAME = "Pad#";

        public static void actionMessage(string action, string details)
        {
            Alert.showDialog(action + ". Here are some additional details: " + details, APP_NAME);
        }
    }
}
