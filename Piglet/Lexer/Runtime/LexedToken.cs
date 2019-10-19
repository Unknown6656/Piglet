﻿using System.Linq;

using Piglet.Parser.Configuration;

namespace Piglet.Lexer.Runtime
{
    /// <summary>
    /// Represents a lexed (accepted) token.
    /// </summary>
    public class LexedToken<T>
    {
        /// <summary>
        /// Returns the lexed symbol.
        /// </summary>
        public T SymbolValue { get; }
        /// <summary>
        /// Returns the string associated with the lexed symbol.
        /// </summary>
        public string? LexedString { get; }
        /// <summary>
        /// The token's absolute index in the input string (zero-based).
        /// </summary>
        public int AbsoluteIndex { get; }
        /// <summary>
        /// The token's starting line number (one-based).
        /// </summary>
        public int StartLineNumber { get; }
        /// <summary>
        /// The token's starting index inside the starting line (one-based).
        /// </summary>
        public int StartCharacterIndex { get; }
        /// <summary>
        /// The token's length (in characters).
        /// </summary>
        public int Length { get; }
        /// <summary>
        /// Determines whether the token is a terminal token
        /// </summary>
        public bool IsTerminal { get; }


        internal LexedToken(T value, int abs_index, int line, int char_index, int length, bool terminal)
            : this(value, null, abs_index, line, char_index, terminal) => Length = length;

        internal LexedToken(T value, string? str, int abs_index, int line, int char_index, bool terminal)
        {
            SymbolValue = value;
            LexedString = str;
            Length = str?.Length ?? 0;
            AbsoluteIndex = abs_index;
            StartLineNumber = line;
            StartCharacterIndex = char_index;
            IsTerminal = terminal;
        }

        public override string ToString() => $"[{AbsoluteIndex}..{AbsoluteIndex + Length}] \"{LexedString}\" at ({StartLineNumber}:{StartCharacterIndex})";
    }

    public sealed class LexedNonTerminal<T>
        : LexedToken<T>
    {
        public INonTerminal<T> NonTerminal { get; }
        public LexedToken<T>[] ChildNodes { get; }
        public LexedToken<T> FirstChild => ChildNodes[0];
        public LexedToken<T> LastChild => ChildNodes[^1];


        private LexedNonTerminal(T value, LexedToken<T>[] children, bool dummy)
            : base(value, children[0].AbsoluteIndex, children[0].StartLineNumber, children[0].StartLineNumber, children[^1].AbsoluteIndex - children[0].AbsoluteIndex + children[^1].Length, false)
        {
            ChildNodes = dummy? new LexedToken<T>[0] : children;
        }

        private LexedNonTerminal(T value, LexedToken<T>[] ordered)
            : this(value, ordered is { } arr && arr.Length > 0 ? arr : new[] { new LexedToken<T>(default, 0, 0, 0, 0, true) }, ordered.Length == 0)
        {
        }

        internal LexedNonTerminal(T value, INonTerminal<T> symbol, LexedToken<T>[] children)
            : this(value, children.OrderBy(c => c.AbsoluteIndex).ToArray()) => NonTerminal = symbol;

        public override string ToString() => base.ToString();
    }
}
