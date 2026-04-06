# TodoApp -- System Specification

## Tracking

| Field | Value |
|---|---|
| Created | 2026-04-06 |
| State | Draft |
| Reviewed | |
| Approved | |
| Executed | |
| Verified | |
| Dependencies | None (base system specification) |

A console application that manages a personal todo list. Users add, complete, delete, and list todo items through a numbered menu. Items persist to a JSON file so they survive between sessions. The domain logic is separated from console IO following The Standard's broker/service/exposer decomposition.

## Architecture

```spec
architecture TheStandard {
    version: "1.0";
    enforce: [layers, flow_forward, florance,
              entity_ownership, autonomy, vocabulary];

    vocabulary {
        broker:      [Insert, Select, Update, Delete];
        foundation:  [Add, Retrieve, Modify, Remove];
        processing:  [Ensure, Upsert, Verify, TryRemove];
        exposer:     [Post, Get, Put, Delete];
    }

    realize broker {
        "Use partial classes to separate entity-specific operations
         into distinct files (e.g., StorageBroker.TodoItems.cs).
         The base StorageBroker.cs holds configuration and generic
         JSON file read/write methods.";

        "Support brokers (DateTimeBroker, LoggingBroker) are single-file,
         single-purpose classes. No partial class splitting needed.";
    }

    realize foundation {
        "Each foundation service class uses a TryCatch partial class
         (e.g., TodoItemService.Exceptions.cs) that contains the exception
         noise cancellation delegate mapping native exceptions to local
         categorical models.";

        "Validation logic lives in a separate partial class
         (e.g., TodoItemService.Validations.cs) with methods named
         ValidateTodoItemOnAdd, ValidateTodoItemOnModify, etc.";
    }

    realize test {
        "Every service gets its own test class. Foundation service tests
         verify each validation rule individually: one test per structural
         validation, one per logical validation, one per external check.";

        "Use Moq for mock setup. Use FluentAssertions for assertion
         readability. Use Force.DeepCloner for expected value isolation.";

        "The Happy Path test for each CRUD operation is the first test
         written. Validation tests follow.";
    }

    realize exposer {
        "The console exposer is a simple menu loop. Each menu option
         maps to exactly one foundation service call. No business logic
         in the console layer; it only handles IO formatting and
         delegates to the service.";
    }

    rationale {
        context "TodoApp follows The Standard for clean, testable,
                 and maintainable architecture.";
        decision "Full enforcement of all rule sets. Layer contracts
                  define formal obligations. Realization directives
                  guide code style.";
        consequence "Components are declared with Standard vocabulary:
                     broker, foundation service, exposer. LLM realization
                     generates code matching each layer's role.";
    }
}
```

## System Declaration

```spec
system TodoApp {
    target: "net10.0";
    responsibility: "Console application for managing a personal todo list.
                     Supports add, complete, delete, and list operations
                     with JSON file persistence.";

    // --- Consumed components: external dependencies ---

    consumed component Moq {
        source: nuget("Moq");
        version: "4.*";
        contract {
            guarantees "Mock object creation and verification for
                        unit testing.";
        }
        rationale {
            context "The Standard requires Moq for mock setup in
                     unit tests.";
            decision "Use Moq as the mocking framework.";
            consequence "All service tests use Mock<T> for broker
                         dependencies.";
        }
    }

    consumed component FluentAssertions {
        source: nuget("FluentAssertions");
        version: "8.*";
        contract {
            guarantees "Readable assertion syntax for unit tests.";
        }
        rationale {
            context "The Standard recommends FluentAssertions for
                     assertion readability.";
            decision "Use FluentAssertions for all test assertions.";
            consequence "Tests use .Should() syntax instead of
                         Assert methods.";
        }
    }

    consumed component Force.DeepCloner {
        source: nuget("Force.DeepCloner");
        version: "1.*";
        contract {
            guarantees "Deep cloning of objects for expected value
                        isolation in tests.";
        }
        rationale {
            context "The Standard requires deep cloning to detect
                     unintended mutations in tests.";
            decision "Use Force.DeepCloner for expected value
                      isolation.";
            consequence "Tests clone input objects before assertions
                         to verify no mutation occurred.";
        }
    }

    consumed component xunit {
        source: nuget("xunit");
        version: "2.*";
        contract {
            guarantees "Test framework for discovering and running
                        unit tests.";
        }
        rationale {
            context "xUnit is the standard test framework for .NET
                     projects following The Standard.";
            decision "Use xUnit as the test runner.";
            consequence "Test classes use [Fact] and [Theory]
                         attributes.";
        }
    }

    // --- Brokers: authored wrappers around external resources ---

    broker TodoApp.Brokers.Storage {
        kind: library;
        path: "src/TodoApp.Brokers.Storage";
        responsibility: "Storage broker. Wraps JSON file read/write
                         operations using System.Text.Json. Partial
                         classes separate entity-specific operations
                         into StorageBroker.TodoItems.cs.";
    }

    broker TodoApp.Brokers.DateTime {
        kind: library;
        path: "src/TodoApp.Brokers.DateTime";
        responsibility: "Support broker. Abstracts the system clock
                         for testable time-dependent logic.";
    }

    broker TodoApp.Brokers.Logging {
        kind: library;
        path: "src/TodoApp.Brokers.Logging";
        responsibility: "Support broker. Wraps ILogger for structured
                         logging.";
    }

    // --- Foundation services: validation and primitive CRUD ---

    foundation service TodoApp.Services.Foundations.TodoItems {
        kind: library;
        path: "src/TodoApp.Services/Foundations/TodoItems";
        owns: TodoItem;
        responsibility: "Validation and primitive CRUD for TodoItem
                         entities. Enforces title-not-empty and
                         unique-ID rules. Single entity integration
                         with the StorageBroker.";
    }

    // --- Exposer: console host ---

    exposer TodoApp.Console {
        kind: application;
        path: "src/TodoApp.Console";
        responsibility: "Console host. Displays numbered menu, reads
                         user input, delegates to the foundation
                         service, and formats output. No business
                         logic.";
    }

    // --- Tests ---

    test TodoApp.Tests {
        kind: tests;
        path: "tests/TodoApp.Tests";
        responsibility: "Unit tests for all layers. Every service is
                         fully unit-tested. Broker tests verify JSON
                         file operations. Foundation tests verify each
                         validation rule individually.";
    }
}
```

## Platform Realization

```spec
dotnet solution TodoApp {
    format: "slnx";
    folder "src" {
        projects: [TodoApp.Brokers.Storage,
                   TodoApp.Brokers.DateTime,
                   TodoApp.Brokers.Logging,
                   TodoApp.Services,
                   TodoApp.Console];
    }
    folder "tests" {
        projects: [TodoApp.Tests];
    }
    startup: TodoApp.Console;

    rationale {
        context "Standard .NET solution structure with src and tests
                 folders.";
        decision "Single solution with slnx format for .NET 10.";
        consequence "All projects build from one solution root.
                     The console app is the startup project.";
    }
}
```

## Package Policy

```spec
package_policy {
    default: require_rationale;

    allow test_only {
        packages: [Moq, FluentAssertions, Force.DeepCloner, xunit,
                   xunit.runner.visualstudio,
                   Microsoft.NET.Test.Sdk];
        rationale "Standard test tooling required by The Standard's
                   testing conventions.";
    }
}
```

## Topology

```spec
topology {
    // Foundation depends on its entity broker plus support brokers
    allow TodoApp.Services.Foundations.TodoItems
        -> [TodoApp.Brokers.Storage,
            TodoApp.Brokers.DateTime,
            TodoApp.Brokers.Logging];

    // Exposer depends on the foundation service (no orchestration
    // layer needed for a single-entity system)
    allow TodoApp.Console
        -> [TodoApp.Services.Foundations.TodoItems];

    // Tests can reach all layers
    allow TodoApp.Tests
        -> [TodoApp.Brokers.Storage,
            TodoApp.Brokers.DateTime,
            TodoApp.Brokers.Logging,
            TodoApp.Services.Foundations.TodoItems,
            TodoApp.Console];

    // Brokers must not call services
    deny TodoApp.Brokers.Storage
        -> [TodoApp.Services.Foundations.TodoItems];

    // Foundation must not call exposer
    deny TodoApp.Services.Foundations.TodoItems
        -> [TodoApp.Console];
}
```

## Phases

```spec
phase Scaffold {
    scope: [TodoApp.Brokers.Storage,
            TodoApp.Brokers.DateTime,
            TodoApp.Brokers.Logging,
            TodoApp.Services.Foundations.TodoItems,
            TodoApp.Console,
            TodoApp.Tests];

    gate {
        command: "dotnet build TodoApp.slnx";
        expects: "exit_code == 0";
    }

    rationale "All projects compile with correct references before
               any business logic is written.";
}

phase Foundation {
    scope: [TodoApp.Services.Foundations.TodoItems,
            TodoApp.Tests];

    gate {
        command: "dotnet test tests/TodoApp.Tests";
        expects: "exit_code == 0";
    }

    gate {
        command: "dotnet test tests/TodoApp.Tests --filter Category=Foundation";
        expects: "test_count >= 12";
    }

    rationale "Foundation service with full validation and CRUD tests.
               At least 12 tests: happy path for Add, Retrieve, Modify,
               Remove (4), plus structural and logical validation tests
               for each operation.";
}

phase Exposer {
    scope: [TodoApp.Console,
            TodoApp.Tests];

    gate {
        command: "dotnet test tests/TodoApp.Tests";
        expects: "exit_code == 0";
    }

    gate {
        command: "dotnet run --project src/TodoApp.Console -- --smoke-test";
        expects: "exit_code == 0";
    }

    rationale "Console host wires the menu to the foundation service.
               Smoke test verifies the app starts and can quit cleanly.";
}
```

## Layer Contracts

```spec
layer_contract FoundationContract {
    layer: foundation;

    guarantees "Every public method is wrapped in a TryCatch exception
                noise cancellation delegate that catches native broker
                exceptions and maps them to categorical local exception
                models (ValidationException, DependencyException,
                ServiceException).";

    guarantees "Validation executes in order: structural (null checks,
                required fields, default values), then logical (cross-field
                rules, value ranges, business invariants), then external
                (existence checks against external resources). Structural
                failures short-circuit before logical; logical failures
                short-circuit before external.";

    guarantees "Each public method calls exactly one broker method.
                No method combines multiple broker calls. Higher-order
                combinations belong to the processing layer.";

    guarantees "The service speaks business language, not storage language:
                Add (not Insert), Retrieve (not Select), Modify (not Update),
                Remove (not Delete).";
}

layer_contract BrokerContract {
    layer: broker;

    guarantees "No flow control: no if-statements, no while-loops, no
                switch-cases. The broker delegates to the external resource
                and returns the result.";

    guarantees "No exception handling. Native exceptions propagate to the
                foundation layer, which maps them to local models.";

    guarantees "The broker speaks the language of the technology it wraps:
                Insert, Select, Update, Delete for storage.";

    guarantees "Partial classes organize entity-specific operations into
                separate files: StorageBroker.TodoItems.cs.";
}

layer_contract ExposerContract {
    layer: exposer;

    guarantees "The console exposer is a pure mapping layer. No business
                logic, no validation, no sequencing beyond reading user
                input and calling the corresponding service method.";

    guarantees "Each menu option maps to exactly one service call.
                The exposer does not combine multiple service calls in
                a single menu action.";

    guarantees "Error display: ValidationException maps to a user-friendly
                error message. DependencyException maps to a storage error
                message. ServiceException maps to a generic error message.";
}

layer_contract TestContract {
    layer: test;

    guarantees "Tests follow Given/When/Then structure with randomized
                inputs: CreateRandomTodoItem().";

    guarantees "Expected values are deep-cloned from input values to
                detect unintended mutations: expectedTodoItem =
                inputTodoItem.DeepClone().";

    guarantees "Mock verification uses Times.Once for each expected call,
                followed by VerifyNoOtherCalls() on every mock to ensure
                no unexpected broker or service interactions.";

    guarantees "Test naming follows MethodName_Scenario_ExpectedResult
                convention.";
}
```

## Data Model

```spec
entity TodoItem {
    id: int @confidence(high);
    title: string @confidence(high);
    isCompleted: bool @default(false) @confidence(high);

    invariant "title not empty": length(title) > 0;
    invariant "id is positive": id > 0;

    rationale "id" {
        context "Each item needs a unique identifier for the complete
                 and delete operations.";
        decision "Auto-incrementing integer. IDs are never reused,
                  even after deletion.";
        consequence "The storage broker tracks the next available ID
                     independently of the current item list. Deleting
                     item 3 does not make ID 3 available again.";
    }

    rationale "isCompleted" {
        context "Users mark items as done through the Complete menu
                 option.";
        decision "Boolean flag, defaults to false on creation.";
        consequence "Completed items display with an X marker in
                     the list view.";
    }
}
```

## Contracts

```spec
contract AddTodoItem {
    requires length(todoItem.title) > 0
        @validation(structural);
    requires todoItem.id == 0
        @validation(structural);
    ensures todoItem.id > 0;
    ensures todoItem.isCompleted == false;
    guarantees "The returned item has a unique ID that has never been
                assigned to any previous item, including deleted items.";
}

contract CompleteTodoItem {
    requires todoItem.id > 0
        @validation(structural);
    requires exists(items, item => item.id == todoItem.id)
        @validation(external);
    ensures todoItem.isCompleted == true;
}

contract RemoveTodoItem {
    requires todoItem.id > 0
        @validation(structural);
    requires exists(items, item => item.id == todoItem.id)
        @validation(external);
    ensures not exists(items, item => item.id == todoItem.id);
}

contract RetrieveAllTodoItems {
    ensures result != null;
    guarantees "Returns all items currently in the list, in insertion
                order. Completed items are included with their
                isCompleted flag set to true.";
}
```

## Constraints

```spec
constraint IdUniqueness {
    scope: [TodoApp.Services.Foundations.TodoItems,
            TodoApp.Brokers.Storage];
    rule: "IDs are monotonically increasing and never reused.
           The next available ID is persisted alongside the item list.
           Deleting an item does not free its ID for reassignment.";
}

constraint DomainIoSeparation {
    scope: [TodoApp.Services.Foundations.TodoItems,
            TodoApp.Console];
    rule: "All domain logic (adding, completing, deleting, listing,
           persisting) resides in the foundation service and broker
           layers. The console exposer contains no business logic and
           could be replaced with a different host without changing
           domain behavior.";
}
```

## Traceability

```spec
trace TodoItemManagement {
    concepts: [TodoItem, AddTodoItem, CompleteTodoItem,
               RemoveTodoItem, RetrieveAllTodoItems];
    components: [TodoApp.Brokers.Storage,
                 TodoApp.Services.Foundations.TodoItems,
                 TodoApp.Console];
    tests: [TodoApp.Tests];

    rationale "Core feature trace. Every CRUD operation on TodoItem
               is traceable from the entity definition through the
               foundation service contract to the console menu option
               and the corresponding test class.";
}

trace Persistence {
    concepts: [TodoItem, IdUniqueness];
    components: [TodoApp.Brokers.Storage];
    tests: [TodoApp.Tests];

    rationale "Persistence trace. JSON file storage and ID generation
               are traceable from the constraint through the storage
               broker to its tests.";
}
```
