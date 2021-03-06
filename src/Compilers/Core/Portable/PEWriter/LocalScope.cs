﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Cci
{
    /// <summary>
    /// A range of CLR IL operations that comprise a lexical scope, specified as an IL offset and a length.
    /// </summary>
    internal struct LocalScope
    {
        private readonly uint _offset;
        private readonly uint _length;
        private readonly ImmutableArray<ILocalDefinition> _constants;
        private readonly ImmutableArray<ILocalDefinition> _locals;

        internal LocalScope(uint offset, uint length, ImmutableArray<ILocalDefinition> constants, ImmutableArray<ILocalDefinition> locals)
        {
            // We should not create 0-length scopes as they are useless.
            // however we will allow the case of "begin == end" as that is how edge inclusive scopes of length 1 are represented.

            Debug.Assert(!locals.Any(l => l.Name == null));
            Debug.Assert(!constants.Any(c => c.Name == null));

            _offset = offset;
            _length = length;
            _constants = constants;
            _locals = locals;
        }

        /// <summary>
        /// The offset of the first operation in the scope.
        /// </summary>
        public uint Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// The length of the scope. Offset+Length equals the offset of the first operation outside the scope, or equals the method body length.
        /// </summary>
        public uint Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Returns zero or more local constant definitions that are local to the given scope.
        /// </summary>
        public ImmutableArray<ILocalDefinition> Constants
        {
            get { return _constants.IsDefault ? ImmutableArray<ILocalDefinition>.Empty : _constants; }
        }

        /// <summary>
        /// Returns zero or more local variable definitions that are local to the given scope.
        /// </summary>
        public ImmutableArray<ILocalDefinition> Variables
        {
            get { return _locals.IsDefault ? ImmutableArray<ILocalDefinition>.Empty : _locals; }
        }
    }
}
