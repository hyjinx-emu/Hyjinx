using Hyjinx.Graphics.Device;
using System;
using System.Collections.Generic;

namespace Hyjinx.Graphics.Host1x;

class Devices : IDisposable
{
    private readonly Dictionary<ClassId, IDeviceState> _devices = new();

    public void RegisterDevice(ClassId classId, IDeviceState device)
    {
        _devices[classId] = device;
    }

    public IDeviceState GetDevice(ClassId classId)
    {
        return _devices.TryGetValue(classId, out IDeviceState device) ? device : null;
    }

    public void Dispose()
    {
        foreach (var device in _devices.Values)
        {
            if (device is ThiDevice thi)
            {
                thi.Dispose();
            }
        }
    }
}