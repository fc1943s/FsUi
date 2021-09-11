namespace FsUi.Components

open FsStore
open FsCore
open Feliz
open FsJs
open FsStore.Hooks
open FsUi.Bindings
open FsUi.Hooks


module MessagesListener =
    [<ReactComponent>]
    let MessagesListener () =
        Profiling.addTimestamp (fun () -> $"{nameof FsUi} | MessagesListener [ render ] ") getLocals
        let messageIdAtoms = Store.useValue Subscriptions.messageIdAtoms

        React.fragment [
            yield!
                messageIdAtoms
                |> Array.map MessageConsumer.MessageConsumer
        ]
