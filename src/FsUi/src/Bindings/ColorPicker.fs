namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop

module ColorPicker =
    let sketchPicker: obj -> obj = import "SketchPicker" "react-color"

    let inline render
        (props: {| color: string
                   onChange: {| hex: string |} -> unit |})
        =
        ReactBindings.React.createElement (
            sketchPicker,
            {| props with
                width = "calc(100% - 19px)"
            |},
            []
        )
