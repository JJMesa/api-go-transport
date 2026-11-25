// Each test class boots its own host through Program, whose top-level code assigns and freezes
// Serilog's static bootstrap logger. Hosts built in parallel race on that shared logger ("The logger
// is already frozen"), so component test classes run one at a time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
