module WeatherService

open System
open System.Threading.Tasks
open System.Net.Http
open FSharp.Json
open Meadow
open WeatherReading
open Secrets

let epoch = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
let localDateTime (unixTimestamp : int64) =
    epoch.AddSeconds(float unixTimestamp).ToLocalTime()

let climateDataUri = "https://api.openweathermap.org/data/2.5/weather"

let forecastDataUri = "https://api.openweathermap.org/data/2.5/forecast"

let printCurrent (reading: ExtendedCurrentResponse) =
    let properties = typeof<ExtendedCurrentResponse>.GetProperties()
    for prop in properties do
        if prop.PropertyType = typeof<Weather[]> then
            let weatherArray = prop.GetValue(reading, null) :?> Weather[]
            for weather in weatherArray do
                printfn "weather:"
                let weatherProperties = typeof<Weather>.GetProperties()
                for weatherProp in weatherProperties do
                    let value = weatherProp.GetValue(weather, null)
                    printfn $"  %s{weatherProp.Name}: {value}"
        else
            let value = prop.GetValue(reading, null)
            printfn $"%s{prop.Name}: {value}"
            
let printForecast (reading: ExtendedForecastResponse) =
    let properties = typeof<ExtendedForecastResponse>.GetProperties()
    for prop in properties do
        if prop.PropertyType = typeof<WeatherForecast[]> then
            let weatherArray = prop.GetValue(reading, null) :?> WeatherForecast[]
            for weather in weatherArray do
                printfn "weather:"
                let weatherProperties = typeof<WeatherForecast>.GetProperties()
                for weatherProp in weatherProperties do
                    let value = weatherProp.GetValue(weather, null)
                    printfn $"  %s{weatherProp.Name}: {value}"
        else
            let value = prop.GetValue(reading, null)
            printfn $"%s{prop.Name}: {value}"

let GetWeatherConditions() : Task<ExtendedCurrentResponse option> =
    async {
        use client = new HttpClient()
        client.Timeout <- TimeSpan(0, 5, 0)
        try
            let! response = client.GetAsync $"%s{climateDataUri}?lat=%s{Secrets.LATITUDE}&lon=%s{Secrets.LONGITUDE}&appid=%s{Secrets.WEATHER_API_KEY}&units=imperial" |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let values = Json.deserialize<Current>(json)

            let reading_local_dt = localDateTime values.dt
            let local_sunrise_dt = localDateTime values.sys.sunrise
            let local_sunset_dt = localDateTime values.sys.sunset

            let extendedValues =
                { coord = values.coord
                  weather = values.weather
                  main = values.main
                  visibility = values.visibility
                  wind = values.wind
                  clouds = values.clouds
                  dt = values.dt
                  local_dt = reading_local_dt
                  sys = { ``type`` = values.sys.``type``
                          id = values.sys.id
                          country = values.sys.country
                          sunrise = values.sys.sunrise
                          local_sunrise_dt = local_sunrise_dt
                          sunset = values.sys.sunset
                          local_sunset_dt = local_sunset_dt }
                  timezone = values.timezone
                  id = values.id
                  name = values.name
                  cod = values.cod
                }
            
            Resolver.Log.Info("Current Weather Task ...")
            printCurrent extendedValues
            return Some extendedValues
        with
        | :? TaskCanceledException ->
            Resolver.Log.Info("Conditions request timed out.")
            return None
        | e ->
            Resolver.Log.Info $"Conditions request went sideways: %s{e.Message}"
            return None
    } |> Async.StartAsTask
    
let GetWeatherForecast() : Task<ExtendedForecastResponse option> =
    async {
        use client = new HttpClient()
        client.Timeout <- TimeSpan(0, 5, 0)
        try
            let! response = client.GetAsync $"%s{forecastDataUri}?lat=%s{Secrets.LATITUDE}&lon=%s{Secrets.LONGITUDE}&appid=%s{Secrets.WEATHER_API_KEY}&units=imperial&cnt=1" |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let forecastResponse = Json.deserialize<ForecastResponse>(json)

            if forecastResponse.list.Length > 0 then
                let values = forecastResponse.list.[0]

                let forecast_local_dt = localDateTime values.dt

                let extendedForecastValues =
                    { dt = values.dt
                      forecast_local_dt = forecast_local_dt
                      main = values.main
                      weather = values.weather
                      clouds = values.clouds
                      wind = values.wind
                      visibility = values.visibility
                      pop = values.pop
                      sys = { pod = values.sys.pod }
                      dt_txt = values.dt_txt
                    }
                Resolver.Log.Info("Weather Forecast Task ...")
                printForecast extendedForecastValues
                return Some extendedForecastValues
            else
                Resolver.Log.Info("No forecast data available.")
                return None
        with
        | :? TaskCanceledException ->
            Resolver.Log.Info("Forecast request timed out.")
            return None
        | e ->
            Resolver.Log.Info $"Forecast request went sideways: %s{e.Message}"
            return None
    } |> Async.StartAsTask