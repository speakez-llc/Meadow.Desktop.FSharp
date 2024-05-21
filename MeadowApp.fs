open System
open Meadow
open Meadow.Foundation.ICs.IOExpanders
open Meadow.Foundation.Relays
open Meadow.Foundation.Servos
open Meadow.Hardware
open System.Threading.Tasks

type MeadowApp() =
    inherit App<Windows>()
    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    let mutable rainSensor : IDigitalInputPort = null
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
    
    override this.Initialize() =
        Console.WriteLine("Creating Outputs")

        let expander = FtdiExpanderCollection.Devices[0]
        
        retractRelay <- Relay(expander.Pins.C3)
        stopRelay <- Relay(expander.Pins.C4)
        extendRelay <- Relay(expander.Pins.C5)
        rainSensor <- expander.Pins.C6.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        // add servo assigment here after adding PCA9685

        Task.CompletedTask

    override this.Run () : Task =
        do Resolver.Log.Info "Run... (F#)"
        Task.Run(Func<Task>(fun () -> Async.StartAsTask driveLEDAsync))

module Main =        
    [<EntryPoint>]
    let main args =
        MeadowOS.Start(args) |> Async.AwaitTask |> Async.RunSynchronously
        0
        