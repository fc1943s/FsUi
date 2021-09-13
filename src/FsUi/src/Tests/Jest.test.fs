namespace FsUi.Tests

open Fable.Jester
open FsJs.Bindings


module Jest =
    Jest.test (
        "trace log",
        promise {
            ()
        },
        Jest.maxTimeout
    )
