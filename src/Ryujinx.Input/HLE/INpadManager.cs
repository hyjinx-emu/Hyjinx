using Ryujinx.Common.Configuration.Hid;
using System;
using System.Collections.Generic;

namespace Ryujinx.Input.HLE
{
    public interface INpadManager : IDisposable
    {
        void ReloadConfiguration(List<InputConfig> inputConfig, bool enableKeyboard, bool enableMouse);
        
        void UnblockInputUpdates();
        
        void BlockInputUpdates();
        
        void Update(float aspectRatio = 1);
        
        InputConfig GetPlayerInputConfigByIndex(int index);
    }
}
