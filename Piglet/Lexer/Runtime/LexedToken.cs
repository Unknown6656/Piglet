﻿using System.Collections.Generic;
using System.Linq;

using Piglet.Parser.Configuration;

namespace Piglet.Lexer.Runtime
{
    /// <summary>
    /// Represents an abstract lexed (accepted) token.
    /// </summary>
    public abstract class LexedTokenBase
    {
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
        /// The token's starting index inside the starting line (zero-based).
        /// </summary>
        public int StartCharacterIndex { get; }
        /// <summary>
        /// The token's length (in characters).
        /// </summary>
        public int Length { get; }
        /// <summary>
        /// Determines whether the token is a terminal token.
        /// </summary>
        public virtual bool IsTerminal { get; } = false;
#if DEBUG
        /// <summary>
        /// The debug name of the current token.
        /// </summary>
        public virtual string? DebugName => LexedString;
#endif

        private protected LexedTokenBase(int abs_index, int line, int char_index, int length)
            : this(null, abs_index, line, char_index) => Length = length;

        private protected LexedTokenBase(string? str, int abs_index, int line, int char_index)
        {
            LexedString = str;
            Length = str?.Length ?? 0;
            AbsoluteIndex = abs_index;
            StartLineNumber = line;
            StartCharacterIndex = char_index;
        }
    }

    /// <summary>
    /// Represents a lexed (accepted) token.
    /// </summary>
    /// <typeparam name="T">The semantic value stored inside the lexed symbol.</typeparam>
    public class LexedToken<T>
        : LexedTokenBase
    {
        /// <summary>
        /// Returns the lexed symbol.
        /// </summary>
        public T SymbolValue { get; }


        internal LexedToken(T value, string? str, int abs_index, int line, int char_index)
            : base(str, abs_index, line, char_index) => SymbolValue = value;

        internal LexedToken(T value, int abs_index, int line, int char_index, int length)
            : base(abs_index, line, char_index, length) => SymbolValue = value;

        // public LexedToken<U> Cast<U>() => this is LexedNonTerminal<T> nt ? nt.Cast<U>() : new LexedToken<U>((U)(object)SymbolValue, LexedString, AbsoluteIndex, StartLineNumber, StartCharacterIndex, true);

        /// <inheritdoc/>
        public override string ToString() => $"[{AbsoluteIndex}..{AbsoluteIndex + Length}] \"{LexedString}\" at ({StartLineNumber}:{StartCharacterIndex})";
    }

    /// <summary>
    /// Represents a lexed non-terminal token.
    /// </summary>
    /// <typeparam name="T">The semantic value stored inside the lexed symbol.</typeparam>
    public sealed class LexedNonTerminal<T>
        : LexedToken<T>
    {
        internal INonTerminal NonTerminal { get; }
        /// <summary>
        /// The 
        /// </summary>
        public LexedToken<T>[] ChildNodes { get; }
        public LexedToken<T> FirstChild => ChildNodes[0];
        public LexedToken<T> LastChild => ChildNodes[^1];
        /// <inheritdoc/>
        public override bool IsTerminal => true;
#if DEBUG
        /// <inheritdoc/>
        public override string? DebugName => NonTerminal.DebugName;
#endif

        private LexedNonTerminal(T value, INonTerminal symbol, LexedToken<T>[] ordered_children)
            : base(
                  value,
                  ordered_children[0].AbsoluteIndex,
                  ordered_children[0].StartLineNumber,
                  ordered_children[0].StartCharacterIndex,
                  ordered_children[^1].AbsoluteIndex - ordered_children[0].AbsoluteIndex + ordered_children[^1].Length
            )
        {
            NonTerminal = symbol;
            ChildNodes = ordered_children;
        }

        internal LexedNonTerminal(T value, INonTerminal symbol, IEnumerable<LexedToken<T>> children)
            : this(value, symbol, children.OrderBy(c => c.AbsoluteIndex).ToArray() is { } arr && arr.Length > 0 ? arr : new[] { new LexedToken<T>(default!, 0, 0, 0, 0) }) => NonTerminal = symbol;

        // public new LexedNonTerminal<U> Cast<U>() => new LexedNonTerminal<U>((U)(object)SymbolValue, NonTerminal, ChildNodes as IEnumerable<LexedToken<U>>);

        /// <inheritdoc/>
        public override string ToString() => $"[{AbsoluteIndex}..{AbsoluteIndex + Length}] \"{NonTerminal.DebugName}\" : {SymbolValue}";
    }
}
