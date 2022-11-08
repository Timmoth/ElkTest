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
Todo

#### Http Request
Todo
