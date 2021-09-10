namespace FsUi.Components

open FsCore
open FsJs
open FsUi.Bindings


module LoadingSpinner =
    let inline LoadingSpinner () =
        Profiling.addTimestamp (fun () -> $"{nameof FsUi} | LoadingSpinner / render") getLocals

        Ui.center
            (fun x -> x.flex <- "1")
            [
                Ui.stack
                    (fun x -> x.alignItems <- "center")
                    [
                        Spinner.Spinner (fun _ -> ())
                        Ui.str "Loading..."
                    ]
            ]

    let inline InlineLoadingSpinner () =
        Profiling.addTimestamp (fun () -> $"{nameof FsUi} | InlineLoadingSpinner / render") getLocals

        Ui.flex
            (fun x -> x.alignItems <- "center")
            [
                Spinner.Spinner
                    (fun x ->
                        x.width <- "10px"
                        x.height <- "10px")
            ]
