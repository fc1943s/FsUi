namespace FsUi.Components

open FsCore
open FsStore
open Fable.React
open Feliz
open FsJs
open FsStore.Bindings
open FsUi.Bindings


module RootWrapper =
    [<ReactComponent>]
    let RootWrapper themeAtom children =
        Profiling.addTimestamp (fun () -> $"{nameof FsUi} | RootWrapper / render") getLocals

        React.strictMode [
            Jotai.jotai.provider [
                React.suspense (
                    [
                        React.ErrorBoundary [
                            GunObserver.GunObserver ()
                            ToastObserver.ToastObserver ()
                            MessagesListener.MessagesListener ()
                            RouterObserverWrapper.RouterObserverWrapper [
                                ThemeWrapper.ThemeWrapper
                                    themeAtom
                                    [
                                        yield! children
                                    ]
                            ]
                        ]
                    ],
                    Ui.box
                        (fun x -> x.className <- "static")
                        [
                            str "Loading..."
                        ]
                )
            ]
        ]
