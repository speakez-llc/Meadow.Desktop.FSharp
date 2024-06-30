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

let weatherUri = "https://api.openweathermap.org/data/2.5/"

let printReading<'T> (reading: 'T) =
    let properties = typeof<'T>.GetProperties()
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
            
let GetWeatherConditions() : Task<ExtendedCurrentResponse option> =
    async {
        use client = new HttpClient()
        client.Timeout <- TimeSpan(0, 5, 0)
        try
            let! response = client.GetAsync (weatherUri +
                                             "weather" +
                                             "?lat=" + LATITUDE +
                                             "&lon=" + LONGITUDE +
                                             "&appid=" + WEATHER_API_KEY +
                                             "&units=imperial") |> Async.AwaitTask
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
            printReading<ExtendedCurrentResponse> extendedValues
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
            let! response = client.GetAsync (weatherUri +
                                             "forecast" +
                                             "?lat=" + Secrets.LATITUDE +
                                             "&lon=" + Secrets.LONGITUDE +
                                             "&appid=" + Secrets.WEATHER_API_KEY +
                                             "&units=imperial&cnt=2") |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let forecastResponse = Json.deserialize<ForecastResponse>(json)

            if forecastResponse.list.Length > 1 then
                let mutable values = forecastResponse.list.[0]
                let forecast_local_dt = localDateTime values.dt
                let current_local_dt = DateTime.Now

                if Math.Abs((forecast_local_dt - current_local_dt).TotalHours) <= 1.0 then
                    Resolver.Log.Info("Using Second Forecast ...")
                    values <- forecastResponse.list.[1]
                    else
                    Resolver.Log.Info("Using First Forecast ...")

                let extendedForecastValues =
                    { dt = values.dt
                      forecast_local_dt = localDateTime values.dt
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
                printReading<ExtendedForecastResponse> extendedForecastValues
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
    
// This code is used to create an east-facing half-oval centered on the specified latitude and longitude
open CoordinateSharp

let getLatLon (coord: Coordinate) =
    let latitude = coord.Latitude.DecimalDegree * 1e6 |> round |> fun x -> x / 1e6
    let longitude = coord.Longitude.DecimalDegree * 1e6 |> round |> fun x -> x / 1e6
    (latitude, longitude)
    
let calculateNewPoint (point : GeoFence.Point, headingDeg: double, distanceKm: double) =
    let newCoordinate = new Coordinate(point.Latitude, point.Longitude)
    newCoordinate.Move(distanceKm * 1000.0, headingDeg, Shape.Sphere)
    let newPoint = getLatLon newCoordinate
    GeoFence.Point(newPoint |> fst, newPoint |> snd)
    
let createPieSlice (latA: float, lonA: float, radiusKm: float, bearingDeg: float, angleDeg: float) =
    let startPoint = GeoFence.Point(latA, lonA)
    let mutable points : List<GeoFence.Point> = []
    points <- startPoint :: points // Prepend startPoint to the list
    let bearingMin = (bearingDeg - (angleDeg / 2.0)) % 360.0
    // Calculate and add intermediate points
    for newBearing in [bearingMin..5.0..(bearingMin + angleDeg)] do
        let newPoint = calculateNewPoint(startPoint, newBearing, radiusKm)
        points <- newPoint :: points
    points <- startPoint :: points // Postpend startPoint to the list
    points
    |> List.map (fun point -> $"{point.Latitude},{point.Longitude},") 
    |> String.concat "\n"
    
// createPieSlice(35.581664, -82.557433, 5.0, 300, 40.0)