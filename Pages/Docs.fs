module Pages.Docs

open System.IO
open Giraffe
open Microsoft.AspNetCore.Http

open FSharp.Formatting.Markdown

let ``GET /docs/:document`` (document':string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->

        let pathToFile document  =
            Path.Combine [| Directory.GetCurrentDirectory (); "WebRoot"; "docs"; ( document + ".md") |]

        let document =
            match document' |> pathToFile |> File.Exists with
            | true  -> document'
            | false -> "introduction"

        let rawHtml =
          document
          |> pathToFile
          |> File.ReadAllText
          |> Markdown.Parse
          |> Markdown.ToHtml

        // TODO: figure out how to style this.
        htmlString rawHtml next ctx 
