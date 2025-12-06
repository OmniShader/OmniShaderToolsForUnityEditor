namespace OmniShader.Common
{
    public class OSUtils
    {
        public static bool IsSupportedShader(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (path.EndsWith(".shader") || path.EndsWith(".shadergraph"))
            {
                return true;
            }

            return false;
        }

        public static void LogError(object message)
        {
            #if OS_DEV
            UnityEngine.Debug.LogError(message);
            #endif
        }

        public static void Log(object message)
        {
            Log(message.ToString());
        }

        public static void Log(string message, params object[] args)
        {
            #if OS_DEV
            UnityEngine.Debug.Log(string.Format(message, args));
            #endif
        }
    }
}