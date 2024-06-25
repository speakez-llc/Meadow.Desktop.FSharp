module WeatherReading

open System

type Coordinates =
    {
        lon: float
        lat: float
    }

type Weather =
    {
        id: int
        main: string
        description: string
        icon: string
    }

type MainCurrent =
    {
        temp: float
        feels_like: float
        temp_min: float
        temp_max: float
        pressure: int
        humidity: int
    }

type Wind =
    {
        speed: decimal
        deg: int
        gust: float option
    }

type Clouds =
    {
        all: int
    }

type SystemCurrent =
    {
        ``type``: int
        id: int
        country: string
        sunrise: int64
        sunset: int64
    }
    
type ExtendedSystemCurrent =
    {
        ``type``: int
        id: int
        country: string
        sunrise: int64
        local_sunrise_dt: DateTime
        sunset: int64
        local_sunset_dt: DateTime
    }

type Current =
    {
        coord: Coordinates
        weather: Weather array
        main: MainCurrent
        visibility: int
        wind: Wind
        clouds: Clouds
        dt: int
        sys: SystemCurrent
        timezone: int64
        id: int
        name: string
        cod: int
    }
    
type ExtendedCurrentResponse =
    {
        coord: Coordinates
        weather: Weather array
        main: MainCurrent
        visibility: int
        wind: Wind
        clouds: Clouds
        dt: int
        local_dt: DateTime
        sys: ExtendedSystemCurrent
        timezone: int64
        id: int
        name: string
        cod: int
    }
    
type MainForecast =
    {
        temp: float
        feels_like: float
        temp_min: float
        temp_max: float
        pressure: int
        sea_level: int
        grnd_level: int
        humidity: int
        temp_kf: float
    }

type WeatherForecast =
    {
        id: int
        main: string
        description: string
        icon: string
    }

type CloudsForecast =
    {
        all: int
    }

type WindForecast =
    {
        speed: float
        deg: int
        gust: float option
    }

type SysForecast =
    {
        pod: string
    }

type ListForecast =
    {
        dt: int64
        main: MainForecast
        weather: WeatherForecast array
        clouds: CloudsForecast
        wind: WindForecast
        visibility: int
        pop: float
        sys: SysForecast
        dt_txt: string
    }
    
type ExtendedForecastResponse =
    {
        dt: int64
        forecast_local_dt: DateTime
        main: MainForecast
        weather: WeatherForecast array
        clouds: CloudsForecast
        wind: WindForecast
        visibility: int
        pop: float
        sys: SysForecast
        dt_txt: string
    }

type ForecastResponse =
    {
        cod: string
        message: int
        cnt: int
        list: ListForecast array
    }