using System;
using OmniShader.Common;
using UnityEditor;

namespace OmniShader.Editor
{
    public class OnLoad
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            try
            {
                if (Unpacker.UnpackInNeeds(Constants.OS_VERSION))
                {
                    Communicator.TryEnsureExecuableIfNeeds();
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}