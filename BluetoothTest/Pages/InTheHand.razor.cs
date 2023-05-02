using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.AspNetCore.Components;

namespace BluetoothTest.Pages;

public partial class InTheHand
{
    [Inject]
    private BluetoothDevicePicker Picker { get; set; } = null!;

    private BluetoothDeviceInfo? DeviceInfo { get; set; } = null;
    private bool Available { get; set; } = false;
    private List<string> Logs { get; set; } = new();
    private string Error { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        
    }

    private async Task BuscarDispositivos()
    {
        try
        {
            DeviceInfo = await Picker.PickSingleDeviceAsync();

        }
        catch (Exception e)
        {
            Error = e.Message;
        }
    }

    private async Task ConectarDispositivo(bool connect)
    {
        

        try
        {
            
        }
        catch (Exception e)
        {
            Error = e.Message;
        }
    }

}
