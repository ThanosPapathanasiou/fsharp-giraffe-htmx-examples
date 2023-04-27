module Pages.Htmx

open Microsoft.AspNetCore.Http
open Giraffe.ViewEngine

let isHtmxRequest (ctx:HttpContext) : bool =
        ctx.Request.Headers.ContainsKey "HX-Request" &&
        not (ctx.Request.Headers.ContainsKey "HX-History-Restore-Request") 

// there is a giraffe htmx library but you can just add these as needed.

let _hxGet                    = (attr "hx-get")
let _hxPost                   = (attr "hx-post")
let _hxTrigger                = (attr "hx-trigger")
let _hxTarget (id:string)     = (attr "hx-target" ("#"+id))
let _hxIndicator              = (attr "hx-indicator")
let _hxSwap                   = (attr "hx-swap")
let _hxPushUrl                = (attr "hx-push-url")
