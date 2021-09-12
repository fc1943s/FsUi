namespace FsUi.Components

open FsCore
open Feliz
open FsStore
open FsStore.Hooks
open FsJs
open FsStore.Bindings
open FsStore.State
open FsUi.Bindings
open FsUi.Hooks
open Fable.React


module GunObserver =

    [<ReactComponent>]
    let GunObserver () =
        let logger = Store.useValue Selectors.Store.logger
        let gun = Store.useValue Selectors.Gun.gun
        let store = Store.useStore ()
        let isMountedRef = React.useIsMountedRef ()

        let inline getLocals () =
            $"isMountedRef={isMountedRef.current} {getLocals ()}"

        Profiling.addTimestamp (fun () -> $"{nameof FsUi} | GunObserver / render") getLocals

        // TODO: arr deps warning is not working with files in this package (only on template main)
        React.useEffect (
            (fun () ->
                gun.on (
                    Gun.GunEvent "auth",
                    (fun () ->
                        //                        JS.setTimeout
//                            (fun () ->
                        promise {
                            if isMountedRef.current then
                                let! _getter, setter = store ()
                                //                                ()

                                Profiling.addTimestamp
                                    (fun () ->
                                        $"{nameof FsUi} | GunObserver [ render / useEffect / setTimeout ] triggering.")
                                    getLocals

                                Atom.change setter Atoms.gunTrigger ((+) 1)
                                Atom.change setter Atoms.hubTrigger ((+) 1)

                                logger.Debug (fun () -> $"{nameof FsUi} | GunObserver.render. triggered") getLocals
                            else
                                logger.Debug
                                    (fun () -> $"{nameof FsUi} | GunObserver.render. already disposed")
                                    getLocals
                        }
                        |> Promise.start
                        //                                )
//                            0
//                        |> ignore
                        )
                )),
            [|
                box isMountedRef
                box gun
                box store
            |]
        )

        nothing
