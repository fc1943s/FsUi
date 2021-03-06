namespace FsUi.Components

open FsUi.Hooks
open Fable.React
open Feliz
open FsUi.Bindings


module Popover =
    [<ReactComponent>]
    let CustomPopover
        (input: {| Trigger: ReactElement
                   CloseButton: bool
                   Padding: string option
                   Body: Ui.Disclosure * (unit -> IRefValue<unit>) -> ReactElement list
                   Props: Ui.IChakraProps -> unit |})
        =
        let disclosure = Ui.useDisclosure ()

        let initialFocusRef = React.useRef ()

        if not disclosure.isOpen then
            Ui.box
                (fun x ->
                    x.display <- "inline"

                    x.onClick <-
                        fun e ->
                            promise {
                                e.preventDefault ()
                                disclosure.onOpen ()
                            })
                [
                    input.Trigger
                ]
        else
            Ui.box
                (fun x -> x.display <- "inline")
                [
                    Ui.popover
                        (fun x ->
                            //                            x.isLazy <- true

                            x.closeOnBlur <- true
                            x.autoFocus <- true
                            //                            x.computePositionOnMount <- false
                            x.defaultIsOpen <- true
                            x.initialFocusRef <- initialFocusRef
                            x.isOpen <- disclosure.isOpen
                            x.onOpen <- disclosure.onOpen

                            x.onClose <- fun x -> promise { disclosure.onClose x }
                            input.Props x)
                        [
                            Ui.popoverTrigger
                                (fun _ -> ())
                                [
                                    Ui.box
                                        (fun x -> x.display <- "inline")
                                        [
                                            input.Trigger
                                        ]
                                ]

                            let mutable initialFocusRefFetched = false

                            let fetchInitialFocusRef =
                                fun () ->
                                    initialFocusRefFetched <- true
                                    initialFocusRef

                            match input.Body (disclosure, fetchInitialFocusRef) with
                            | [ x ] when x = nothing -> nothing
                            | content ->
                                Ui.popoverContent
                                    (fun x ->
                                        x.width <- "auto"
                                        x.border <- "0"
                                        x.boxShadow <- "lg"
                                        x.borderRadius <- "0")
                                    [
                                        Ui.popoverArrow (fun _ -> ()) []

                                        if not input.CloseButton then
                                            nothing
                                        else
                                            Button.Button
                                                {|
                                                    Tooltip = None
                                                    Icon =
                                                        Some (
                                                            Icons.io.IoMdClose |> Icons.render,
                                                            Button.IconPosition.Left
                                                        )
                                                    Props =
                                                        fun x ->
                                                            x.position <- "absolute"
                                                            x.right <- "0"
                                                            x.margin <- "5px"
                                                            x.minWidth <- "20px"
                                                            x.height <- "20px"

                                                            if not initialFocusRefFetched then x.ref <- initialFocusRef

                                                            x.onClick <-
                                                                fun e ->
                                                                    promise {
                                                                        e.preventDefault ()
                                                                        disclosure.onClose ()
                                                                    }
                                                    Children = []
                                                |}

                                        Ui.popoverBody
                                            (fun x ->
                                                x.padding <- input.Padding |> Option.defaultValue "15px"
                                                x.backgroundColor <- "gray.13"
                                                x.maxWidth <- "95vw"
                                                x.maxHeight <- "95vh"
                                                x.overflow <- "auto")
                                            [
                                                yield! content
                                            ]
                                    ]
                        ]
                ]

    let inline Popover
        (input: {| Trigger: ReactElement
                   Body: Ui.Disclosure * (unit -> IRefValue<unit>) -> ReactElement list |})
        =
        CustomPopover
            {| input with
                CloseButton = true
                Padding = None
                Props = fun _ -> ()
            |}

    let inline MenuItemPopover
        (input: {| Trigger: ReactElement
                   Body: Ui.Disclosure * (unit -> IRefValue<unit>) -> ReactElement list |})
        =
        CustomPopover
            {| input with
                CloseButton = true
                Padding = None
                Props = fun x -> x.closeOnBlur <- false
            |}

    [<ReactComponent>]
    let CustomConfirmPopover (props: Ui.IChakraProps -> unit) closeButton trigger onConfirm children =
        let isMountedRef = React.useIsMountedRef ()

        CustomPopover
            {|
                Props = props
                Padding = None
                CloseButton = closeButton
                Trigger = trigger
                Body =
                    fun (disclosure, fetchInitialFocusRef) ->
                        [
                            Ui.stack
                                (fun x -> x.spacing <- "10px")
                                [
                                    let mutable initialFocusRefFetched = false

                                    let fetchInitialFocusRef2 =
                                        fun () ->
                                            initialFocusRefFetched <- true
                                            fetchInitialFocusRef ()

                                    let children =
                                        children (disclosure, fetchInitialFocusRef2)
                                        |> Seq.toList

                                    yield! children

                                    Ui.box
                                        (fun _ -> ())
                                        [
                                            Button.Button
                                                {|
                                                    Tooltip = None
                                                    Icon =
                                                        Some (
                                                            Icons.fi.FiCheck |> Icons.render,
                                                            Button.IconPosition.Left
                                                        )
                                                    Props =
                                                        fun x ->
                                                            if children.IsEmpty || not initialFocusRefFetched then
                                                                x.ref <- fetchInitialFocusRef ()

                                                            x.onClick <-
                                                                fun e ->

                                                                    promise {
                                                                        e.preventDefault ()

                                                                        let! result = onConfirm ()

                                                                        if result && isMountedRef.current then
                                                                            disclosure.onClose ()
                                                                    }
                                                    Children =
                                                        [
                                                            str "Confirm"
                                                        ]
                                                |}
                                        ]
                                ]
                        ]
            |}

    let inline ConfirmPopover trigger onConfirm children =
        CustomConfirmPopover (fun _ -> ()) true trigger onConfirm children

    let inline MenuItemConfirmPopover icon label onConfirm =
        CustomConfirmPopover
            (fun x -> x.closeOnBlur <- false)
            false
            (MenuItem.MenuItem icon label None (fun x -> x.closeOnSelect <- false))
            onConfirm
            (fun _ -> [])
