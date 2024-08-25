SquiggleCop originally used the SARIF SDK (and ideally will again). However, the improvements of moving from
`Newtonosft.Json` to `System.Text.Json` were too big to ignore. As a result, this directory contains a hand-rolled
object model for SARIF files that contains only the members SquiggleCop needs (and ignores unmapped properties).

https://github.com/microsoft/sarif-sdk/issues/1989 tracks migrating to STJ, at which point SquiggleCop should switch
back to the official SDK.
