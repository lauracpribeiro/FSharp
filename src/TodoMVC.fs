module TodoMVC

open System
open Fable.Core
open Fable.React
open Feliz
open Browser.Types

// For React Fast Refresh to work, the file must have **one single export**
// This is shy it is important to set the inner modules as private

module private Elmish =
    open Elmish

    type Todo =
        {
            Id: Guid
            Description: string
            Display: bool
        }

    type State = { Todos: Todo list }

    type Msg =
        | ToggleDisplay of Guid

    let newTodo txt =
        {
            Id = Guid.NewGuid()
            Description = txt
            Display = true
        }

    let initTodos (count: int) =
        [
            newTodo "Market Data"
            { newTodo "Blotter" with Display = false}
            
        ]

    let init (count: int) = { Todos = initTodos count }, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with

        | ToggleDisplay todoId ->
            state.Todos
            |> List.map (fun todo ->
                if todo.Id = todoId then
                    let completed = not todo.Display
                    { todo with Display = completed }
                else
                    todo)
            |> fun todos -> { state with Todos = todos }, Cmd.none


module private Components =
    open Elmish

    [<JSX.Component>]
    let Button (iconClass: string) (classes: (string * bool) list) dispatch =
     
        
        JSX.jsx
            $"""
        <button type="button"
                onClick={fun _ -> dispatch ()}
                style={toStyle [ style.marginRight (length.px 4) ]}
                className={toClass [ "button", true; yield! classes ]}>
            <i className={iconClass}></i>
        </button>
        """

    [<JSX.Component>]
    let TodoView dispatch (todo: Todo) (key: Guid) =
        let inputRef = React.useRef<HTMLInputElement option> (None)
        let edit, setEdit = React.useState<string option> (None)
        let isEditing = Option.isSome edit


        React.useEffect (
            (fun () ->
                if isEditing then
                    inputRef.current.Value.select ()
                    inputRef.current.Value.focus ()

                None),
            [| box isEditing |]
        )

        JSX.jsx
            $"""

        <li className="box">
            <div className="columns">
                <div className="column is-7">
                {match edit with
                 | None ->
                     Html.p
                         [
                             prop.className "subtitle"
                             prop.style [ style.userSelect.none; style.cursor.pointer ]
                             prop.children [ Html.text todo.Description ]
                         ]}                
                </div>
                {Html.div
                     [
                         prop.className "column is-2"
                         prop.children
                             [

                                    Button "fa fa-check" [ "is-success", todo.Display ] (fun () -> ToggleDisplay todo.Id |> dispatch)
                                     |> toReact


                             ]
                     ]}
            </div>
        </li>
        """

open Elmish
open Components

[<JSX.Component>]
let App () =
    let model, dispatch = React.useElmish (init, update, arg = 2)

    JSX.jsx
        $"""
    <div className="container">
        <ul>{model.Todos |> List.map (fun t -> TodoView dispatch t t.Id)}</ul>
    </div>
    """



       