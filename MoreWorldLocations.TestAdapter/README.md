# Optional MWL testing adapter

This project owns the seven MWL port/shipment commands extracted from valheimCLI.
It is development tooling, separate from the shipped MWL plugin. Building or
installing MWL normally does not build, install, or reference this adapter or CLI.

The CLI core remains in `BepInEx/plugins`. The adapter can be placed in
`BepInEx/scripts` for ScriptEngine replacement. Do not copy CLI, Unity, or game
assemblies into scripts. Build against the exact CLI extension-host candidate you
install. Preview API v1 is not a compatibility promise with an older upstream CLI.

```sh
dotnet build MoreWorldLocations.TestAdapter/MoreWorldLocations.TestAdapter.csproj \
  -c Release -p:CliDll=/absolute/path/to/valheimCLI.dll
```

`GameReferences.props` contains compile references only: it deliberately does not
import MWL's production packaging/deployment targets. Symbols are embedded for
ScriptEngine. Reflection binds to the **currently registered MWL plugin**, never
to an arbitrary old assembly left behind by a reload.

| Extension command | Existing compatibility alias | Access |
|---|---|---|
| `mwl.testing/port-status` | `cli_mwl_port_status [radius]` | Read-only, loaded world |
| `mwl.testing/goto-port` | `cli_mwl_goto_port [index]` | Client mutation |
| `mwl.testing/clear-shipments` | `cli_mwl_clear_shipments` | Server mutation |
| `mwl.testing/payment-probe` | `cli_mwl_port_payment_regression [item] [count]` | Client mutation |
| `mwl.testing/delivery-probe` | `cli_mwl_port_delivery_regression [item] [count]` | Client mutation |
| `mwl.testing/ownership-seed` | `cli_mwl_port_ownership_seed [item] [count]` | Client mutation |
| `mwl.testing/ownership-check` | `cli_mwl_port_ownership_check <id> [blocked\|allowed]` | Client mutation |

Use `cli_extension <extension-command> ...` for the structured result. Aliases go
through the **same** extension dispatcher, shared operation gate, cancellation,
and permission checks. Mutations require devcommands; a joined client also needs
`AllowOnServerClients`. The adapter refuses to register if another plugin already
owns one of its aliases, and unregisters only the exact command objects it owns.
An old CLI with built-in MWL commands must be replaced before loading this adapter.

`mwl.testing/runtime` is an additional read-only capability usable at the menu. It
reports whether a live MWL plugin is registered. MWL need not be installed to test
the adapter lifecycle; port operations require it and a loaded world.

## Results and scope

The original probe implementations move with their MIT notice; their world/player
effects are not rewritten as part of extraction. Successful replies retain their
console lines and also carry structured `fields`, `source` and `complete`. Errors,
silence, duplicate fields and ambiguous multiple replies fail. Invalid arguments
are refused instead of quietly turning into a zero, default, or different action.

Transport success is not a test pass. `result=BUG_PRESENT` is a valid observation
which **fails** the external scenario. A `retry=true` ownership response is
incomplete, never auto-replayed. Teleport, shipment submission and clearing actions
are also marked incomplete: accepting their request does not prove arrival or
server-side completion. Missing collection counts remain incomplete.

Use only disposable full-MWL worlds and test characters for these port probes.
They can grant currency, create or clear shipments, open UI, load deliveries and
move the player. Unloading the adapter removes commands; it does **not** roll back
world or inventory changes. Server-only MWL excludes ports, so these probes do not
validate server-only catalogue or terrain behavior.

## Test pyramid

- `MoreWorldLocations.Tests`: existing MWL unit tests, unchanged.
- `MoreWorldLocations.TestAdapter.Tests`: engine-free tests of actual argument and
  reply rules, plus fake-transport tests of the external scenario assertions.
- `MoreWorldLocations.SystemTests`: `PortScenarios` uses shared `GameActor` and
  `ScenarioReport` for payment, delivery and ownership checks. The fixture host
  verifies environment pins, prepares the port/player, writes JSON/JUnit, and owns
  teardown. Effects are issued once. Existing monolithic probes are a migration
  stage; separating them into individual setup/action/observation operations is
  follow-up work before expanding the scenario coverage.

Preview project references default to a sibling `cli-test-framework` checkout;
set `CliDll` and `GameTestingProject` for a different layout. Once shared packages
are published, replace the development project reference with a pinned package.

```sh
dotnet test MoreWorldLocations.TestAdapter.Tests/MoreWorldLocations.TestAdapter.Tests.csproj
```

The executable in `MoreWorldLocations.SystemTests` is a **main-menu boundary
smoke**, not a port gameplay test. It refuses a loaded world or installed MWL,
then verifies alias registration, shared world preconditions, missing-dependency
observation, cleanup, and the unchanged core on the same connection. Supply an
empty owned scripts directory on the same machine as the CLI port:

```sh
dotnet run --project MoreWorldLocations.SystemTests -- \
  5555 /absolute/path/MoreWorldLocations.TestAdapter.dll \
  /absolute/disposable-game/BepInEx/scripts /absolute/results/adapter.json
```

Full MWL port scenarios still need a prepared game fixture. Fast tests do not
establish Harmony timing, RPC settlement, or persistence in the game.


For a standalone consumer, pass `-p:ToolkitPackageVersion=0.1.0-preview.3` to the
system/adapter test projects and supply the local package feed. This avoids the
sibling CLI source checkout for the external driver. The game adapter still builds
against the exact installed CLI extension API via `CliDll`. The 35 local tests
pass against this preview. Full-mode payment/delivery/ownership gameplay remains
unrun; the validated Roads terrain scenarios do not establish those MWL effects.
