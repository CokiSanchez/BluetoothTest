using Blazor.Bluetooth;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace BluetoothTest.Pages;

public partial class Index
{
    [Inject]
    private IBluetoothNavigator BluetoothNavigator { get; set; } = null!;
    private IDevice? Device { get; set; }
    private IBluetoothRemoteGATTCharacteristic? Characteristic { get; set; }

    private bool Available { get; set; } = false;
    private List<string> Logs { get; set; } = new();
    private string Error { get; set; } = string.Empty;

    private string Text { get; set; } = string.Empty;

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
                //Services = new List<object>
                //{
                //    "00001800-0000-1000-8000-00805f9b34fb",
                //    "0000180a-0000-1000-8000-00805f9b34fb",
                //    "000018f0-0000-1000-8000-00805f9b34fb",
                //    "0000ffe0-0000-1000-8000-00805f9b34fb",
                //}
            };

            Device = await BluetoothNavigator.RequestDevice(new RequestDeviceQuery
            {
                AcceptAllDevices = true,
                OptionalServices = new List<string>
                {
                    "00001800-0000-1000-8000-00805f9b34fb",
                    "0000180a-0000-1000-8000-00805f9b34fb",
                    "000018f0-0000-1000-8000-00805f9b34fb",
                    "0000ffe0-0000-1000-8000-00805f9b34fb",
                },
                //Filters = new List<Filter> { filter },
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
                Logs.Add($"{DateTime.Now:HH:mm} - Dispositivo {Device.Name} conectado.");
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
        Logs.Add($"{DateTime.Now:HH:mm} - Buscando servicios para 000018f0-0000-1000-8000-00805f9b34fb.");

        if (Device is null)
            return;

        var service = await Device.Gatt.GetPrimaryService("000018f0-0000-1000-8000-00805f9b34fb");

        Logs.Add($"{DateTime.Now:HH:mm} - detectado servicio {service.IsPrimary} {service.Uuid}.");


        Characteristic = await service.GetCharacteristic("00002af1-0000-1000-8000-00805f9b34fb");

        if (Characteristic is null)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Caracteristica '00002af1-0000-1000-8000-00805f9b34fb' no encontrada.");
            return;
        }

        Logs.Add($"{DateTime.Now:HH:mm} - detectado caracteristica {Characteristic.Value} {Characteristic.Uuid}.");

        Characteristic.OnRaiseCharacteristicValueChanged += (sender, e) =>
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Captura evento {e.ServiceId} {e.CharacteristicId} {Encoding.Default.GetString(e.Value)}.");
        };

        Logs.Add($"{DateTime.Now:HH:mm} - caracteristica {Characteristic.Uuid} suscribe evento.");

        try
        {
            //await Characteristic.StartNotifications();
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

    private async Task Enviar()
    {
        if (Characteristic is null)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Caracteristica '00002af1-0000-1000-8000-00805f9b34fb' no encontrada.");
            return;
        }

        if (string.IsNullOrEmpty(Text))
        {
            Logs.Add($"{DateTime.Now:HH:mm} - No se puede enviar.");
            return;
        }

        try
        {
            await Characteristic.WriteValueWithoutResponse(Encoding.ASCII.GetBytes(Text));
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - No se puede enviar. {e.Message}");
        }
        //await Characteristic.WriteValueWithResponse(Encoding.ASCII.GetBytes("0x037A"));        
    }
}
