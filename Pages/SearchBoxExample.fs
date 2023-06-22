module Pages.SearchBoxExample

open Giraffe
open Giraffe.ViewEngine

open Microsoft.AspNetCore.Http
open Pages.BaseView
open Pages.Htmx

// ---------------------------------
// Models
// ---------------------------------

type SearchRequest = string
type SearchResponseItem = string
type SearchResponse = SearchResponseItem list


// ---------------------------------
// "Components" 
// ---------------------------------

let simpleSearchBox =
    let searchResults = "search-results"

    div [ _classes [ Bulma.notification; Bulma.``is-full-desktop`` ] ] [

        input [
            _type           "text"
            _name           (nameof SearchRequest)
            _classes        [ BulmaStyle; Bulma.input; Bulma.``is-medium`` ]
            _placeholder    "Enter text to search for something."

            _onfocus        "this.select()"
            _autofocus

            // trigger the post request when user stops typing and place results in the target
            _hxTrigger      "keyup changed delay:500ms"
            _hxPost         "/search"
            _hxTarget       searchResults

            // show progress bar while waiting for response
            _hxIndicator    ".htmx-indicator"
        ]

        progress  [
            _classes [ "htmx-indicator"; Bulma.progress
                       BulmaStyle; Bulma.``is-small``; Bulma.``is-marginless`` ]
        ] [ ]

        div [ _id searchResults ] [
            // htmx results are placed here
        ]
    ]

// ---------------------------------
// POST /search  View + Handler
// ---------------------------------

/// The html that is returned from the POST /search request.
let searchResultsView (searchResults : SearchResponse) =
    match searchResults with
    | [] ->
        comment "nothing to see here, move along!"
    | _  -> 
        div [ _classes [ Bulma.box ] ] [
            for searchResult in searchResults do
            a [ _class Bulma.media
                _href "#"] [
                div [ _class Bulma.``media-content`` ] [
                    div [ _class Bulma.content ] [
                        p [] [ Text searchResult ]
                    ]
                ]
            ]
        ]

/// Handles the POST /search request and returns the 'searchResultsView' html results
let ``POST /search`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {

    #if DEBUG // add some waiting time to have a chance to see the loading animation
            for i in [1..20_000_000] do i + 10 |> ignore
    #endif

            let searchTerm = ctx.Request.Form[nameof(SearchRequest)] |> string

            // TODO: get the response from a database / service / etc
            //       you can do that by using the ctx to inject what you need.
            // 
            //       let myService = ctx.GetService<IMyService>()
            let searchResults =
                match searchTerm with
                | "" -> []
                | _  -> [
                            "First search result."
                            "Second search result."
                            "Third search result."
                            "You get the idea..."
                        ]

            let view = htmlView (searchResultsView searchResults)
            return! view next ctx
        }

// ---------------------------------
// GET /searchbox-example View and Handler
// ---------------------------------

/// The html that is returned from the GET /searchbox-example request
let searchBoxExampleView =
    let subtitle      = "A simple SearchBox 'component' example"
    let contents      = [
        section [ _classes [ Bulma.section ] ] [
            div [ _classes [ Bulma.container ] ] [
                simpleSearchBox
            ]
        ]
    ]
    createPage subtitle contents

/// Handles the GET /searchbox-example request and returns the 'searchBoxExampleView' html results
let ``GET /searchbox-example`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> 
        htmlView searchBoxExampleView next ctx

