namespace FsUi.Components

open FsJs
open FsStore.Model
open FsStore.State
open FsUi.Hooks
open Feliz
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fable.React


module ToastObserver =
    [<ReactComponent>]
    let ToastObserver () =
        let toast = Ui.useToast ()
        let deviceInfo = Store.useValue Selectors.Store.deviceInfo
        let appState, setAppState = Store.useState (Atoms.Device.appState deviceInfo.DeviceId)

        React.useEffect (
            (fun () ->
                appState.NotificationQueue
                |> List.iter
                    (fun notification ->
                        let status, title, description =
                            match notification with
                            | Notification.Success (title, description) -> "success", title, description
                            | Notification.Error (title, description, error) ->
                                let getLocals () =
                                    $"title={title} description={description} error={error}"

                                Logger.logError (fun () -> $"{nameof FsUi} | ToastObserver error") getLocals
                                "error", title, description
                            | Notification.Warning (title, description) -> "warning", title, description
                            | Notification.Info (title, description) -> "info", title, description

                        toast
                            (fun x ->
                                x.status <- status
                                x.title <- title
                                x.description <- description))

                if not appState.NotificationQueue.IsEmpty then
                    setAppState { appState with NotificationQueue = [] }),
            [|
                box appState
            |]
        )

        nothing
