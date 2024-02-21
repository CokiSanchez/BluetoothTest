using BluetoothTest.Shared.BluetoothService.Interfaces;
using BluetoothTest.Shared.BluetoothService.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;

namespace BluetoothTest.Pages;

public partial class Index : IDisposable
{
    [Inject] public IJSRuntime JS { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IBluetoothNavigator? BluetoothNavigator { get; set; }
    private IDevice? Device { get; set; }
    private IBluetoothRemoteGATTCharacteristic? Characteristic { get; set; }

    private bool Available { get; set; } = false;
    private List<string> Logs { get; set; } = new();
    private string Error { get; set; } = string.Empty;
    private string Text { get; set; } = string.Empty;

    private readonly string Parte = "{ESC@}{ESCR7}{ESCE1}{ESCa0}{ESC-1}{ESCa1}{ESC-0} ILUSTRé  ñUNICIPALIDAD N° DE VITACURA {NLN} INSPECCION MUNICIPAL {NLN}{NLN}{GSB1} Citacion_Tipo {GSB0}{ESC-1}{ESCa2} {ESC-0}{NLN}{ESCa2} Nº Citacion: Citacion_IdNrPedido{NLN}{ESCa0}{TABH} Vitacura, Fecha/Hora:{TAB9}{ESCE0}Gen_Fecha Gen_Hora HRS.{NLN}{NLN}{ESCE1}{ESC-2}VEHICULO{ESC-0}{ESCE0}{NLN}{ESCE1}{ESCE0}Placa:{TAB9}Transito_Placa{NLN}Marca:{TAB9}Transito_Marca{NLN}Modelo:{TAB9}Transito_Modelo{NLN}Color:{TAB9}Transito_Color{NLN}Tipo Vehiculo:{TAB9}Transito_TipoVehiculo{NLN}{NLN}{ESCE1}{ESC-2}FISCALIZACION{ESC-0}{ESCE0}{NLN}Infraccion:{NLN}Citacion_Infracciones_Ind{NLN}- LUGAR:{TAB9}Transito_Lugar{NLN}- OBSERVACIONES:{NLN}{NLN}Citacion_Infracciones_Obs{NLN}{NLN}{ESCE1}{ESC-2}CITACION{ESC-0}{ESCE0}{NLN}CITO A UD AL Citacion_Juzgado, UBICADO EN {ESCE1}Citacion_DirJuzgado{ESCE0}.{NLN}PARA LA AUDENCIA DEL {ESCE1}Citacion_FechaCitacion A LAS {ESCE1}Citacion_HoraCitacion{ESCE0} HRS.{NLN}{NLN}SI EL DIA FIJADO  NO COMPARECIERE, SERA JUZGADO EN REBELDIA CONFORME A LA LEY.{NLN}{NLN}RECIBIDO POR: {TAB9}Citacion_Nombre{NLN}{NLN}-INSPECTOR:{TAB9}Gen_NombreInspector1{NLN}{NLN}Nº INTERNO: Citacion_NrNotif{NLN}";

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
            var query = new RequestDeviceQuery
            {
                AcceptAllDevices = false,
                Filters = new List<Filter>
                {
                    new()
                    {
                        Name = "RPP320-3016-B",
                        NamePrefix = null,
                    }
                },
                OptionalServices = new()
            };

            query.OptionalServices.Add("00001800-0000-1000-8000-00805f9b34fb");
            query.OptionalServices.Add("0000180a-0000-1000-8000-00805f9b34fb");
            query.OptionalServices.Add("000018f0-0000-1000-8000-00805f9b34fb");
            query.OptionalServices.Add("0000ffe0-0000-1000-8000-00805f9b34fb");

            Logs.Add($"{DateTime.Now:HH:mm} - Buscando...");

            Device = await BluetoothNavigator.RequestDevice(query);

            if (Device is not null)
                Logs.Clear();
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
                await Device.Gatt.Disonnect();

                Logs.Add($"{DateTime.Now:HH:mm} - Dispositivo {Device.Name} {Device.Id} desconectado.");
                Device = null;
                Logs.Clear();
            }
        }
        catch (Exception e)
        {
            Error = e.Message;
            Logs.Add($"{DateTime.Now:HH:mm} - Error: {e.Message}.");
        }
    }

    private async Task ComenzarServicios()
    {
        try
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Buscando servicios para 0000ffe0-0000-1000-8000-00805f9b34fb.");

            if (Device is null)
                return;

            if (!Device.Gatt.Connected)
                return;

            var service = await Device.Gatt.GetPrimaryService("0000ffe0-0000-1000-8000-00805f9b34fb");

            if (service is null)
            {
                Logs.Add($"{DateTime.Now:HH:mm} - Servicio 0000ffe0-0000-1000-8000-00805f9b34fb no detectado.");
                return;
            }

            Logs.Add($"{DateTime.Now:HH:mm} - Servicio {(service.IsPrimary ? "primario" : "")} {service.Uuid} detectado.");

            Characteristic = await service.GetCharacteristic("0000ffe1-0000-1000-8000-00805f9b34fb");

            if (Characteristic is null)
            {
                Logs.Add($"{DateTime.Now:HH:mm} - Caracteristica '0000ffe1-0000-1000-8000-00805f9b34fb' no encontrada.");
                return;
            }

            Logs.Add($"{DateTime.Now:HH:mm} - Detectada caracteristica {Characteristic.Uuid}.");
        }
        catch (Exception e)
        {
            Error = e.Message;
            Logs.Add($"{DateTime.Now:HH:mm} - Error: {e.Message}.");
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
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
            Error = e.Message;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        //await PruebaImagen1();

        if (BluetoothNavigator is null)
            return;

        BluetoothNavigator.OnAvailabilityChanged += async () => await OnAvailabilityChanged();
        await BuscarServicio();
    }

    private const string Nombre = "prueba.png";

    private async Task PruebaImagen1()
    {
        try
        {
            var (bytes, ancho, alto) = await ObtenerDatosImagen(Nombre);

            var comandos = CapturaDatosImagen(bytes, ancho, alto);
            //var pixels = GetPixelValues(bytes, ancho);

            //var init = new byte[] { 0x1B, 0x2A, 0x21, (byte)(ancho % 256), (byte)Math.Floor((decimal)ancho / 256) };
            var init = new byte[] { 0x1B, 0x2A, 0x21, (byte)(2 % 256), (byte)Math.Floor((decimal)2 / 256) };

            await Characteristic.WriteValueWithoutResponse(init);

            var b = new byte[] { 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00 };

            foreach (var chunk in b.Chunk(3))
            {
                await Characteristic.WriteValueWithoutResponse(chunk);
            }

            await Characteristic.WriteValueWithoutResponse(new byte[] { 0x0A });

            //await Characteristic.WriteValueWithoutResponse(comandos.ToArray());
            //await Characteristic.WriteValueWithoutResponse(pixels.ToArray());
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
        }
    }

    public async Task<byte[]> CargarImagenComoBytes()
    {
        try
        {
            var imageUrl = $"{NavigationManager.BaseUri}img/{Nombre}";
            var byteArray = await JS.InvokeAsync<byte[]>("cargarImagenComoBytes", imageUrl);
            return byteArray;
        }
        catch (Exception ex)
        {
            // Manejar el error
            Console.WriteLine($"Error al cargar la imagen: {ex.Message}");
            return null;
        }
    }

    public static List<byte> GetPixelValues(byte[] imageData, int ancho)
    {
        var pixelValues = new List<byte>
        {
            0x1B,
            0x2A,
            0x21,
            (byte)(ancho / 8),
            (byte)(ancho / 8 >> 8)
        };

        foreach (byte pixel in imageData)
        {
            // Si el valor del byte es mayor que 128, consideramos que es un píxel negro (0xFF), de lo contrario, es blanco (0x00)
            byte pixelValue = pixel > 128 ? (byte)0xFF : (byte)0x00;
            pixelValues.Add(pixelValue);
        }

        pixelValues.Add(0x0A);

        return pixelValues;
    }

    private List<byte> CapturaDatosImagen(byte[] bytes, int ancho, int alto)
    {
        var commands = new List<byte>();

        for (int i = 0; i < alto; i++)
        {
            for (int j = 0; j < ancho / 8; j++)
            {
                byte data = 0x00;
                for (int k = 0; k < 8; k++)
                {
                    int x = j * 8 + k;
                    int y = i;
                    int pixelIndex = y * ancho + x;
                    if (pixelIndex < bytes.Length && bytes[pixelIndex] == 0xFF)
                    {
                        data |= (byte)(0x80 >> k);
                    }
                }
                commands.Add(data);
            }
        }

        return commands;
    }

    private async Task PruebaImagen2()
    {
        try
        {
            var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(NavigationManager.BaseUri)
            };

            var stream = await _httpClient.GetStreamAsync($"/img/{Nombre}");
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            var dimension = await ObtenerDatosImagen(Nombre);

            var data = GenerateImageCommands(bytes, dimension.Ancho, dimension.Alto);
            await Characteristic.WriteValueWithoutResponse(data);
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
        }
    }

    public byte[] GenerateImageCommands(byte[] imageData, int width, int height)
    {
        var commands = new List<byte>();

        // ESC * m nL nH d1...dk (Print raster bit image)
        commands.Add(0x1B); // ESC
        commands.Add(0x2A); // *
        commands.Add(0x21); // m (value 0, 48 dot-density)
        //commands.Add((byte)(width / 8)); // nL (image width in bytes)
        //commands.Add((byte)(width / 8 >> 8)); // nH

        commands.Add((byte)0x0F); // 15 mod 256 = 15
        commands.Add((byte)0x00); // 15/256 = 0

        // H
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        // O
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        // L
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x0F);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x0F);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        // A
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0xF0);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        commands.Add((byte)0x0A);

        //-------------------------
        commands.Add(0x1B); // ESC
        commands.Add(0x2A); // *
        commands.Add(0x21); // m (value 0, 48 dot-density)
        //commands.Add((byte)(width / 8)); // nL (image width in bytes)
        //commands.Add((byte)(width / 8 >> 8)); // nH

        commands.Add((byte)0x1E); // 30 mod 256 = 30
        commands.Add((byte)0x00); // 30/256 = 0

        // H
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        // O
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        // L
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        // A
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xF0);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xF0);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);
        commands.Add((byte)0xFF);

        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);
        commands.Add((byte)0x00);

        commands.Add((byte)0x0A);

        //for (int i = 0; i < height; i++)
        //{
        //    for (int j = 0; j < width / 8; j++)
        //    {
        //        byte data = 0x00;
        //        for (int k = 0; k < 8; k++)
        //        {
        //            int x = j * 8 + k;
        //            int y = i;
        //            int pixelIndex = y * width + x;
        //            if (pixelIndex < imageData.Length && imageData[pixelIndex] == 0xFF)
        //            {
        //                data |= (byte)(0x80 >> k);
        //            }
        //        }
        //        commands.Add(data);
        //    }
        //}

        return commands.ToArray();
    }

    private async Task PruebaImagen3()
    {
        try
        {
            var imageUrl = $"{NavigationManager.BaseUri}img/{Nombre}";
            var escPosData = await JS.InvokeAsync<object[]>("convertImageToESCPOS", imageUrl);

            var jsonString = JsonSerializer.Serialize(escPosData);

            var data = Encoding.UTF8.GetBytes(jsonString);

            var ancho = (await ObtenerDatosImagen(Nombre)).Ancho;

            var n1 = BitConverter.GetBytes(ancho % 256);
            var n2 = BitConverter.GetBytes((int)Math.Floor((decimal)ancho / 256));

            //await Characteristic.WriteValueWithoutResponse(new byte[] { 0x1B, 0x2A, 33 });
            //await Characteristic.WriteValueWithoutResponse(new byte[]{ 27, 42, 33});
            //await Characteristic.WriteValueWithoutResponse(n1);
            //await Characteristic.WriteValueWithoutResponse(n2);
            //await Characteristic.WriteValueWithoutResponse(new byte[] { 0x1B, 0x2A, 0x21, 0x02, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00 });

            foreach (var chunk in data.Chunk(24))
                await Characteristic.WriteValueWithoutResponse(chunk);
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
        }
    }

    [JSInvokable]
    public async Task<(byte[] Bytes, int Ancho, int Alto)> ObtenerDatosImagen(string nombre)
    {
        var _httpClient = new HttpClient
        {
            BaseAddress = new Uri(NavigationManager.BaseUri)
        };

        var stream = await _httpClient.GetStreamAsync($"/img/{nombre}");
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] bytes = memoryStream.ToArray();

        var imageUrl = $"{NavigationManager.BaseUri}img/{nombre}";
        var dimensiones = await JS.InvokeAsync<int[]>("ObtenerDimensionesImagen", imageUrl);

        var ancho = 0;
        var alto = 0;

        if (dimensiones != null && dimensiones.Length == 2)
        {
            ancho = Convert.ToInt32(dimensiones[0]);
            alto = Convert.ToInt32(dimensiones[1]);
        }

        return (bytes, ancho, alto);
    }

    private async Task Enviar()
    {
        if (Characteristic is null)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Caracteristica '0000ffe1-0000-1000-8000-00805f9b34fb' no encontrada.");
            return;
        }

        try
        {
            var lineas = string.IsNullOrEmpty(Text) ? Parte.Split("{NLN}") : Text.Split("{NLN}");

            foreach (var (linea, index) in lineas.Select((linea, index) => (linea, index)))
            {
                var chunk = Formatear($"{linea} {{NLN}} ");

                Logs.Add($"{DateTime.Now:HH:mm} - [{index + 1}/{lineas.Length}] Se envia a imprimir {chunk.Length} de tamaño");

                await Characteristic.WriteValueWithoutResponse(chunk);
            }
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - No se puede enviar. {e.Message}");
        }
    }

    private static byte[] Formatear(string text)
    {
        return Encoding.ASCII.GetBytes(PrintDriver(text));
    }

    private static string PrintDriver(string toPrint)
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
        toPrint = toPrint.Replace("{ESCR7}", "\u001b\u0074\u0039");// Caracteres Spain
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

    public struct Hexadecimal
    {
        private readonly int value;

        public Hexadecimal(int value)
        {
            this.value = value;
        }

        public readonly string ToHexString()
        {
            string hex = "";
            int remainder;
            int quotient = value;

            do
            {
                remainder = quotient % 16;
                quotient /= 16;
                hex = (remainder < 10 ? (char)(remainder + '0') : (char)(remainder - 10 + 'A')) + hex;
            } while (quotient != 0);

            return hex;
        }
    }

    public void Dispose()
    {
        Device?.Gatt.Disonnect();

        BluetoothNavigator = null;
        Device = null;
        Characteristic = null;

        GC.SuppressFinalize(this);
    }
}
