using Microsoft.JSInterop;

namespace BluetoothTest.Shared.BluetoothService.Models;

internal class BluetoothAvailabilityHandler
{
    private readonly BluetoothNavigator _bluetoothNavigator;

    internal BluetoothAvailabilityHandler(BluetoothNavigator bluetoothNavigator)
    {
        _bluetoothNavigator = bluetoothNavigator;
    }

    [JSInvokable]
    public void HandleAvailabilityChanged()
    {
        _bluetoothNavigator.RaiseOnAvailabilityChanged();
    }
}
