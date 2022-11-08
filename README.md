# Elk Tester

ElkTester enables users to debug & test embedded systems using xunit integration tests and a Raspberry Pi Pico.

#### Benefits
- Test production code on the physical device.
- No need to write any abstractions around device specific libraries
- Simulating hardware inputs enables you to reliably test your code without physically recreating scenarios
- Easily test networking / http requests

### Setup:

------------

- clone the repo

`git clone https://github.com/Timmoth/ElkTester.git
`
- Add Arduino pico to your board manager (file->preferences->additional board manager urls) paste the following url

`https://github.com/earlephilhower/arduino-pico/releases/download/global/package_rp2040_index.json`

- Open the Elk tester firmware in Arduino IDE

`ElkTester/ElkTest/ElkFirmware/ElkFirmware.ino
`
- Plug in the Pico
- Select the device + port 
- Flash the firmware

### How does it work?

------------
There are two main components to the Elk Tester framework:

- The ElkTestDevice is a Raspberry Pi Pico flashed with the ElkTester firmware, each of its GPIO pins can be used to read / write data to / from the system under test. One pin of which is used to reset the SUT. The ElkTestDevice recieves requests from the test runner over a serial connection, these requests allow the tests to set inputs / read values on the SUT.

- The ElkApi is a dotnet minimal API configured at the start of each test that allows the SUT connected to the same local network to connect & make requests, and the tests can then assert on the requests made.

The tests are written in dotnet using xunit an example of a test for a device that reads the digital input for one pin and writes it to another can be seen here:

```csharp
//Arrange
var device = await _fixture.Setup(_output);// Connect to the ElkTestDevice & reset the SUT

//Act
await device.SendAndWait(Device.Pins.Set(21, 1)); // Set pin 21 high on the ElkTestDevice

await Task.Delay(1000); // Arbitrary wait time

//Assert
var pinResponse = await device.SendAndWait(Device.Pins.Get(20)); // Read value from pin 20
var actualValue = int.Parse(pinResponse.Arguments[0]);
Assert.InRange(actualValue, 1);` // Assert expected value
```

### Examples:
------------

#### Echo
The echo example sets analog pin 28 and asserts that the SUT in turn echos the same value to pin 27, the same logic is tested for digital pins 21 & 20

Flash the firmware to the SUT

`ElkTester/Example/Sketches/echo/echo.ino`

Connect the following pins:
28 -> 28 : analog input pin
27 -> 27 : analog output pin
21 -> 21 : digial input pin
20 -> 20 : digital output pin

The test device reset is implemented in hardware by connecting an npn transistor collector to the SUT run pin, emitter to SUT ground & base to ElkTestDevice pin 22.

![image](https://user-images.githubusercontent.com/21103223/200681563-d62551f0-ffd3-410e-8cc9-0e8ac0e2a06b.png)

Ensure both devices are plugged in and configure the ElkTestDevice port in the test projects appsettings.json

`ElkTester/Example/appsettings.json`

#### Http Request
Todo



#### Upcoming features
- Test PWM
- Test I2C
