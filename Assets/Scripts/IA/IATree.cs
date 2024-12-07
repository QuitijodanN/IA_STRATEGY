
using System;

public class IANode
{
    public IANode() { }
    public virtual void Action() { }
}

public class IASequenceNode : IANode
{
    protected IANode n_true, n_false;
    

    public IASequenceNode(IANode nodeTrue, IANode nodeFalse)
    {
        n_true = nodeTrue;
        n_false = nodeFalse;
    }
}

