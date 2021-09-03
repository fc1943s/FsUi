namespace FsUi.State.Atoms

open FsStore
open FsUi.Model
open FsUi
open FsStore.Model


module rec Ui =
    let rec darkMode =
        Engine.createAtomWithSubscriptionStorage
            (RootAtomPath (FsUi.storeRoot, AtomName (nameof darkMode)))
            UiState.Default.DarkMode

    let rec fontSize =
        Engine.createAtomWithSubscriptionStorage
            (RootAtomPath (FsUi.storeRoot, AtomName (nameof fontSize)))
            UiState.Default.FontSize

    let rec systemUiFont =
        Engine.createAtomWithSubscriptionStorage
            (RootAtomPath (FsUi.storeRoot, AtomName (nameof systemUiFont)))
            UiState.Default.SystemUiFont
