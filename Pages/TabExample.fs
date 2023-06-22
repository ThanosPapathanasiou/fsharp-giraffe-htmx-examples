module Pages.TabExample

open Giraffe
open Giraffe.ViewEngine

open Microsoft.AspNetCore.Http

open Pages.Htmx
open Pages.BaseView

// ---------------------------------
// Model
// ---------------------------------

type Tab =
    | ``First Tab``
    | ``Second Tab``
    | ``Etc Tab``

let tabToUrlString (tab:Tab) : string =
    (string tab).Replace(' ', '-').ToLowerInvariant()

let tabFromUrlString (s:string) : Option<Tab> =
    let s' = s.ToLowerInvariant()
    match s' with
    | "first-tab"  -> Some Tab.``First Tab``
    | "second-tab" -> Some Tab.``Second Tab``
    | "etc-tab"    -> Some Tab.``Etc Tab``
    | _            -> None

// ---------------------------------
// "Components"
// ---------------------------------

let tabComponent (baseUrl:string) (activeTab:Tab) (tabContent:XmlNode) =
    let tabComponentId = "tabbed-content"

    /// add li for given tab
    let li' (thisTab:Tab) =
        let _classes' =
            if activeTab = thisTab then _classes [ Bulma.``is-active`` ]
            else                        _classes [ ]

        li [ _classes' ] [
            a [
                // do a htmx get and replace the target with the response
                _hxGet $"{baseUrl}/{tabToUrlString thisTab}"
                _hxPushUrl "true"
                _hxTarget tabComponentId
                _hxSwap "outerHTML"
            ] [
                span [] [ Text ( string thisTab ) ]
            ]
        ]

    div [
        _id tabComponentId
    ] [
        div [ _classes [
              Bulma.container; Bulma.``is-primary``; Bulma.``is-centered``
              Bulma.tabs; Bulma.``is-toggle``; Bulma.``is-fullwidth``
        ] ] [
            ul [] [
                li' Tab.``First Tab``
                li' Tab.``Second Tab``
                li' Tab.``Etc Tab``
            ]
        ]
        tabContent
    ]

// ---------------------------------
// Views
// ---------------------------------

let tabView (subtitle:string) (baseUrl:string) (activeTab:Tab) (tabContent:XmlNode) =
    let contents      = [
        section [ _classes [ Bulma.section ] ] [
            tabComponent baseUrl activeTab tabContent
        ]
    ]

    createPage subtitle contents

let firstTabView =
    div [ _classes [ Bulma.box; ] ] [
        p [] [ Text "First tab content"]
    ]

let secondTabView = 
    div [ _classes [ Bulma.box; ] ] [
        p [] [ Text "Second tab content"]
    ]

let etcTabView =
    div [ _classes [ Bulma.box; ] ] [
        p [] [ Text "Etc tab content"]
    ]

// ---------------------------------
// Controller
// ---------------------------------

let ``GET /tab/:id`` (activeTab:string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let selectedTab =
                match tabFromUrlString activeTab with
                | Some t -> t
                | None -> ``First Tab`` //the default tab

            // TODO: get the data from an api and return it here. Then have views display it.
            let getViewForTab (tab:Tab) =
                match tab with
                | ``First Tab``  -> firstTabView
                | ``Second Tab`` -> secondTabView
                | ``Etc Tab``    -> etcTabView

            let baseUrl = "/tab"
            let subtitle = "Select from multiple tabs"
            let tabContent = getViewForTab selectedTab

            let htmlResponse (tab: Tab) = 
                if isHtmxRequest ctx then
                    tabComponent baseUrl selectedTab tabContent
                else 
                    tabView subtitle baseUrl selectedTab tabContent

            let view = selectedTab |> htmlResponse |> htmlView
            return! view next ctx
        }

