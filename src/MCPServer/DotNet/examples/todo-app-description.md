# Todo App

A console application that manages a personal todo list.

## What it does

The user sees a numbered menu when the app starts:

1. Add item
2. Complete item
3. Delete item
4. List items
5. Quit

**Add item** prompts for a title and adds it to the list. Each item gets a unique ID assigned automatically.

**Complete item** prompts for an item ID and marks it as done.

**Delete item** prompts for an item ID and removes it from the list.

**List items** shows all items with their ID, title, and completion status. Completed items are marked with an X.

**Quit** exits the app.

## Persistence

Items are saved to a JSON file in the current directory so they survive between sessions. The file is created on first use.

## Rules

- Item titles cannot be empty.
- IDs are never reused. Deleting item 3 does not make ID 3 available again.
- The domain logic (adding, completing, deleting, listing, persisting) must be separate from the console IO so it could be reused in a different host.
