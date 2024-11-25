using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace BehaviourTree
{
    public interface IStrategy
    {
        Node.Status Process();

    public class Leaf : Node
    {
        readonly IStrategy strategy;
    }

    public class Node
    {
        public enum Status { Success, Failure, Running }

        public readonly string name;

        public readonly List<Node> children;
        protected int currentChild;

        public Node(string name = "Node") { 
            this.name = name;
        }

        public void AddChild(Node child) => children.Add(child);

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset()
        {
            foreach(Node child in children)
            {
                child.Reset();
            }
        }
    }
}
