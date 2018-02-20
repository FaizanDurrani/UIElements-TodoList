using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Todo : ScriptableObject
{
    private List<string> _taskKeys;
    private List<bool> _taskValues;

    public Todo()
    {
        _taskValues = new List<bool>();
        _taskKeys = new List<string>();
    }

    public List<string> GetAllTasks()
    {
        return _taskKeys;
    }

    public bool? GetTask(string task)
    {
        int i = _taskKeys.IndexOf(task);
        if (i >= 0)
        {
            return _taskValues[i];
        }
        
        return null;
    }

    public void NewTask(string task)
    {
        _taskKeys.Add(task);
        _taskValues.Add(false);
    }

    public void UpdateTask(string task)
    {
        int i = _taskKeys.IndexOf(task);
        if (i >= 0)
        {
            _taskValues[i] = !_taskValues[i];
        }
    }

    public bool ContainsTask(string task)
    {
        return _taskKeys.IndexOf(task) >= 0;
    }

    public void RemoveTask(string task)
    {
        _taskValues.RemoveAt(_taskKeys.IndexOf(task));
        _taskKeys.Remove(task);
    }
}