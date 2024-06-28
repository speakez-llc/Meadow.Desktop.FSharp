module Secrets

open Meadow

type AppSettingsCollector() =
    inherit ConfigurableObject()
    member this.GetConfiguredString(key: string) =
        let settings = Resolver.App.Settings
        if settings.ContainsKey(key) then
            let value = settings.[key]
            value
        else
            null

let LATITUDE = AppSettingsCollector().GetConfiguredString("WeatherService.Latitude")

let LONGITUDE = AppSettingsCollector().GetConfiguredString("WeatherService.Longitude")

let WEATHER_API_KEY = AppSettingsCollector().GetConfiguredString("WeatherService.Api_Key")
