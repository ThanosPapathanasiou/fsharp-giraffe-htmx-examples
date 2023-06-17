module Pages.ContactFormExample

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
type Email        = FormField
type FirstName    = FormField

type ContactInformation = {
    Email     : Email
    FirstName : FirstName
}

// ---------------------------------
// Validations
// ---------------------------------

let validateEmail (rawEmail: string) : Email =
    match rawEmail with
    | s when System.String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s when s.Contains("@") && s.Contains(".") -> Valid s
    | s  -> Invalid (s, "Invalid Email Address") 

let validateFirstName (rawFirstName: string) : FirstName =
    match rawFirstName with
    | s when System.String.IsNullOrWhiteSpace s -> Invalid (s, "Field is mandatory.")
    | s -> Valid s

// ---------------------------------
// "Components" and Views
// ---------------------------------

let textFieldComponent formField inputName labelText validationUrl =
    let spaceIcon   = "&#160;"
    let successIcon = "&#10004;"
    let warningIcon = "&#9888;"

    let textFieldComponent' classes icon value error =
        let groupId = "group" + inputName
        let imageLoadingId = "loading" + inputName
        
        div [ _id groupId; _classes [ Bulma.field ] ] [
            label [ _class Bulma.label; _for inputName ] [ Text labelText ]
            div   [ _classes [ Bulma.control; Bulma.``has-icons-right`` ] ] [
                input [
                    _type             "text"
                    _classes          classes
                    _name             inputName
                    _value            value

                    _hxPost           validationUrl
                    _hxIndicatorId    imageLoadingId
                    _hxTrigger        "blur delay:200ms"
                    _hyperScript      "on htmx:beforeRequest if myForm.submitting then preventDefault"
                    _hxTarget         groupId
                    _hxExt            "disable-element"
                    _hxDisableElement "self"
                ]
                span [ _classes [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ] ] [
                    Text icon
                    img [ _id imageLoadingId; _src "/img/loading.svg"; _class "htmx-indicator" ]
                ]
                match error with
                | "" -> p [ _classes [Bulma.help; Bulma.``is-danger``] ] [ Text error ] 
                | _  -> comment ""
            ]
        ]
    
    match formField with
    | Initial                   ->
        textFieldComponent' [Bulma.input] spaceIcon "" ""
    | Invalid (value, error)    ->
        textFieldComponent' [Bulma.input; Bulma.``is-danger``] warningIcon value error
    | Valid value               ->
        textFieldComponent' [Bulma.input; Bulma.``is-success``] successIcon value ""

let contactFormComponent (contactInformation : ContactInformation) =

    form [
        _id          "contactForm"
        _name        (nameof ContactInformation)
        _hxPost      "/contact-form"
        _hxSwap      "outerHTML"
        _hxIndicator ".htmx-indicator"
        _hyperScript "on submit set contactForm.submitting to true, wait for htmx:afterOnLoad from me set contactForm.submitting to false"
    ] [
        let email = (nameof Unchecked.defaultof<ContactInformation>.Email)
        textFieldComponent
            contactInformation.Email
            email
            "Email"
            ("/contact-form/" + email.ToLowerInvariant())

        let firstName = (nameof Unchecked.defaultof<ContactInformation>.FirstName)
        textFieldComponent
            contactInformation.FirstName
            firstName
            "First Name"
            ("/contact-form/" + firstName.ToLowerInvariant())

        progress  [
            _classes [ "htmx-indicator"; Bulma.progress
                       BulmaStyle; Bulma.``is-small``; Bulma.``is-marginless`` ]
        ] [ ]

        br []

        div [ _classes [ Bulma.field; Bulma.``is-grouped`` ] ] [
            button [
                _classes [ Bulma.button; Bulma.``is-link`` ]
                _type "submit"
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
                    contactFormComponent {Email = Initial; FirstName = Initial }
                ]
            ]
        ]
    ]
    createPage subtitle contents

// ---------------------------------
// Endpoint handlers 
// ---------------------------------

let ``GET /contact-form`` (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let view = contactFormView
    htmlView view next ctx

let ``POST /contact-form`` (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    task {

#if DEBUG // add some waiting time to have a chance to see the loading animation
        for i in [1..20_000_000] do i + 10 |> ignore
#endif

        let rawEmail = (string)ctx.Request.Form[nameof(Unchecked.defaultof<ContactInformation>.Email)]
        let emailValue = validateEmail rawEmail

        let rawFirstName = (string)ctx.Request.Form[nameof(Unchecked.defaultof<ContactInformation>.FirstName)]
        let firstNameValue = validateFirstName rawFirstName

        let contactInformation = {
            Email     = emailValue
            FirstName = firstNameValue
        }

        let view = htmlView (contactFormComponent contactInformation)
        return! view next ctx
    }

let ``POST /contact-form/email`` (next: HttpFunc) (ctx: HttpContext) : HttpFuncResult =
    task {

#if DEBUG // add some waiting time to have a chance to see the loading animation
        for i in [1..20_000_000] do i + 10 |> ignore
#endif

        let email         = nameof(Unchecked.defaultof<ContactInformation>.Email)
        let rawValue      = (string)ctx.Request.Form[email]
        let value         = validateEmail rawValue
        let validationUrl = "/contact-form/" + email.ToLowerInvariant()
        let html          = textFieldComponent value email "Email" validationUrl
        let view          = htmlView html

        return! view next ctx
    }

let ``POST /contact-form/fistname`` (next: HttpFunc) (ctx: HttpContext) : HttpFuncResult =
    task {

#if DEBUG // add some waiting time to have a chance to see the loading animation
        for i in [1..20_000_000] do i + 10 |> ignore
#endif

        let firstName     = nameof(Unchecked.defaultof<ContactInformation>.FirstName)
        let rawValue      = (string)ctx.Request.Form[firstName]
        let value         = validateFirstName rawValue
        let validationUrl = "/contact-form/" + firstName.ToLowerInvariant()
        let html          = textFieldComponent value firstName "First Name" validationUrl 
        let view          = htmlView html

        return! view next ctx
    }
