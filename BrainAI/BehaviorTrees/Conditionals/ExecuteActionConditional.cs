﻿namespace BrainAI.BehaviorTrees.Conditionals
{
    using System;

    using BrainAI.BehaviorTrees.Actions;

    /// <summary>
    /// wraps an ExecuteAction so that it can be used as a Conditional
    /// </summary>
    public class ExecuteActionConditional<T> : ExecuteAction<T>, IConditional<T>
    {
        public ExecuteActionConditional( Func<T,TaskStatus> action ) : base( action )
        {}
    }
}

