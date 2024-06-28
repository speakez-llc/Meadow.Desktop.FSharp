open System
open Meadow
open Meadow.Foundation.Relays
open Meadow.Foundation.Servos
open Meadow.Foundation.ICs.IOExpanders
open Meadow.Hardware
open System.Threading.Tasks
open Meadow.Units
open WeatherService

type MeadowApp() =
    inherit App<Windows>()    
    //let mutable expander = FtdiExpanderCollection.Devices[0]
    let mutable i2CBus = null
    //let mutable pca9685 : Pca9685 = null
    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    //let mutable rainSensor : IDigitalInputPort = null
    //let mutable port0 : IPwmPort = null
    //let mutable wiperServo : AngularServo = null
    
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
            

    (*let driveLEDAsync = task {
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
    }*)
    
    (*let runServoTask() = task {
        do wiperServo.RotateTo(Angle 0) 

        while true do
            for i in 0 .. int wiperServo.MaximumAngle.Degrees do
                do wiperServo.RotateTo(Angle i)
                Resolver.Log.Info(sprintf "Rotating to %d" i)

            do! Task.Delay(1000) |> Async.AwaitTask

            for i in [180 .. -1 .. int wiperServo.MinimumAngle.Degrees] do
                do wiperServo.RotateTo(Angle i) 
                Resolver.Log.Info(sprintf "Rotating to %d" i)

            do! Task.Delay(1000) |> Async.AwaitTask
    }*)
    
    override this.Initialize() =
        Console.WriteLine("Creating Outputs")
        (*retractRelay <- Relay(expander.Pins.C3)
        stopRelay <- Relay(expander.Pins.C4)
        extendRelay <- Relay(expander.Pins.C5)
        rainSensor <- expander.Pins.C6.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        i2CBus <- expander.CreateI2cBus()*)
        
        (*pca9685 <- new Pca9685(i2CBus, Frequency(50, Frequency.UnitType.Hertz));
        port0 <- pca9685.CreatePwmPort(pca9685.Pins.LED0, 0.05f)
        wiperServo <- new Sg90(port0)*)

        Task.CompletedTask

    override this.Run () : Task =
        Resolver.Log.Info "Run... (F#)"
        let task1 = Task.Run(Func<Task>(fun () -> GetWeatherConditions()))
        task1.Wait()
        let task2 = Task.Run(Func<Task>(fun () -> GetWeatherForecast()))
        task2.Wait()
        Task.CompletedTask

module Main =        
    [<EntryPoint>]
    let main args =
        MeadowOS.Start(args) |> Async.AwaitTask |> Async.RunSynchronously
        0
        