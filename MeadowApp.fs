namespace Meadow.Desktop.FSharp

open System
open Meadow
open Meadow.Foundation.ICs.IOExpanders
open Meadow.Foundation.Leds
open Meadow.Peripherals.Leds
open System.Threading.Tasks

type MeadowApp() =
    inherit App<Windows>()

    let mutable rgbLed : RgbLed = null

    override this.Initialize() =
        Console.WriteLine("Creating Outputs")

        let expander = FtdiExpanderCollection.Devices.[0]

        rgbLed <- new RgbLed(
            expander.Pins.C2,
            expander.Pins.C1,
            expander.Pins.C0)

        Task.CompletedTask

    override this.Run() =
        let runAsync = async {
            while true do
                Resolver.Log.Info("Going through each color...")
                for i in 0 .. int RgbLedColors.count - 1 do
                    rgbLed.SetColor(unbox<RgbLedColors>(i))
                    do! Task.Delay(500) |> Async.AwaitTask

                do! Task.Delay(1000) |> Async.AwaitTask

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

                do! Task.Delay(1000) |> Async.AwaitTask
        }
        runAsync |> Async.StartAsTask :> Task

    static member Main(args: string[]) =
        MeadowOS.Start(args) |> Async.AwaitTask |> ignore