using UnityEngine;

namespace GDEUtils.StateMachine
{
    public class State<T> : MonoBehaviour
    {
        public virtual void Enter(T entity) { }
        public virtual void Exit() { }
        public virtual void Execute() { }
    }
}