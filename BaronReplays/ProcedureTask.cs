using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public abstract class ProcedureTask
    {
        protected delegate bool Task();
        protected delegate void TaskResult(bool isSuccess, String msg);
        protected TaskResult TaskDoneEvent;

        protected List<Task> tasks;
        protected Dictionary<Task, String> tasksErrorMsg;

        protected ProcedureTask()
        {
            tasks = new List<Task>();
            tasksErrorMsg = new Dictionary<Task, string>();
        }

        protected void AddTask(Task t, String errorMsg)
        {
            tasks.Add(t);
            tasksErrorMsg.Add(t, errorMsg);
        }

        protected void AddTask(Task t)
        {
            tasks.Add(t);
        }

        protected void RunTasks()
        {
            foreach (Task task in tasks)
            {
                bool isSuccess;
                try
                {
                    isSuccess = task();
                }
                catch (Exception e)
                {
                    isSuccess = false;
                }
                if (!isSuccess)
                {
                    if (TaskDoneEvent != null)
                    {
                        String msg = String.Empty;
                        if (tasksErrorMsg.ContainsKey(task))
                            msg = tasksErrorMsg[task];
                        TaskDoneEvent(false, msg);
                    }
                    return;
                }
                
            }
            if(TaskDoneEvent!=null)
                TaskDoneEvent(true, String.Empty);
        }


        
    }
}
