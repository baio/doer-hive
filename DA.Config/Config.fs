module DA.Config

open Microsoft.Extensions.Configuration

// open Newtonsoft.Json

// Default fileName = "settings.json", directory = current directory
let getConfig2 fileName directory =

    let fname = match fileName with Some x -> x | None -> "settings.json"
    let dir = match directory with Some x -> x | None -> System.IO.Directory.GetCurrentDirectory()

    let builder = ConfigurationBuilder()

    builder.SetBasePath(dir)
        .AddJsonFile(fname, true)
        .AddEnvironmentVariables() |> ignore

    builder.Build()

let getConfig () = getConfig2 None None

