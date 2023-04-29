using Blazor.Bluetooth;
using Microsoft.AspNetCore.Components;

namespace BluetoothTest.Pages;

public partial class Index
{
    [Inject]
    private IBluetoothNavigator BluetoothNavigator { get; set; } = null!;
    private IDevice? Device { get; set; }

    private bool Connected { get; set; } = false;
    private bool Available { get; set; } = false;
    private string Error { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        BluetoothNavigator.OnAvailabilityChanged += async () => await OnAvailabilityChanged();
        await BuscarServicio();
    }

    private async Task OnAvailabilityChanged()
    {
        await BuscarServicio();
    }

    private async Task BuscarServicio()
    {
        try
        {
            Available = await BluetoothNavigator.GetAvailability();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task BuscarDispositivos()
    {
        try
        {
            Device = await BluetoothNavigator.RequestDevice(
            new RequestDeviceQuery
            {
                AcceptAllDevices = true
            });
        }
        catch (Exception e)
        {
            Error = e.Message;
        }
    }

    private async Task ConectarDispositivo(bool connect)
    {
        if (Device == null)
            return;

        Connected = connect;

        if (connect)
        {
            await Device.Gatt.Connect();
        }
        else
        {
            await Device.Gatt.Disonnect();
            Device = null;
        }
    }

}
