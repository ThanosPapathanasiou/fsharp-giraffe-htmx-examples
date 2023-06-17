module Pages.Index

open Giraffe
open Giraffe.ViewEngine

open Microsoft.AspNetCore.Http
open Pages.BaseView

// ---------------------------------
// Views
// ---------------------------------

let indexView =

    let subtitle      = ""
    let contents      = [
        section [ _classes [ Bulma.section ] ] [
            div [ _classes [ Bulma.container ] ] [
                div [ _classes [ Bulma.box ] ] [
                    div [ _classes [ Bulma.content ] ] [
                        a [ _href "/contact-form" ] [
                            Text "Contact Form Example"
                        ]
                        p [] [
                            Text "A simple contact form that uses htmx to validate the input when the input loses focus and when the form is submitted."
                        ]
                        p [] [
                            Text "It also disables the fields while they are being validated and uses hyperscript to fix the issue of the blur event firing on the fields when the form is submitting."
                        ]
                    ]
                ]
                div [ _classes [ Bulma.box ] ] [
                    div [ _classes [ Bulma.content ] ] [
                        a [ _href "/searchbox-example" ] [
                            Text "Searchbox Example"
                        ]
                        p [] [
                            Text "A simple searchbox that uses htmx to automatically send a post request once the user stops typing."
                        ]
                    ]
                ]
                div [ _classes [ Bulma.box ] ] [
                    div [ _classes [ Bulma.content ] ] [
                        a [ _href "/tab/first-tab" ] [
                            Text "Page with tabs Example"
                        ]
                        p [] [
                            Text "A simple page with multiple tabs that uses htmx to partially load the content based on the selected tab."
                        ]
                    ]
                ]

            ]
        ]
    ]
    createPage subtitle contents


// ---------------------------------
// "Controllers"
// ---------------------------------
let ``GET /`` (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    htmlView indexView next ctx
