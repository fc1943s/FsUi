namespace FsUi.Components

open System
open Fable.Core.JsInterop
open Fable.React
open Feliz
open FsJs
open FsStore
open FsStore.Bindings.Jotai
open FsCore
open FsStore.State
open FsUi.State
open FsStore.Hooks
open FsUi.Bindings
open FsUi.Hooks


module DebugPanel =
    open System.Text.RegularExpressions


    [<RequireQualifiedAccess>]
    type DebugPanelDisplay =
        | None
        | Overlay
        | Inline

    let inline mapDict dict =
        dict
        |> Seq.indexed
        |> Seq.map (fun (i, KeyValue (k, v)) -> $"{i}. {k}", box $"""{v} call{if v = 1 then "" else "s"}""")

    [<ReactComponent>]
    let ValueIndicator name atom =
        let value = Store.useValue atom

        Ui.box
            (fun _ -> ())
            [
                str $"[{name}=%A{Json.encodeWithNullFormatted value}]"
            ]

    [<ReactComponent>]
    let GunSyncIndicator () =
        ValueIndicator (nameof Atoms.gunSync) Atoms.gunSync

    [<ReactComponent>]
    let GunPeersIndicator () =
        ValueIndicator (nameof Selectors.Gun.gunPeers) Selectors.Gun.gunPeers

    [<ReactComponent>]
    let HubSyncIndicator () =
        ValueIndicator (nameof Atoms.hubSync) Atoms.hubSync

    [<ReactComponent>]
    let HubUrlsIndicator () =
        ValueIndicator (nameof Atoms.hubUrls) Atoms.hubUrls

    [<ReactComponent>]
    let UiStateIndicator () =
        ValueIndicator (nameof Selectors.Ui.uiState) Selectors.Ui.uiState

    [<ReactComponent>]
    let SessionRestoredIndicator () =
        ValueIndicator (nameof Atoms.sessionRestored) Atoms.sessionRestored

    [<ReactComponent>]
    let ShowDebugIndicator () =
        ValueIndicator (nameof Atoms.showDebug) Atoms.showDebug

    [<ReactComponent>]
    let AliasIndicator () =
        ValueIndicator (nameof Selectors.Gun.alias) Selectors.Gun.alias

    [<ReactComponent>]
    let PrivateKeysIndicator () =
        ValueIndicator (nameof Selectors.Gun.privateKeys) Selectors.Gun.privateKeys

    let inline naturalSortFn (text: string) =
        Regex("([0-9]+)").Split text
        |> Array.map
            (fun text ->
                match Int32.TryParse text with
                | true, i -> Choice1Of2 i
                | false, _ -> Choice2Of2 text)

    let inline getSchedulingInterval (deviceInfo: Dom.DeviceInfo) =
        if not deviceInfo.IsTesting then 1000
        elif Dom.globalExit.Get () then 2000 // * 30
        else 0

    let debugText = Atom.Primitives.create (AtomType.Atom "")
    let debugOldJson = Atom.Primitives.create (AtomType.Atom "")

    [<ReactComponent>]
    let DebugPanel display =
        let deviceInfo = Store.useValue Selectors.Store.deviceInfo
        let showDebug = Store.useValue Atoms.showDebug

        let interval, setInterval = React.useState (getSchedulingInterval deviceInfo)

        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun _ _ ->
                promise {
                    if not showDebug then
                        ()
                    else
                        let newInterval = getSchedulingInterval deviceInfo
                        if newInterval <> interval then setInterval newInterval
                })

        let inline getLocals () =
            $"interval={interval} showDebug={showDebug} {getLocals ()}"

        Logger.logTrace (fun () -> $"{nameof FsUi} | DebugPanel / render") getLocals

        let text, setText = Store.useState debugText
        let oldJson, setOldJson = Store.useState debugOldJson

        Scheduling.useScheduling
            Scheduling.Interval
            interval
            (fun _ _ ->
                promise {
                    if not showDebug then
                        ()
                    else
                        let json =
                            [
                                Json.encodeWithNullFormatted
                                    {|
                                        CountMap =
                                            Profiling.profilingState.CountMap
                                            |> mapDict
                                            |> createObj
                                    |}
                                Json.encodeWithNullFormatted
                                    {|
                                        SortedCountMap =
                                            Profiling.globalProfilingState.Get().CountMap
                                            |> mapDict
                                            |> Seq.sortByDescending (snd >> string >> naturalSortFn)
                                            |> createObj
                                    |}
                                Json.encodeWithNullFormatted
                                    {|
                                        TimestampMap =
                                            Profiling.profilingState.TimestampMap
                                            |> Seq.map (fun (k, v) -> $"{k} = {v}ms")
                                            |> Seq.toArray
                                    |}
                            ]
                            |> String.concat Environment.NewLine

                        if json = oldJson then
                            ()
                        else
                            setText json
                            setOldJson json
                })

        React.fragment [
            //            if debug then
//                Chakra.box
//                    (fun x ->
//                        x.id <- "test1"
//                        x.position <- "absolute"
//                        x.width <- "100px"
//                        x.height <- "80px"
//                        x.top <- "40px"
//                        x.right <- "24px"
//                        x.backgroundColor <- "#ccc3"
//                        x.zIndex <- 1)
//                    [
//                        str "test1"
//                    ]

            Ui.flex
                (fun x ->
                    match display with
                    | DebugPanelDisplay.Overlay ->
                        x.width <- "min-content"
                        x.height <- if showDebug then "60%" else "initial"
                        x.position <- "fixed"
                        x.right <- "24px"
                        x.bottom <- "0"
                        x.zIndex <- 4
                        x.overflow <- if showDebug then "scroll" else "initial"
                    | _ -> ()

                    x.flex <- "1"
                    x.backgroundColor <- "#44444455")
                [
                    if showDebug then
                        Ui.box
                            (fun x ->
                                x.flex <- "1"
                                x.id <- "debug"
                                x.fontSize <- "9px"
                                x.padding <- "4px"
                                x.lineHeight <- "11px"
                                x.whiteSpace <- "pre"
                                x.fontFamily <- "Roboto Condensed Light, system-ui, sans-serif")
                            [
                                AliasIndicator ()
                                PrivateKeysIndicator ()
                                SessionRestoredIndicator ()
                                ShowDebugIndicator ()
                                GunSyncIndicator ()
                                GunPeersIndicator ()
                                HubSyncIndicator ()
                                HubUrlsIndicator ()
                                UiStateIndicator ()
                                str text
                                ValueIndicator (nameof Selectors.Store.deviceInfo) Selectors.Store.deviceInfo
                            ]
                ]
        ]
