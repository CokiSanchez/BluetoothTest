﻿using BluetoothTest.Shared.BluetoothService.Interfaces;
using BluetoothTest.Shared.BluetoothService.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

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

    private readonly string Parte = "{ESC@}{ESCR7}{ESCE1}{ESCa0}{ESC-1}{ESCa1}{ESC-0} ILUSTRE ñññññ hólá MUNICIPALIDAD DE VITACURA {NLN} INSPECCION MUNICIPAL {NLN}{NLN}{GSB1} Citacion_Tipo {GSB0}{ESC-1}{ESCa2} {ESC-0}{NLN}{ESCa2} Nº Citacion: Citacion_IdNrPedido{NLN}{ESCa0}{TABH} Vitacura, Fecha/Hora:{TAB9}{ESCE0}Gen_Fecha Gen_Hora HRS.{NLN}{NLN}{ESCE1}{ESC-2}VEHICULO{ESC-0}{ESCE0}{NLN}{ESCE1}{ESCE0}Placa:{TAB9}Transito_Placa{NLN}Marca:{TAB9}Transito_Marca{NLN}Modelo:{TAB9}Transito_Modelo{NLN}Color:{TAB9}Transito_Color{NLN}Tipo Vehiculo:{TAB9}Transito_TipoVehiculo{NLN}{NLN}{ESCE1}{ESC-2}FISCALIZACION{ESC-0}{ESCE0}{NLN}Infraccion:{NLN}Citacion_Infracciones_Ind{NLN}- LUGAR:{TAB9}Transito_Lugar{NLN}- OBSERVACIONES:{NLN}{NLN}Citacion_Infracciones_Obs{NLN}{NLN}{ESCE1}{ESC-2}CITACION{ESC-0}{ESCE0}{NLN}CITO A UD AL Citacion_Juzgado, UBICADO EN {ESCE1}Citacion_DirJuzgado{ESCE0}.{NLN}PARA LA AUDENCIA DEL {ESCE1}Citacion_FechaCitacion A LAS {ESCE1}Citacion_HoraCitacion{ESCE0} HRS.{NLN}{NLN}SI EL DIA FIJADO  NO COMPARECIERE, SERA JUZGADO EN REBELDIA CONFORME A LA LEY.{NLN}{NLN}RECIBIDO POR: {TAB9}Citacion_Nombre{NLN}{NLN}-INSPECTOR:{TAB9}Gen_NombreInspector1{NLN}{NLN}Nº INTERNO: Citacion_NrNotif{NLN}";

    protected override async Task OnInitializedAsync()
    {
        //await PruebaImagen2();

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

    private async Task PruebaImagen1()
    {
        try
        {
            var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(NavigationManager.BaseUri)
            };

            var stream = await _httpClient.GetStreamAsync($"/img/cat.png");
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            var data = TransformarImagen(bytes, 100);

            await Characteristic.WriteValueWithoutResponse(data);
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
        }
    }

    private async Task PruebaImagen2()
    {
        try
        {
            var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(NavigationManager.BaseUri)
            };

            var stream = await _httpClient.GetStreamAsync($"/img/cat.png");
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            var dimension = await ObtenerDimensionesImagen();

            var ancho = dimension.Item1;
            var alto = dimension.Item2;

            var n1 = Convert.ToByte((ancho % 256).ToString("X"));
            var n2 = Convert.ToByte(((int)Math.Floor((decimal)alto / 256)).ToString("X"));

            byte[] imageMode = { 0x1B, 0x2A, 33 };
            byte[] n = { n1, n2 };
            var data = ConvertirImagenAHex(bytes);

            await Characteristic.WriteValueWithoutResponse(imageMode);
            await Characteristic.WriteValueWithoutResponse(n);
            await Characteristic.WriteValueWithoutResponse(n);
        }
        catch (Exception e)
        {
            Logs.Add($"{DateTime.Now:HH:mm} - Error {e.Message}.");
        }
    }

    public static string[] ConvertirImagenAHex(byte[] imagenBytes)
    {
        string[] hexColumnas = new string[imagenBytes.Length];

        for (int i = 0; i < imagenBytes.Length; i++)
        {
            hexColumnas[i] = imagenBytes[i].ToString("X2");
        }

        return hexColumnas;
    }

    public byte[]? TransformarImagen(byte[] imageBytes, int maxWidth)
    {
        try
        {
            // Calcular el ancho y la altura de la imagen
            int imageWidth = maxWidth;
            int imageHeight = imageBytes.Length / (maxWidth / 8); // 1 byte por cada 8 píxeles

            // Crear el arreglo de bytes para la imagen ESC/POS
            byte[] escPosImage = new byte[8 + (imageWidth * imageHeight)];

            // Encabezado de comando ESC/POS para imprimir imagen
            escPosImage[0] = 0x1B; // ESC
            escPosImage[1] = 0x2A; // '*'
            escPosImage[2] = 0x00; // Subcomando
            escPosImage[3] = 0x58; // 'X'
            escPosImage[4] = (byte)(imageWidth % 256); // Ancho de la imagen (LSB)
            escPosImage[5] = (byte)(imageWidth / 256); // Ancho de la imagen (MSB)
            escPosImage[6] = (byte)(imageHeight % 256); // Altura de la imagen (LSB)
            escPosImage[7] = (byte)(imageHeight / 256); // Altura de la imagen (MSB)

            // Convertir la imagen a formato ESC/POS
            int dataIndex = 8;
            for (int i = 0; i < imageBytes.Length; i += (maxWidth / 8))
            {
                // Copiar los bytes de la imagen a la matriz ESC/POS
                Array.Copy(imageBytes, i, escPosImage, dataIndex, Math.Min(maxWidth / 8, imageBytes.Length - i));
                dataIndex += maxWidth / 8;
            }

            return escPosImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al convertir la imagen a ESC/POS: " + ex.Message);
            return null;
        }
    }

    private async Task<Tuple<int, int>> ObtenerDimensionesImagen()
    {
        var imagenUrl = $"{NavigationManager.BaseUri}img/cat.png";
        return await ObtenerDimensionesImagenJavaScript(imagenUrl);
    }

    [JSInvokable]
    public async Task<Tuple<int, int>> ObtenerDimensionesImagenJavaScript(string imageUrl)
    {
        var dimensiones = await JS.InvokeAsync<int[]>("ObtenerDimensionesImagen", imageUrl);

        var ancho = 0;
        var alto = 0;

        if (dimensiones != null && dimensiones.Length == 2)
        {
            ancho = Convert.ToInt32(dimensiones[0]);
            alto = Convert.ToInt32(dimensiones[1]);
        }

        return new Tuple<int, int>(ancho, alto);
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
