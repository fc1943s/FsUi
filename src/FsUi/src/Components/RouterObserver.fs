namespace FsUi.Components

open Fable.Core
open FsCore
open FsJs
open Feliz.Router
open Feliz
open FsStore
open FsStore.Model
open FsStore.Hooks
open FsStore.State
open FsUi.Bindings
open Fable.Core.JsInterop
open Microsoft.FSharp.Core.Operators


module RouterObserverWrapper =
    [<ReactComponent>]
    let rec RouterObserverWrapper children =
        Profiling.addTimestamp (fun () -> $"{nameof FsUi} | RouterObserverWrapper [ render ] ") getLocals

        let logger = Store.useValue Selectors.logger
        let alias = Store.useValue Selectors.Gun.alias

        let deviceInfo = Store.useValue Selectors.deviceInfo
        let lastSegments = React.useRef []
        let appState = Store.useValue (Atoms.Device.appState deviceInfo.DeviceId)
        let consumeCommands = Store.useCallbackRef (Engine.consumeCommands Messaging.appUpdate appState)

        React.useEffect (
            (fun () ->
                Profiling.addTimestamp
                    (fun () -> $"{nameof FsUi} | RouterObserverWrapper [ render / useEffect ] ")
                    getLocals

                match Dom.window () with
                | Some window ->
                    let redirect = window.sessionStorage?redirect
                    emitJsExpr () "delete sessionStorage.redirect"

                    match redirect with
                    | String.Valid _ when redirect <> window.location.href ->
                        Router.navigatePath (redirect |> String.split "/" |> Array.skip 3)
                    | _ -> ()
                | None -> ()),
            [||]
        )

        let onChange =
            Store.useCallbackRef
                (fun _ setter (newSegments: string list) ->
                    promise {
                        Profiling.addTimestamp
                            (fun () -> $"{nameof FsUi} | RouterObserverWrapper / render / onChange")
                            getLocals

                        if newSegments <> lastSegments.current then
                            let getLocals () =
                                $"newSegments={JS.JSON.stringify newSegments} lastSegments.current={JS.JSON.stringify lastSegments.current} {getLocals ()}"

                            logger.Debug (fun () -> $"{nameof FsUi} | RouterObserverWrapper. onChange 1") getLocals

                            lastSegments.current <- newSegments
                            Atom.change setter Atoms.routeTrigger ((+) 1)

                            let messages =
                                match newSegments with
                                | [ base64 ] ->
                                    try

                                        let json =
                                            match Dom.window () with
                                            | Some window -> window?atob base64
                                            | None -> ""

                                        json
                                        |> Json.decode<Message<AppCommand, AppEvent> list>
                                        |> Some
                                    with
                                    | ex ->
                                        let getLocals () =
                                            $"ex={ex} newSegments={JS.JSON.stringify newSegments} {getLocals ()}"

                                        logger.Error
                                            (fun () ->
                                                $"{nameof FsUi} | RouterObserverWrapper. onChange 3. error deserializing")
                                            getLocals

                                        Some []
                                | _ -> None

                            match messages with
                            | Some [] ->
                                let getLocals () =
                                    $"newSegments={newSegments} {getLocals ()}"

                                logger.Error (fun () -> $"{nameof FsUi} | Invalid messages") getLocals
                            | Some messages ->
                                let getLocals () =
                                    $"messages={messages} newSegments={JS.JSON.stringify newSegments} {getLocals ()}"

                                logger.Debug (fun () -> $"{nameof FsUi} | RouterObserverWrapper. onChange 2. saving messages") getLocals

                                match alias with
                                | Some _ ->
                                    messages
                                    |> List.iter
                                        (fun message ->
                                            let messageId = Hydrate.hydrateAppMessage setter message
                                            let getLocals () = $"messageId={messageId} {getLocals ()}"
                                            logger.Debug (fun () -> $"{nameof FsUi} | RouterObserverWrapper. message hydrated") getLocals)
                                | None ->
                                    let commands =
                                        messages
                                        |> List.choose
                                            (function
                                            | Message.Command command -> Some command
                                            | _ -> None)

                                    let! events = consumeCommands commands
                                    let getLocals () = $"events={events} {getLocals ()}"

                                    logger.Debug
                                        (fun () -> $"{nameof FsUi} | RouterObserverWrapper. no alias. consumed inline")
                                        getLocals

                                Router.navigatePath [||]
                            | None -> Router.navigatePath [||]
                    })

        Store.useHashedEffectOnce
            (nameof RouterObserverWrapper, deviceInfo.DeviceId)
            (fun _getter _setter ->
                promise {
                    let getLocals () =
                        $"location.href={Browser.Dom.window.location.href} {getLocals ()}"

                    Profiling.addTimestamp
                        (fun () -> $"{nameof FsUi} | RouterObserverWrapper / render / useHashedEffectOnce")
                        getLocals

                    match Browser.Dom.window.location.hash with
                    | null
                    | "" -> ()
                    | hash when hash.StartsWith "#" ->
                        let newSegment = hash |> String.substringFrom 1
                        do! onChange [ newSegment ]
                    | _ -> ()
                })


        //        let setSessionRestored = Store.useSetState Atoms.Session.sessionRestored
//
//        React.useEffect (
//            (fun () -> setSessionRestored true),
//            [|
//                box setSessionRestored
//            |]
//        )

        React.router [
            router.hashMode
            router.onUrlChanged (onChange >> Promise.start)
            router.children [ yield! children ]
        ]
