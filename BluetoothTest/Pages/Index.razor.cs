using Blazor.Bluetooth;

namespace BluetoothTest.Pages;

public partial class Index
{
	private readonly IBluetoothNavigator BluetoothNavigator;

    public Index(IBluetoothNavigator bluetoothNavigator)
    {
        BluetoothNavigator = bluetoothNavigator;
    }



}
