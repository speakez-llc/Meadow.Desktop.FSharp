open System
open Meadow
open Meadow.Foundation.ICs.IOExpanders
open Meadow.Foundation.Leds
open Meadow.Foundation.Relays
open Meadow.Foundation.Sensors.Buttons
open Meadow.Foundation.Sensors.Switches
open Meadow.Hardware
open Meadow.Peripherals.Leds
open System.Threading.Tasks

type MeadowApp() =
    inherit App<Windows>()

    let mutable rgbLed : RgbLed = null
    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    
    let mutable rainSensor : IDigitalInputPort = null

    let driveLEDAsync = async {
        while true do
            Resolver.Log.Info("Going through subset of colors...")
            for i in 0 .. (int RgbLedColors.count / 2 ) do
                rgbLed.SetColor(unbox<RgbLedColors>(i))
                do! Task.Delay(250) |> Async.AwaitTask
            rgbLed.IsOn <- false
            
            if rainSensor.State = true  then
                Resolver.Log.Info("Rain Sensor is ON")
            else
                Resolver.Log.Info("Rain Sensor is OFF")
            
            do! Task.Delay(1000) |> Async.AwaitTask
(*            
            Resolver.Log.Info("Setting Retract Relay...")
            retractRelay.Toggle()
            do! Task.Delay(500) |> Async.AwaitTask
            retractRelay.Toggle()
            do! Task.Delay(1500) |> Async.AwaitTask
            
            Resolver.Log.Info("Setting Stop Relay...")
            stopRelay.Toggle()
            do! Task.Delay(500) |> Async.AwaitTask
            stopRelay.Toggle()
            do! Task.Delay(1500) |> Async.AwaitTask
            
            Resolver.Log.Info("Setting Extend Relay...")
            extendRelay.Toggle()
            do! Task.Delay(500) |> Async.AwaitTask
            extendRelay.Toggle()
            do! Task.Delay(1500) |> Async.AwaitTask
           
            Resolver.Log.Info("Setting Stop Relay...")
            stopRelay.Toggle()
            do! Task.Delay(500) |> Async.AwaitTask
            stopRelay.Toggle()
            do! Task.Delay(1500) |> Async.AwaitTask

            Resolver.Log.Info("Blinking through each color (on 500ms / off 500ms)...")
            for i in 0 .. int RgbLedColors.count - 1 do
                do! rgbLed.StartBlink(unbox<RgbLedColors>(i)) |> Async.AwaitTask
                do! Task.Delay(3000) |> Async.AwaitTask
                do! rgbLed.StopAnimation() |> Async.AwaitTask
                rgbLed.IsOn <- false

            do! Task.Delay(1000) |> Async.AwaitTask

            Resolver.Log.Info("Blinking through each color (on 1s / off 1s)...")
            for i in 0 .. int RgbLedColors.count - 1 do
                if rgbLed <> null then
                    do! rgbLed.StartBlink(unbox<RgbLedColors>(i), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)) |> Async.AwaitTask
                do! Task.Delay(3000) |> Async.AwaitTask
                do! rgbLed.StopAnimation() |> Async.AwaitTask
                rgbLed.IsOn <- false

            do! Task.Delay(1000) |> Async.AwaitTask *)
    }
    
    override this.Initialize() =
        Console.WriteLine("Creating Outputs")

        let expander = FtdiExpanderCollection.Devices[0]

        rgbLed <- new RgbLed(
            expander.Pins.C2,
            expander.Pins.C1,
            expander.Pins.C0)
        
        retractRelay <- Relay(expander.Pins.C3)
        stopRelay <- Relay(expander.Pins.C4)
        extendRelay <- Relay(expander.Pins.C5)
        rainSensor <- expander.Pins.C6.CreateDIgitalInputPort(Port.ResistorMode.ExternalPullDown)

        Task.CompletedTask

    override this.Run () : Task =
        do Resolver.Log.Info "Run... (F#)"
        Task.Run(Func<Task>(fun () -> Async.StartAsTask driveLEDAsync))

module Main =        
    [<EntryPoint>]
    let main args =
        MeadowOS.Start(args) |> Async.AwaitTask |> Async.RunSynchronously
        0
        