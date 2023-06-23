module App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

// ---------------------------------
// Routing
// ---------------------------------

open Pages.Index
open Pages.Docs
open Pages.SearchBoxExample
open Pages.TabExample
open Pages.ContactFormExample

// you can keep all your endpoints in one place :) 
let webApp =
    choose [
        GET  >=> route  "/"                        >=> ``GET /``

        GET  >=> routef  "/docs/%s"               (fun document -> ``GET /docs/:document`` document)

        GET  >=> route  "/searchbox-example"       >=> ``GET /searchbox-example``
        POST >=> route  "/search"                  >=> ``POST /search``

        GET  >=> routef "/tab/%s"                 (fun tabId -> ``GET /tab/:id`` tabId)
        GET  >=> routef "/tab/%s/"                (fun tabId -> ``GET /tab/:id`` tabId)

        GET  >=> route  "/contact-form"            >=> ``GET /contact-form``
        GET  >=> route  "/contact-form/"           >=> ``GET /contact-form``
        POST >=> route  "/contact-form"            >=> ``POST /contact-form``
        POST >=> route  "/contact-form/email"      >=> ``POST /contact-form/email``
        POST >=> route  "/contact-form/firstname"  >=> ``POST /contact-form/fistname``
        POST >=> route  "/contact-form/lastname"   >=> ``POST /contact-form/lastname``

        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001")
       .AllowAnyMethod()
       .AllowAnyHeader()
       |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0