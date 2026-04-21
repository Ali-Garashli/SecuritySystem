// To run:
// dotnet run -- /dev/cu.usbmodem1301
// or
// dotnet run -- /dev/cu.usbmodem1301 9600 http://localhost:5180

using System.IO.Ports;
using System.Net.Http.Json;
using System.Text.Json;
using SecurityDashboard.Models;
using SecurityDashboard.DTO;

// CONFIG
// assign default values
string serialPort = DetectArduinoPort();
int baudRate = 9600;
string apiBase = "http://localhost:5180";

// assign values from arguments
switch (args.Length) {
    case 1:
        serialPort = args[0];
        break;
    case 2:
        baudRate = int.Parse(args[1]);
        break;
    case 3:
        apiBase = args[2];
        break;
}

Console.WriteLine($"Serial port: {serialPort}");
Console.WriteLine($"Baud rate: {baudRate}");
Console.WriteLine($"API base: {apiBase}\n");

// HTTP client
using HttpClient http = new() {
    BaseAddress = new Uri(apiBase)
};
http.Timeout = TimeSpan.FromSeconds(5);

// SERIAL PORT
using SerialPort serial = new(serialPort, baudRate) {
    ReadTimeout = 2000,
    WriteTimeout = 2000,
    NewLine = "\n"
};

try {
    serial.Open();
    Console.WriteLine("Serial port opened. Waiting for Arduino.");

    // give arduino time to reset itself after opening serial
    await Task.Delay(2000);
}
catch (Exception ex) {
    Console.Error.WriteLine($"Failed to open serial port: {ex.Message}");
    Console.Error.WriteLine("Available ports: " + string.Join(", ", SerialPort.GetPortNames()));
    return;
}

// MAIN LOOP
CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, e) => {
    // prevent sudden cancellation of the program
    // this let's the loop finish before closing
    e.Cancel = true; cancellationTokenSource.Cancel();
};

bool lastArmedState = true;

while (!cancellationTokenSource.IsCancellationRequested) {
    string line;
    try {
        line = serial.ReadLine().Trim();
    }
    catch (TimeoutException) {
        continue; // keep waiting for arduino to connect
    }
    catch (Exception ex) {
        Console.Error.WriteLine($"Serial read error: {ex.Message}");
        break;
    }

    // if we don't get the expected format
    if (!line.StartsWith("G:")) {
        Console.WriteLine($"Unknown line ignored: {line}");
        continue;
    }

    if (!TryParseSensorLine(line,
                            out int gas,
                            out int flame,
                            out int motion)) {
        Console.Error.WriteLine($"Couldn't parse: {line}");
        continue;
    }

    Console.WriteLine($"Received input: [Gas:{gas}  Flame:{flame}  Motion:{motion}]");

    // post all sensors
    bool armedState = lastArmedState;
    armedState = await PostSensorAsync(http, SensorType.Gas, gas, armedState);
    armedState = await PostSensorAsync(http, SensorType.Flame, flame, armedState);
    armedState = await PostSensorAsync(http, SensorType.Motion, motion, armedState);

    // send armed state back to arduino only when it changes
    if (armedState != lastArmedState) {
        Console.WriteLine($"Armed state changed to: {(armedState ? "ARMED" : "DISARMED")}");
        lastArmedState = armedState;
    }

    bool buzzerActive = await GetBuzzerActiveAsync(http, armedState);

    try {
        serial.WriteLine($"A:{(armedState ? 1 : 0)}");
        serial.WriteLine($"B:{(buzzerActive ? 1 : 0)}");
        Console.WriteLine($"Bridge -> A:{(armedState ? 1 : 0)}  B:{(buzzerActive ? 1 : 0)}");
    }
    catch (Exception ex) {
        Console.Error.WriteLine($"Serial write error: {ex.Message}");
    }
}

Console.WriteLine("Shutting down the bridge.");
serial.Close();

// EXTRA METHODS
static async Task<bool> GetBuzzerActiveAsync(HttpClient http, bool fallback) {
    try {
        HttpResponseMessage response = await http.PostAsync("api/sensor/buzzer", null);
        if (!response.IsSuccessStatusCode) return fallback;

        using JsonDocument jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        if (jsonDocument.RootElement.TryGetProperty("active", out JsonElement jsonValue))
            return jsonValue.GetBoolean();
    }
    catch (Exception ex) {
        Console.Error.WriteLine($"Error with buzzer: {ex.Message}");
    }

    // return default given value if we fail
    return fallback;
}

static bool TryParseSensorLine(string line,
                               out int gas,
                               out int flame,
                               out int motion) {
    gas = flame = motion = 0;

    int flameMarker = line.IndexOf(",F:");
    int motionMarker = line.IndexOf(",M:");

    // return if F or M values are not given
    if (flameMarker < 0 || motionMarker < 0)
        return false;

    string gasStr = line.Substring(2, flameMarker - 2);
    string flameStr = line.Substring(flameMarker + 3, motionMarker - (flameMarker + 3));
    string motionStr = line.Substring(motionMarker + 3);

    return int.TryParse(gasStr, out gas) &&
           int.TryParse(flameStr, out flame) &&
           int.TryParse(motionStr, out motion);
}

static async Task<bool> PostSensorAsync(HttpClient http,
                                        SensorType sensorType,
                                        int value,
                                        bool fallbackArmed) {
    SensorReadingDto dto = new() {
        SensorType = sensorType,
        ReadingValue = value
    };

    try {
        HttpResponseMessage response = await http.PostAsJsonAsync("api/sensor/reading", dto);

        if (!response.IsSuccessStatusCode) {
            Console.Error.WriteLine($"API error for {sensorType}: HTTP {response.StatusCode}");
            return fallbackArmed;
        }

        using JsonDocument jsonDocument =
            await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        if (jsonDocument.RootElement.TryGetProperty("systemArmed", out JsonElement jsonValue))
            return jsonValue.GetBoolean();
    }
    catch (HttpRequestException ex) {
        Console.Error.WriteLine($"HTTP error for {sensorType}: {ex.Message}");
    }
    catch (Exception ex) {
        Console.Error.WriteLine($"Error for {sensorType}: {ex.Message}");
    }

    // return default given value if we fail
    return fallbackArmed;
}

// detect the port automatically
static string DetectArduinoPort() {
    string[] ports = SerialPort.GetPortNames();

    // MacOS
    foreach (string port in ports)
        if (port.Contains("usbmodem", StringComparison.OrdinalIgnoreCase) ||
            port.Contains("usbserial", StringComparison.OrdinalIgnoreCase))
            return port;

    // Linux
    foreach (string port in ports)
        if (port.StartsWith("/dev/ttyACM") ||
            port.StartsWith("/dev/ttyUSB"))
            return port;

    // Windows
    if (ports.Length > 0)
        return ports[0];

    Console.Error.WriteLine("No serial ports found. Enter port manually.");
    Console.Error.WriteLine("Example: dotnet run -- /dev/cu.usbmodem1301");
    Environment.Exit(1);
    return string.Empty;
}