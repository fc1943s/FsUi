namespace FsUi.State

open FsCore
open FsJs
open FsStore
open FsUi.Model
open FsUi.State


module Actions =
    let hydrateUiState =
        Atom.Primitives.setSelector
            (fun _getter setter (uiState: UiState) ->
                Profiling.addTimestamp (fun () -> $"{nameof FsUi} | Actions.hydrateUiState") getLocals
                Atom.set setter Atoms.Ui.darkMode uiState.DarkMode
                Atom.set setter Atoms.Ui.fontSize uiState.FontSize
                Atom.set setter Atoms.Ui.systemUiFont uiState.SystemUiFont)
