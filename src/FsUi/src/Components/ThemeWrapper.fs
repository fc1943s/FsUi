namespace FsUi.Components

open FsCore
open Fable.Core
open FsStore
open FsUi.State
open FsStore.Hooks
open Feliz
open FsJs
open FsUi.Bindings


module ThemeWrapper =
    [<ReactComponent>]
    let ThemeWrapper themeAtom children =
        let theme = Store.useValue (themeAtom |> Option.defaultValue Atom.empty)

        let newTheme =
            React.useMemo (
                (fun () ->
                    let jsObj = JsInterop.toPlainJsObj ({|  |} ++ theme)
                    let newTheme = Ui.react.extendTheme jsObj

                    let inline getLocals () =
                        $"theme={JS.JSON.stringify theme} jsObj={JS.JSON.stringify jsObj} newTheme={JS.JSON.stringify newTheme} {getLocals ()}"

                    Logger.logTrace (fun () -> $"{nameof FsUi} | ThemeWrapper [ render ] ") getLocals
                    newTheme),
                [|
                    box theme
                |]
            )

        let darkMode = Store.useValue Atoms.Ui.darkMode

        Ui.provider
            (fun x -> x.theme <- newTheme)
            [
                (if darkMode then Ui.darkMode else Ui.lightMode)
                    (fun _ -> ())
                    [
                        yield! children
                    ]
            ]
