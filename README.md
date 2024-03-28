# Skunk

Air Quality Sensor Package

![Animated Sensors](skunk2.gif)

- VOC - Volitile organic compounds
  - inferred from Eth and H2
- Eth - Ethanol
- CO2 - Carbondioxide
- The red circle is the max value that has been seen
- The grey circle is the lifetime average value

## Setup

![Overview](Overview.png)

- The hardware (an Arduino with attached sensors at this point) communicates via serial connection
  - source/skunk.arduino
  - Outputs sensor data via serial
- .Net application 
  - source/SkunkBackend
  - Hosts the UI as a website
  - Hosts a SignalR Hub to push data to clients
  - Connects to the hardware via serial connection
- Angular SPA
  - source/skunkapp
  - Subscribes via SignalR to data updates

![Architecture](architecture.png)

- Bus is a generic asynchronous Publish/Subscribe interface
  - The bus is currently in-memory, but could easily be replaced by RabbitMq, Kafka, etc to allow cross-application communication
- Horizontal layers structured to allow each to become a separate application and scale independently as needed
- Horizontal layers do not directly interface, but instead Publish/Subscribe to the bus


## Development

To develop the project on raspberry pi, run
> curl -sSL <https://dot.net/v1/dotnet-install.sh> | bash /dev/stdin --channel LTS

then

> echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

## Sensors

### SGP30
Documentation https://github.com/RobTillaart/SGP30 
