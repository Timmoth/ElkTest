# Elk Test

ElkTest enables users to debug & test embedded systems using xunit integration tests and a Raspberry Pi Pico.

#### Benefits
- Test production code on the physical device
- No need to write any abstractions around device specific libraries
- Simulating hardware inputs enables you to reliably test your code without physically recreating scenarios
- Easily test networking / http requests
- Device / Platform agnostic

### Setup:

------------

- clone the repo

`git clone https://github.com/Timmoth/ElkTester.git
`
- Add Arduino pico to your board manager (file->preferences->additional board manager urls) paste the following url

`https://github.com/earlephilhower/arduino-pico/releases/download/global/package_rp2040_index.json`

- Open the Elk test firmware in Arduino IDE

`ElkTester/ElkTest/ElkFirmware/ElkFirmware.ino
`
- Plug in the Pico
- Select the device + port 
- Flash the firmware
- Add appsettings.json to test project
- Fill out appsettings.json (ElkSUTDevice is optional)
```json
{
  "ElkTestDevice": {
    "Port": "COM12",
    "BaudRate": 9600
  },
  "ElkSUTDevice": {
    "Port": "COM11",
    "BaudRate": 9600
  }
}
```
- Set appsettings 'copy to output directory' property to 'copy if newer'

### How does it work?

------------
There are two main components to the Elk Test framework:

- The ElkTestDevice is a Raspberry Pi Pico flashed with the ElkTest firmware, each of its GPIO pins can be used to read / write data to / from the system under test. One pin of which is used to reset the SUT at the start of each test. The ElkTestDevice recieves requests from the test runner over a serial connection, these requests allow the tests to set inputs & assert on outputs.

- The ElkApi is a dotnet minimal API configured at the start of each test that allows the SUT connected to the same local network to connect & make requests, the tests can then assert on the requests made.

Tests are written in dotnet using xunit, examples can be seen below.

### Examples:
------------


[Echo](https://github.com/Timmoth/ElkTest/tree/main/Example/Sketches/echo "Echo")

The echo example sets analog pin 28 and asserts that the SUT in turn echos the same value to pin 27, the same logic is tested for digital pins 21 & 20

Flash the firmware to the SUT

`ElkTester/Example/Sketches/echo/echo.ino`

Connect the following pins:

28 -> 28 : analog input pin

27 -> 27 : analog output pin

21 -> 21 : digial input pin

20 -> 20 : digital output pin


The SUT reset is implemented in hardware by connecting an npn transistor collector to the SUT run pin, emitter to ground & base to ElkTestDevice pin 22.

![image](https://user-images.githubusercontent.com/21103223/200683937-6679051d-e61f-4599-9eb8-116fd2d8415c.png)

Ensure both devices are plugged in and configure the ElkTestDevice port in the test projects appsettings.json

`ElkTester/Example/appsettings.json`

```csharp
 //Arrange
var device = await _fixture.Setup(_output); // Connect to the ElkTestDevice & reset the SUT

//Act
await device.SendAndWait(Device.Pins.Set(21, input)); // Set pin 21 high on the ElkTestDevice

await Task.Delay(1000); // Arbitrary wait to ensure sut loop has executed

//Assert
var pinResponse = await device.SendAndWait(Device.Pins.Get(20)); // Read value from pin 20
var actualValue = int.Parse(pinResponse.Arguments[0]);
Assert.Equal(expectedOutput, actualValue);
```


[Sample](https://github.com/Timmoth/ElkTest/blob/main/Example/Sketches/sample "Sample")

Sample digital pin values over time. 



[Serial](https://github.com/Timmoth/ElkTest/tree/main/Example/Sketches/serial "Serial")

Tests writing serial data to the SUT over USB & UART & asserts on the Serial output from the device.



[Network](https://github.com/Timmoth/ElkTest/tree/main/Example/Sketches/network "Network")

The network example makes a sequence of POST, GET, PUT, DELETE requests.

This example uses [HttpRequester](https://github.com/Timmoth/HttpRequester "HttpRequester") to make HttpRequests on the SUT. See project readme for instructions on installing the library.

Copy + Rename `arduino_secrets_example.h` => `arduino_secrets.h`

Update the three arduino_secrets values.
ELKAPI_IP is the local IP address of the machine that runs the tests.

Flash the firmware to the SUT
`ElkTest/Example/Sketches/network/network.ino`

Then run the tests! 

#### External examples

[Outdoor Lights](https://github.com/Timmoth/OutdoorLights "Outdoor Lights")

#### Upcoming
- More complex examples
- Improve documentation
- Test PWM
- Test I2C
- Test Serial
- Hardware pass / fail buttons
