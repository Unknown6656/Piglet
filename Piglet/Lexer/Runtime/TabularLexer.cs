using System;

namespace Piglet.Lexer.Runtime
{
    internal sealed class TabularLexer<T>
        : LexerBase<T, int>
    {
        private readonly TransitionTable<T> _transitionTable;


        public TabularLexer(TransitionTable<T> transitionTable, int endOfInputTokenNumber)
            : base(endOfInputTokenNumber) => _transitionTable = transitionTable;

        protected override bool ReachedTermination(int nextState) => nextState == -1;

        protected override int GetNextState(int state, char c) => _transitionTable[state, c];

        protected override (int number, Func<string, T>? action)? GetAction(int state) => _transitionTable.GetAction(state);

        protected override int GetInitialState() => 0;
    }
}