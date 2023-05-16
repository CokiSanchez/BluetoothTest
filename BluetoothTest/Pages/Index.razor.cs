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
            var filter = new Filter
            {
                Name = Device?.Name ?? "---",
                NamePrefix = Device?.Name ?? "---",
                Services = new List<object>
                {
                    "00001800-0000-1000-8000-00805f9b34fb",
                    "0000180a-0000-1000-8000-00805f9b34fb",
                    "000018f0-0000-1000-8000-00805f9b34fb",
                    "0000ffe0-0000-1000-8000-00805f9b34fb",
                }
            };

            Device = await BluetoothNavigator.RequestDevice(new RequestDeviceQuery
            {
                //OptionalServices = new List<string>
                //{
                //    "00001800-0000-1000-8000-00805f9b34fb",
                //    "0000180a-0000-1000-8000-00805f9b34fb",
                //    "000018f0-0000-1000-8000-00805f9b34fb",
                //    "0000ffe0-0000-1000-8000-00805f9b34fb",
                //},
                Filters = new List<Filter> { filter },
            });

            //Device = await BluetoothNavigator.RequestDevice(
            //new RequestDeviceQuery
            //{
            //    AcceptAllDevices = true
            //});
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
                Logs.Add($"{DateTime.Now:HH:mm} - Dispositivo {Device.Name} {Device.Id} conectado.");
                Logs.Add($"{DateTime.Now:HH:mm} - Dispositivo {Device.Gatt.DeviceUuid} conectado.");
            }
            else
            {
                //var service = await Device.Gatt.GetPrimaryService(Device.Gatt.DeviceUuid);
                //var characteristic = await service.GetCharacteristic(Device.Gatt.DeviceUuid);

                //await characteristic.StopNotifications();

                await Device.Gatt.Disonnect();
                Logs.Add($"{DateTime.Now:HH:mm} - Dispositivo {Device.Name} {Device.Id} desconectado.");
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
        Logs.Add($"{DateTime.Now:HH:mm} - Buscando servicios para 0000ffe0-0000-1000-8000-00805f9b34fb.");

        if (Device is null)
            return;

        try
        {
            var service = await Device.Gatt.GetPrimaryService("0000ffe0-0000-1000-8000-00805f9b34fb");

            Logs.Add($"{DateTime.Now:HH:mm} - detectado servicio {service.IsPrimary} {service.Uuid}.");

            var characteristic = await service.GetCharacteristic("0000ffe1-0000-1000-8000-00805f9b34fb");

            Logs.Add($"{DateTime.Now:HH:mm} - detectado caracteristica {characteristic.Value} {characteristic.Uuid}.");

            //var service = await Device!.Gatt.GetPrimaryService(Device.Gatt.DeviceUuid);
            //var characteristic = await service.GetCharacteristic(Device.Gatt.DeviceUuid);
            //if (characteristic.Properties.Write)
            //{
            //    characteristic.OnRaiseCharacteristicValueChanged += (sender, e) =>
            //    {
            //        Logs.Add($"{DateTime.Now:HH:mm} - Evento {e.ServiceId} {e.CharacteristicId} {e.Value}.");

            //    };
            //    await characteristic.StartNotifications();
            //    //await characteristic.WriteValueWithResponse(/* Your byte array */);
            //}
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
            Error = e.Message;
        }
    }

    private async Task DetenerServicios()
    {
        Logs.Add($"{DateTime.Now:HH:mm} - Deteniendo servicios para {Device?.Gatt.DeviceUuid}.");

        if (Device is null)
            return;

        try
        {
            //var service2 = await Device.Gatt.GetPrimaryService("0000180a-0000-1000-8000-00805f9b34fb");

            //Logs.Add($"{DateTime.Now:HH:mm} - detectado servicio {service2.IsPrimary} {service2.Uuid}.");

            //var characteristic2 = await service2.GetCharacteristic(service2.Uuid);

            //Logs.Add($"{DateTime.Now:HH:mm} - detectado caracteristica {characteristic2.Value} {characteristic2.Uuid}.");


            //var service = await Device.Gatt.GetPrimaryService(Device.Gatt.DeviceUuid);
            //var characteristic = await service.GetCharacteristic(Device.Gatt.DeviceUuid);
            //if (characteristic.Properties.Write)
            //{
            //    characteristic.OnRaiseCharacteristicValueChanged += (sender, e) =>
            //    {

            //    };
            //    await characteristic.StopNotifications();
            //}
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
            Error = e.Message;
        }
    }
}
