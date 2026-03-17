namespace MonsterSystem
{
    /// <summary>
    /// Interface for states that receive context data during transitions.
    /// Implement this on any state that needs to receive typed data when entered.
    /// </summary>
    /// <typeparam name="T">The type of context data this state expects</typeparam>
    public interface IStateWithContext<T>
    {
        /// <summary>
        /// Called immediately before OnStateEnter when transitioning with context.
        /// Use this to store the context data for use during the state's lifetime.
        /// </summary>
        void ReceiveContext(T context);
    }
}
