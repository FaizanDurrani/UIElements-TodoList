using System;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using Object = UnityEngine.Object;

public class TodoEditor : EditorWindow
{
    private static Todo _selectedTodo;
    private TextField _newTaskName;
    private VisualElement _alreadyExistsBox;

    [MenuItem("Tools/Todo List")]
    private static void Init()
    {
        var window = GetWindow<TodoEditor>();
        window.Show();
    }

    private void NewTodoList()
    {
        var sc = CreateInstance<Todo>();
        ProjectWindowUtil.CreateAsset(sc, "Assets/TodoLists/TodoList.asset");
        _selectedTodo = Resources.Load<Todo>("Todo");
        RebuildUi();
    }

    private void OnFocus()
    {
        RebuildUi();
    }

    private void OnEnable()
    {
        BuildUi();
    }

    private void RebuildUi()
    {
        if (_selectedTodo != null)
        {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(_selectedTodo);
            AssetDatabase.SaveAssets();
        }

        this.GetRootVisualContainer().Clear();
        BuildUi();
    }

    private void BuildUi()
    {
        var root = this.GetRootVisualContainer();
        root.AddStyleSheetPath("styles");

        TodoSelectionSection(root);

        if (_selectedTodo == null)
            return;

        TaskAlreadyExistsBox(root);
        TaskInputSection(root);
        TaskListSection(root);
    }

    private void TaskAlreadyExistsBox(VisualElement root)
    {
        _alreadyExistsBox = new VisualElement {name = "alreadyExistsBox"};
        root.Add(_alreadyExistsBox);
        
        Label errorLabel = new Label("The Task with that name already exists");
        _alreadyExistsBox.Add(errorLabel);
        _alreadyExistsBox.SetEnabled(false);
    }

    private void NewTodoTask()
    {
        if (_alreadyExistsBox.enabledSelf || _newTaskName.text.Trim().Length == 0)
            return;

        _selectedTodo.NewTask(_newTaskName.text.Trim());
        _newTaskName.text = "";
        RebuildUi();
    }

    private void UpdateTodoTask(string task)
    {
        _selectedTodo.UpdateTask(task);
        RebuildUi();
    }

    private void DeleteTodoTask(string task)
    {
        _selectedTodo.RemoveTask(task);
        RebuildUi();
    }

    private void SelectTodoList(Todo list)
    {
        _selectedTodo = list;
        RebuildUi();
    }

    private void TaskNameKeyDownCallback(KeyUpEvent evt)
    {
        if (evt.shiftKey || evt.commandKey || evt.altKey || evt.ctrlKey)
            return;
        
        _alreadyExistsBox.SetEnabled(_selectedTodo.ContainsTask(_newTaskName.text.Trim()));
        if (evt.keyCode != KeyCode.Return)
            return;
        
        NewTodoTask();
        evt.StopImmediatePropagation();
    }

    private void TodoSelectionSection(VisualElement root)
    {
        GenericMenu menu = new GenericMenu();
        string[] todos = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
        foreach (string todoPath in todos)
        {
            string todoAssetPath = "Assets" + todoPath.Replace(Application.dataPath, "").Replace('\\', '/');
            Todo todo = AssetDatabase.LoadAssetAtPath<Todo>(todoAssetPath);
            if (todo != null)
                menu.AddItem(new GUIContent(todo.name), false, () => SelectTodoList(todo as Todo));
        }

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("New List"), false, NewTodoList);

        Button todoContextButton = new Button(menu.ShowAsContext)
        {
            name = "currTodoButton",
            persistenceKey = "currTodoButton",
            text = _selectedTodo != null ? _selectedTodo.name : "Select a List"
        };

        root.Add(todoContextButton);
    }

    private void TaskInputSection(VisualElement root)
    {
        VisualContainer row = new VisualContainer {name = "newTaskRow"};
        root.Add(row);
        if (_newTaskName == null)
        {
            _newTaskName = new TextField() {name = "newTaskNameInput"};
            _newTaskName.RegisterCallback<KeyUpEvent>(TaskNameKeyDownCallback);
        }

        row.Add(_newTaskName);

        Button newTaskButton = new Button(NewTodoTask) {name = "newTaskButton", text = "Add Task"};
        row.Add(newTaskButton);
    }

    private void TaskListSection(VisualElement root)
    {
        VisualContainer list = new VisualContainer {name = "taskList"};
        root.Add(list);
        foreach (string task in _selectedTodo.GetAllTasks())
        {
            VisualContainer row = new VisualContainer();
            row.AddToClassList("taskRow");
            list.Add(row);

            Label taskNameLabel = new Label(task);
            taskNameLabel.AddToClassList("taskNameLabel");

            if (_selectedTodo.GetTask(task) ?? true)
                taskNameLabel.AddToClassList("taskDone");

            row.Add(taskNameLabel);

            Button taskUpdateButton = new Button(() => UpdateTodoTask(task));
            taskUpdateButton.AddToClassList("taskUpdateButton");
            if (_selectedTodo.GetTask(task) ?? true)
            {
                taskNameLabel.AddToClassList("taskDone");
                taskUpdateButton.text = "Not Done";
            }
            else
                taskUpdateButton.text = "Done";

            row.Add(taskUpdateButton);

            Button taskDeleteButton = new Button(() => DeleteTodoTask(task)) {text = "Delete"};
            taskDeleteButton.AddToClassList("taskDeleteButton");
            row.Add(taskDeleteButton);
        }
    }
}