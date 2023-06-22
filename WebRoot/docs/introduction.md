# Working with F#, Giraffe and HTMX

### Basic F#

F# is a functional style programming language in the .NET ecosystem. 
In order to understand how we will use it in this repository you will need some basic knowledge of F# .
Here are some references: linky links

### Basic Giraffe.ViewEngine 

[Giraffe](https://github.com/giraffe-fsharp/Giraffe) is a micro web framework that sits upon ASP.NET Core and makes it more functional.
The reason I chose to use it is that it allows to write HTML in its own F# domain specific language.
This allows the creation of functions that return/manipulate HTML in a way that is checked for errors at compile-time.

Here is a simple example of Giraffe.ViewEngine being used with F# 
You can see that we have a list of strings being returned as part of a search query
and if that list is not empty then we iterate over it and produce ```a``` elements inside a ```div```

You can go to [Giraffe.ViewEngine](https://github.com/giraffe-fsharp/Giraffe.ViewEngine)'s website and have a look at their README for more examples.

```fsharp
// MODELS
type SearchRequest = string
type SearchResponseItem = string
type SearchResponse = SearchResponseItem list

// View "Components"
let searchResultsView (searchResults : SearchResponse) : XmlNode =
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
```

### Basic HTMX

[HTMX](https://htmx.org/) is the product of the [grug brain developer](https://grugbrain.dev/). 
It was created because frontend development has become too complicated for what we're trying to accomplish 80% of the time.
i.e. Most websites do **not** need to be SPAs using react or angular and hundreds of node packages. 

The main idea of HTMX is that it should be easy for html components to make http calls, get ***html*** back and use it to update their state.

It's revolutionary in this day and age, but it shouldn't

Here's an example of a button making a post call to an endpoint and replacing itself with the response.
```html
  <script src="https://unpkg.com/htmx.org@1.9.2"></script>
  <!-- have a button POST a click via AJAX -->
  <button hx-post="/clicked" hx-swap="outerHTML">
    Click Me
  </button>
```

### Putting it all together

- We can use htmx (and its sibling, [hyperscript](https://hyperscript.org/)) to avoid 99% of any javascript we otherwise would have needed to write.
- We can use Giraffe.ViewEngine to make HTML be checked during compile time and also be manipulated and returned from functions with simple logic in them.
- We can use F# and its expressive type system to represent all the possible states of a control and have everything checked at compile time.

### Example 'component'

This is the definition of ***"The Whole is Greater than the Sum of its Parts"***

```fsharp
type Value        = string
type ErrorMessage = string
type FormField    = Initial | Invalid of (Value * ErrorMessage) | Valid of Value

/// text input field component that is part of a form.
let textFieldComponent formField inputName labelText validationUrl =
    let emptySpaceIcon = "&#160;"
    let successIcon    = "&#10004;"
    let warningIcon    = "&#9888;"

    let textFieldComponent' inputCssClasses icon (value: string option) (error : string option) =
        let groupId = "group" + inputName
        let imageLoadingId = "loading" + inputName
        
        div [ _id groupId; _classes [ Bulma.field ] ] [
            label [ _class Bulma.label; _for inputName ] [ Text labelText ]
            div   [ _classes [ Bulma.control; Bulma.``has-icons-right`` ] ] [
                input [
                    _type             "text"
                    _classes          inputCssClasses
                    _name             inputName
                    match value with
                    | Some v -> _value v
                    | None   -> _value ""

                    // when we lose focus
                    // wait 200ms to make sure the parent form isn't submitting
                    // and then call the validation url (server side validation)
                    // while the call is happening,
                    // trigger the loading icon and disable the input field
                    // and replace the parent div with id 'groupId'
                    // with the HTML response from the validation url.
                    _hxPost           validationUrl
                    _hxIndicatorId    imageLoadingId
                    _hxTrigger        "blur delay:200ms"
                    _hyperScript      "on htmx:beforeRequest if (closest <form/>).submitting then halt"
                    _hxTarget         groupId
                    _hxExt            "disable-element"
                    _hxDisableElement "self"
                ]
                span [ _classes [ Bulma.icon; Bulma.``is-right``; Bulma.``is-small`` ] ] [
                    Text icon
                    img [ _id imageLoadingId; _src "/img/loading.svg"; _class "htmx-indicator" ]
                ]
                match error with
                | Some err -> p [ _classes [Bulma.help; Bulma.``is-danger``] ] [ Text err ] 
                | None     -> comment ""
            ]
        ]

    // any text field has these 3 different states, give them proper css classes, icons, value text, error text
    match formField with
    | Initial                -> textFieldComponent' [Bulma.input]                       emptySpaceIcon  None         None
    | Invalid (value, error) -> textFieldComponent' [Bulma.input; Bulma.``is-danger``]  warningIcon     (Some value) (Some error)
    | Valid    value         -> textFieldComponent' [Bulma.input; Bulma.``is-success``] successIcon     (Some value) None
```

