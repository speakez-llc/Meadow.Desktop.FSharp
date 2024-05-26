open System
open Meadow
open Meadow.Foundation.ICs.IOExpanders
open Meadow.Foundation.Relays
open Meadow.Foundation.Servos
open Meadow.Hardware
open System.Threading.Tasks
open Meadow.Peripherals.Sensors.Location
open Meadow.Units

type MeadowApp() =
    inherit App<Windows>()
    let mutable pca9685 : Pca9685 = null
    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    let mutable rainSensor : IDigitalInputPort = null
    let mutable port0 : IPwmPort = null
    let mutable wiperServo : Servo = null
    
    let retractAwning =
        async {
            Resolver.Log.Info("Setting Retract Relay...")
            retractRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            retractRelay.Toggle()
        }
            
    let expandAwning =
        async {
            Resolver.Log.Info("Setting Extend Relay...")
            extendRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            extendRelay.Toggle()
        }
            
    let stopAwning =
        async {
            Resolver.Log.Info("Setting Stop Relay...")
            stopRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            stopRelay.Toggle()
        }
            

    let driveLEDAsync = async {
        while true do
            
            if rainSensor.State = true  then
                Resolver.Log.Info("Rain Sensor is ON")
                retractAwning |> Async.Start
                do! Task.Delay(2000) |> Async.AwaitTask
                stopAwning |> Async.Start
                do! Task.Delay(2000) |> Async.AwaitTask
            else
                Resolver.Log.Info("Rain Sensor is OFF")
            
            do! Task.Delay(3000) |> Async.AwaitTask
    }
    
    let runServoAsync = async {
        while true do
            wiperServo.RotateTo(new Angle(90.0, Angle.UnitType.Degrees)) |> ignore
            do! Task.Delay(1000) |> Async.AwaitTask
            wiperServo.RotateTo(new Angle(0.0, Angle.UnitType.Degrees)) |> ignore
            do! Task.Delay(1000) |> Async.AwaitTask
    }
    
    override this.Initialize() =
        Console.WriteLine("Creating Outputs")
        let expander = FtdiExpanderCollection.Devices[0]
        retractRelay <- Relay(expander.Pins.C3)
        stopRelay <- Relay(expander.Pins.C4)
        extendRelay <- Relay(expander.Pins.C5)
        rainSensor <- expander.Pins.C6.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        
        let i2cBus = expander.CreateI2cBus(expander.Pins.D0, expander.Pins.D1, I2cBusSpeed.Standard)
        pca9685 <- Pca9685(i2cBus, Frequency(240.0, Frequency.UnitType.Hertz), Convert.ToByte(64))
        port0 <- pca9685.CreatePwmPort(Convert.ToByte(0), 0.05f)
        wiperServo <- Servo(port0, NamedServoConfigs.SG90)

        Task.CompletedTask

    override this.Run () : Task =
        do Resolver.Log.Info "Run... (F#)"
        Task.Run(Func<Task>(fun () -> Async.StartAsTask runServoAsync))

module Main =        
    [<EntryPoint>]
    let main args =
        MeadowOS.Start(args) |> Async.AwaitTask |> Async.RunSynchronously
        0
        