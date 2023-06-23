module Pages.ContactFormExample

open System
open System.Collections.Generic
open Giraffe
open Giraffe.ViewEngine

open Microsoft.AspNetCore.Http
open Pages.BaseView
open Pages.Htmx

// ---------------------------------
// Models
// ---------------------------------

type Value        = string
type ErrorMessage = string
type FormField    = Initial | Invalid of (Value * ErrorMessage) | Valid of Value

type Email     = FormField
type FirstName = FormField
type LastName  = FormField
type ContactInformation = {
    Email     : Email
    FirstName : FirstName
    LastName  : LastName
}

// TODO: get the name and label from data annotations attributes?
let data : IDictionary<string, (string * string * string ) > = dict [
    nameof(Unchecked.defaultof<ContactInformation>.Email),     (nameof(Unchecked.defaultof<ContactInformation>.Email), "Email", "/contact-form/email")
    nameof(Unchecked.defaultof<ContactInformation>.FirstName), (nameof(Unchecked.defaultof<ContactInformation>.FirstName), "First Name", "/contact-form/firstname")
    nameof(Unchecked.defaultof<ContactInformation>.LastName),  (nameof(Unchecked.defaultof<ContactInformation>.LastName), "Last Name", "/contact-form/lastname")
]

// ---------------------------------
// Validations
// ---------------------------------

let validateEmail (rawEmail: string) : Email =
    match rawEmail with
    | s when String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s when s.Contains("@") && s.Contains(".") -> Valid s
    | s  -> Invalid (s, "Invalid Email Address") 

let validateFirstName (rawFirstName: string) : FirstName =
    match rawFirstName with
    | s when String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s -> Valid s

let validateLastName (rawFirstName: string) : LastName =
    match rawFirstName with
    | s when String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s -> Valid s

// ---------------------------------
// "Components" and Views
// ---------------------------------

let textFieldComponent formFieldValue formFieldName formFieldLabel validationUrl =
    let emptySpaceIcon = "&#160;"
    let successIcon    = "&#10004;"
    let warningIcon    = "&#9888;"

    let cssClasses, value, message, icon =
        match formFieldValue with
        | Initial                -> [""]                  , ""   , emptySpaceIcon, emptySpaceIcon
        | Invalid (value, error) -> [Bulma.``is-danger`` ], value, error         , warningIcon
        | Valid    value         -> [Bulma.``is-success``], value, emptySpaceIcon, successIcon

    div [ _classes [ Bulma.field ] ] [
        label [ _class Bulma.label; _for formFieldName ] [ Text formFieldLabel ]
        div   [ _classes [ Bulma.control; Bulma.``has-icons-right`` ] ] [
            input [
                _type             "text"
                _classes          ([ Bulma.input ] @ cssClasses)
                _name             formFieldName
                _value            value

                _hxPost           validationUrl
                _hxTrigger        "blur delay:200ms"
                _hyperScripts     ["on htmx:beforeRequest if (closest <form/>).submitting then halt end";
                                   "then on htmx:beforeRequest add .is-loading to (closest <div/>)"
                                   "then on htmx:beforeRequest add @disabled to me";
                                   "then on htmx:beforeRequest remove (next <span/>) end"]
                _hxTarget          ("closest ." + Bulma.field)
            ]
            span [ _classes [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ] ] [
                Text icon
            ]
            p [ _classes ([Bulma.help] @ cssClasses) ] [ Text message ]
        ]
    ]

let contactFormComponent (contactInformation : ContactInformation) =

    form [
        _name        (nameof ContactInformation)
        _hxPost      "/contact-form"
        _hxSwap      "outerHTML"
        _hyperScript "on submit set me.submitting to true wait for htmx:afterOnLoad from me set me.submitting to false"
    ] [

        let formFieldValue   = contactInformation.Email
        let name, label, url = data[nameof(contactInformation.Email)]
        textFieldComponent formFieldValue name label url
        
        let formFieldValue   = contactInformation.FirstName
        let name, label, url = data[nameof(contactInformation.FirstName)]
        textFieldComponent formFieldValue name label url

        let formFieldValue   = contactInformation.LastName
        let name, label, url = data[nameof(contactInformation.LastName)]
        textFieldComponent formFieldValue name label url

        div [ _classes [ Bulma.field; Bulma.``is-grouped`` ] ] [
            button [
                _classes [ Bulma.button; Bulma.``is-link`` ]
                _type "submit"
                _hyperScript "on click add .is-loading to me"
            ] [ Text "Submit" ]
        ]
    ]

let contactFormView =
    let subtitle      = "A simple Form 'component' example"
    let contents      = [
        section [ _classes [ Bulma.section ] ] [
            div [ _classes [ Bulma.container ] ] [
                div [ _classes [ Bulma.``is-full-desktop`` ] ] [
                    h3 [ _classes [Bulma.subtitle ; Bulma.``is-3``] ] [ Text "Signup Form" ]
                    contactFormComponent { Email = Initial; FirstName = Initial; LastName = Initial }
                ]
            ]
        ]
    ]
    createPage subtitle contents

// ---------------------------------
// Endpoint handlers 
// ---------------------------------

let ``GET /contact-form`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let view = contactFormView
        htmlView view next ctx

let ``POST /contact-form`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let emailValue =
                nameof(Unchecked.defaultof<ContactInformation>.Email)
                |> fun s -> ctx.Request.Form[s]
                |> string
                |> validateEmail

            let firstNameValue =
                nameof(Unchecked.defaultof<ContactInformation>.FirstName)
                |> fun s -> ctx.Request.Form[s]
                |> string
                |> validateFirstName

            let lastNameValue =
                nameof(Unchecked.defaultof<ContactInformation>.LastName)
                |> fun s -> ctx.Request.Form[s]
                |> string
                |> validateFirstName

            let contactInformation = {
                Email     = emailValue
                FirstName = firstNameValue
                LastName  = lastNameValue 
            }

            let html = contactFormComponent contactInformation
            let view = htmlView html
            return! view next ctx
        }

let ``POST /contact-form/email`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let name, label, url = data[nameof(Unchecked.defaultof<ContactInformation>.Email)]
            let formFieldValue   = name |> fun s -> ctx.Request.Form[s] |> string |> validateEmail
            let html             = textFieldComponent formFieldValue name label url
            let view             = htmlView html

            return! view next ctx
        }

let ``POST /contact-form/fistname`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let name, label, url = data[nameof(Unchecked.defaultof<ContactInformation>.FirstName)]
            let formFieldValue   = name |> fun s -> ctx.Request.Form[s] |> string |> validateFirstName
            let html             = textFieldComponent formFieldValue name label url
            let view             = htmlView html

            return! view next ctx
        }

let ``POST /contact-form/lastname`` : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let name, label, url = data[nameof(Unchecked.defaultof<ContactInformation>.LastName)]
            let formFieldValue   = name |> fun s -> ctx.Request.Form[s] |> string |> validateLastName
            let html             = textFieldComponent formFieldValue name label url
            let view             = htmlView html

            return! view next ctx
        }