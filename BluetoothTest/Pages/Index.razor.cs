using Blazor.Bluetooth;
using Microsoft.AspNetCore.Components;

namespace BluetoothTest.Pages;

public partial class Index
{
    [Inject]
    private IBluetoothNavigator BluetoothNavigator { get; set; } = null!;
    private IDevice? Device { get; set; }

    private bool Available { get; set; } = false;
    private List<string> Logs { get; set; } = new();
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
        catch (Exception e)
        {
            Error = e.Message;
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

        try
        {
            if (connect)
            {
                await Device.Gatt.Connect();
                Logs.Add($"{DateTime.Now:HH:mm} - Dispositivo {Device.Name} conectado.");
            }
            else
            {
                var service = await Device.Gatt.GetPrimaryService(Device.Gatt.DeviceUuid);
                var characteristic = await service.GetCharacteristic(Device.Gatt.DeviceUuid);

                await characteristic.StopNotifications();

                await Device.Gatt.Disonnect();
                Logs.Add($"{DateTime.Now:HH:mm} - Dispositivo {Device.Name} desconectado.");
                Device = null;
                Logs.Clear();
            }
        }
        catch (Exception e)
        {
            Error = e.Message;
        }
    }

    private async Task ComenzarServicios()
    {
        Logs.Add($"{DateTime.Now:HH:mm} - Buscando servicios para {Device?.Gatt.DeviceUuid}.");

        if (Device is null)        
            return;        

        try
        {
            var service = await Device!.Gatt.GetPrimaryService(Device.Gatt.DeviceUuid);
            var characteristic = await service.GetCharacteristic(Device.Gatt.DeviceUuid);
            if (characteristic.Properties.Write)
            {
                characteristic.OnRaiseCharacteristicValueChanged += (sender, e) =>
                {
                    Logs.Add($"{DateTime.Now:HH:mm} - Evento {e.ServiceId} {e.CharacteristicId} {e.Value}.");

                };
                await characteristic.StartNotifications();
                //await characteristic.WriteValueWithResponse(/* Your byte array */);
            }
        }
        catch (Exception e)
        {
            Error = e.Message;
        }
    }

    private async Task DetenerServicios()
    {
        Logs.Add($"{DateTime.Now:HH:mm} - Deteniendo servicios para {Device.Gatt.DeviceUuid}.");

        try
        {
            var service = await Device.Gatt.GetPrimaryService(Device.Gatt.DeviceUuid);
            var characteristic = await service.GetCharacteristic(Device.Gatt.DeviceUuid);
            if (characteristic.Properties.Write)
            {
                characteristic.OnRaiseCharacteristicValueChanged += (sender, e) =>
                {

                };
                await characteristic.StopNotifications();
            }
        }
        catch (Exception e)
        {

            Error = e.Message;
        }
    }
}
