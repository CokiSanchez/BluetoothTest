using Blazor.Bluetooth;
using Microsoft.AspNetCore.Components;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothTest.Pages;

public partial class Index : IDisposable
{
    [Inject]
    private IBluetoothNavigator? BluetoothNavigator { get; set; }
    private IDevice? Device { get; set; }
    private IBluetoothRemoteGATTCharacteristic? Characteristic { get; set; }

    private bool Available { get; set; } = false;
    private List<string> Logs { get; set; } = new();
    private string Error { get; set; } = string.Empty;
    private string Text { get; set; } = string.Empty;

    private string Parte = "{ESC@}{ESCR7}{ESCE1}{ESCa0}{ESC-1}{ESCa1}{ESC-0} ILUSTRE MUNICIPALIDAD DE VITACURA {NLN} INSPECCION MUNICIPAL {NLN}{NLN}{GSB1} {Citacion_Tipo} {GSB0}{ESC-1}{ESCa2} {ESC-0}{NLN}{ESCa2} Nº Citacion: {Citacion_IdNrPedido}{NLN}{ESCa0}{TABH} Vitacura, Fecha/Hora:{TAB9}{ESCE0}{Gen_Fecha} {Gen_Hora} HRS.{NLN}{NLN}{ESCE1}{ESC-2}VEHICULO{ESC-0}{ESCE0}{NLN}{ESCE1}{ESCE0}Placa:{TAB9}{Transito_Placa}{NLN}Marca:{TAB9}{Transito_Marca}{NLN}Modelo:{TAB9}{Transito_Modelo}{NLN}Color:{TAB9}{Transito_Color}{NLN}Tipo Vehiculo:{TAB9}{Transito_TipoVehiculo}{NLN}{NLN}{ESCE1}{ESC-2}FISCALIZACION{ESC-0}{ESCE0}{NLN}Infraccion:{NLN}{Citacion_Infracciones_Ind}{NLN}- LUGAR:{TAB9}{Transito_Lugar}{NLN}- OBSERVACIONES:{NLN}{NLN}{Citacion_Infracciones_Obs}{NLN}{NLN}{ESCE1}{ESC-2}CITACION{ESC-0}{ESCE0}{NLN}CITO A UD AL {Citacion_Juzgado}, UBICADO EN {ESCE1}{Citacion_DirJuzgado}{ESCE0}.{NLN}PARA LA AUDENCIA DEL {ESCE1}{Citacion_FechaCitacion} A LAS {ESCE1}{Citacion_HoraCitacion}{ESCE0} HRS.{NLN}{NLN}SI EL DIA FIJADO  NO COMPARECIERE, SERA JUZGADO EN REBELDIA CONFORME A LA LEY.{NLN}{NLN}RECIBIDO POR: {TAB9}{Citacion_Nombre}{NLN}{NLN}-INSPECTOR:{TAB9}{Gen_NombreInspector1}{NLN}{NLN}Nº INTERNO: {Citacion_NrNotif}{NLN}";

    protected override async Task OnInitializedAsync()
    {
        if (BluetoothNavigator is null)
            return;

        BluetoothNavigator.OnAvailabilityChanged += async () => await OnAvailabilityChanged();
        await BuscarServicio();
    }

    private async Task OnAvailabilityChanged()
    {
        await BuscarServicio();
    }

    private async Task BuscarServicio()
    {
        if (BluetoothNavigator is null)
            return;

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
        if (BluetoothNavigator is null)
            return;

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
            Logs.Add($"{DateTime.Now:HH:mm} - Error: {e.Message}.");
            Error = e.Message;
        }
    }

    private async Task BuscarServicios()
    {
        if (Device is null)
            return;

        Logs.Add($"{DateTime.Now:HH:mm} - Buscando servicios para {Device.Gatt.DeviceUuid}.");

        try
        {
            var services = await Device.Gatt.GetPrimaryServices(Device.Gatt.DeviceUuid);
            Logs.Add($"{DateTime.Now:HH:mm} - Servicios encontrados {string.Join("-", services?.Select(s => s.Uuid) ?? Array.Empty<string>())}.");
        }
        catch (Exception e)
        {

            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
            Error = e.Message;
        }
    }

    private async Task ComenzarServicios()
    {
        Logs.Add($"{DateTime.Now:HH:mm} - Buscando servicios para 000018f0-0000-1000-8000-00805f9b34fb.");

        if (Device is null)
            return;

        if (!Device.Gatt.Connected)
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

        if (Characteristic is null)
            return;

        try
        {
            await Characteristic.StopNotifications();
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

        //if (string.IsNullOrEmpty(Text))
        //{
        //    Logs.Add($"{DateTime.Now:HH:mm} - No se puede enviar.");
        //    return;
        //}

        try
        {
            var chunks = Formatear(Text);

            foreach (var chunk in chunks)
            {
                await Characteristic.WriteValueWithoutResponse(chunk);
            }

        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - No se puede enviar. {e.Message}");
        }
    }

    private IEnumerable<byte[]> Formatear(string text)
    {
        var texto = Encoding.ASCII.GetBytes(PrintDriver(Parte));

        Logs.Add($"{DateTime.Now:HH:mm} - Se envia a imprimir {texto.Length} de tamaño");

        var chunks = texto.Chunk(512);

        return chunks;
    }

    private string PrintDriver(string toPrint)
    {

        toPrint = toPrint.Replace("{ESC@}", "\u001b\u0040");//inicializa Impresora    	
        toPrint = toPrint.Replace("{ESCS}", "\u001b\u0053");//Standar Mode

        toPrint = toPrint.Replace("{ESCR0}", "\u001b\u0052\u0000");//Caracteres USA
        toPrint = toPrint.Replace("{ESCR1}", "\u001b\u0052\u0001");// Caracteres France
        toPrint = toPrint.Replace("{ESCR2}", "\u001b\u0052\u0002");// Caracteres Germany
        toPrint = toPrint.Replace("{ESCR3}", "\u001b\u0052\u0003");// Caracteres UK
        toPrint = toPrint.Replace("{ESCR4}", "\u001b\u0052\u0004");// Caracteres DENMARK
        toPrint = toPrint.Replace("{ESCR5}", "\u001b\u0052\u0005");// Caracteres Sweden
        toPrint = toPrint.Replace("{ESCR6}", "\u001b\u0052\u0006");// Caracteres Italy
        toPrint = toPrint.Replace("{ESCR7}", "\u001b\u0052\u0007");// Caracteres Spain
        toPrint = toPrint.Replace("{ESCR8}", "\u001b\u0052\u0008");// Caracteres Japan
        toPrint = toPrint.Replace("{ESCR9}", "\u001b\u0052\u0009");// Caracteres Norway

        toPrint = toPrint.Replace("{ESCt0}", "\u001b\u0074\u0000");//inicializa PC437
        toPrint = toPrint.Replace("{ESCt1}", "\u001b\u0074\u0001");//inicializa kATAKANA
        toPrint = toPrint.Replace("{ESCt2}", "\u001b\u0074\u0002");//inicializa PC850 MultiLengual
        toPrint = toPrint.Replace("{ESCt3}", "\u001b\u0074\u0003");//inicializa PC860
        toPrint = toPrint.Replace("{ESCt4}", "\u001b\u0074\u0004");//inicializa PC863

        toPrint = toPrint.Replace("{ESCa0}", "\u001b\u0061\u0000");//Texto a la Izquierda
        toPrint = toPrint.Replace("{ESCa1}", "\u001b\u0061\u0001");//Centra Texto
        toPrint = toPrint.Replace("{ESCa2}", "\u001b\u0061\u0002");//Texto a La derecha 
        toPrint = toPrint.Replace("{ESCE1}", "\u001b\u0045\u0001");//Negrita    	
        toPrint = toPrint.Replace("{ESCE0}", "\u001b\u0045\u0000");//Quitar Negrita
        toPrint = toPrint.Replace("{ESC!00}", "\u001d\u0021\u0000");//TamaÃƒÂ±o Caracteres regular 12
        toPrint = toPrint.Replace("{ESC!01}", "\u001d\u0021\u0001");//TamaÃƒÂ±o Caracteres altura 14
        toPrint = toPrint.Replace("{ESC!02}", "\u001d\u0021\u0002");//TamaÃƒÂ±o Caracteres altura 16
        toPrint = toPrint.Replace("{ESC!03}", "\u001d\u0021\u0003");//TamaÃƒÂ±o Caracteres altura 18
        toPrint = toPrint.Replace("{ESC!04}", "\u001d\u0021\u0004");//TamaÃƒÂ±o Caracteres altura 20
        toPrint = toPrint.Replace("{ESC!05}", "\u001d\u0021\u0005");//TamaÃƒÂ±o Caracteres altura 22
        toPrint = toPrint.Replace("{ESC!06}", "\u001d\u0021\u0006");//TamaÃƒÂ±o Caracteres altura 24
        toPrint = toPrint.Replace("{ESC!07}", "\u001d\u0021\u0007");//TamaÃƒÂ±o Caracteres altura 26	    	
        toPrint = toPrint.Replace("{ESC!10}", "\u001d\u0021\u0010");//TamaÃƒÂ±o Caracteres ancho 14
        toPrint = toPrint.Replace("{ESC!20}", "\u001d\u0021\u0020");//TamaÃƒÂ±o Caracteres ancho 16
        toPrint = toPrint.Replace("{ESC!30}", "\u001d\u0021\u0030");//TamaÃƒÂ±o Caracteres ancho 18
        toPrint = toPrint.Replace("{ESC!40}", "\u001d\u0021\u0040");//TamaÃƒÂ±o Caracteres ancho 20
        toPrint = toPrint.Replace("{ESC!50}", "\u001d\u0021\u0050");//TamaÃƒÂ±o Caracteres ancho 22
        toPrint = toPrint.Replace("{ESC!60}", "\u001d\u0021\u0060");//TamaÃƒÂ±o Caracteres ancho 22
        toPrint = toPrint.Replace("{ESC!70}", "\u001d\u0021\u0070");//TamaÃƒÂ±o Caracteres ancho 22	    	
        toPrint = toPrint.Replace("{GSB1}", "\u001d\u0042\u0001");// White/Black Invertido pg 15
        toPrint = toPrint.Replace("{GSB0}", "\u001d\u0042\u0000");// Normal White/Black Invertido pg 15
        toPrint = toPrint.Replace("{ESC-0}", "\u001b\u002d\u0000");// Finaliza Subrayado Pg 12
        toPrint = toPrint.Replace("{ESC-1}", "\u001b\u002d\u0001");// Inicia Subrayado Pg 12
        toPrint = toPrint.Replace("{ESC-2}", "\u001b\u002d\u0002");// Inicia Subrayado doble Pg 12
        toPrint = toPrint.Replace("{BARC}", "\u001d\u0068\u0070\u001d\u0077\u0003\u001d\u006b\u0049\u000c"); //Barcodes..

        //toPrint = toPrint.Replace("{TABH}","\u001b\u0044\u0002\u0010" + "\u0022\u0000");//Setting Horizontal Tab - Pg. 3-27
        toPrint = toPrint.Replace("{TABH}", "\u001b\u0044\u0002\u0010\u0000");//Setting Horizontal Tab - Pg. 3-27

        toPrint = toPrint.Replace("{TAB9}", "\u0009");// Inicia Subrayado doble Pg 12
        toPrint = toPrint.Replace("{NLN}", "\n");// Cambio de linea
        toPrint = toPrint.Replace("{NLN}", "\n");// Cambio de linea

        toPrint = toPrint.Replace("Ã¡", "\u00e1");
        toPrint = toPrint.Replace("Ã©", "\u00e9");
        toPrint = toPrint.Replace("Ã­", "\u00ed");
        toPrint = toPrint.Replace("Ã³", "\u00f3");
        toPrint = toPrint.Replace("Ãº", "\u00fa");
        toPrint = toPrint.Replace("Ã±", "\u00f1");
        toPrint = toPrint.Replace("Ã‘", "\u00d1");

        return toPrint;
    }

    public void Dispose()
    {
        if (Characteristic is not null)
        {
            Characteristic.OnRaiseCharacteristicValueChanged -= (sender, e) => { };
            Characteristic.StopNotifications();
        }

        Device?.Gatt.Disonnect();

        BluetoothNavigator = null;
        Device = null;
        Characteristic = null;

        GC.SuppressFinalize(this);
    }
}
