# Elk Test

ElkTest enables users to debug & test embedded systems using xunit integration tests and a Raspberry Pi Pico.

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

- Open the Elk test firmware in Arduino IDE

`ElkTester/ElkTest/ElkFirmware/ElkFirmware.ino
`
- Plug in the Pico
- Select the device + port 
- Flash the firmware

### How does it work?

------------
There are two main components to the Elk Test framework:

- The ElkTestDevice is a Raspberry Pi Pico flashed with the ElkTest firmware, each of its GPIO pins can be used to read / write data to / from the system under test. One pin of which is used to reset the SUT at the start of each test. The ElkTestDevice recieves requests from the test runner over a serial connection, these requests allow the tests to set inputs & assert on outputs.

- The ElkApi is a dotnet minimal API configured at the start of each test that allows the SUT connected to the same local network to connect & make requests, the tests can then assert on the requests made.

Tests are written in dotnet using xunit, examples can be seen below.

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


#### Http Request
Todo



#### Upcoming
- More complex examples
- Improve documentation
- Test PWM
- Test I2C
