/**
 * Copyright (C) 2004-2009 Jive Software. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;

namespace org.xmpp.util
{
	/// <summary>
	/// A wrapper implementation for cached values, suitable for {@link Map} based
	/// caches where a significant portion of keys matches the corresponding value
	/// exactly.
	/// </summary>
	public class ValueWrapper<T>
	{
		private const long serialVersionUID = -9054807517127343156L;

		/// <summary>
		/// Indication of how the key of this cache entry represents the cache value.
		/// </summary>
		public enum RepresentationType
		{
			/// <summary>
			/// The key that maps to this {@link ValueWrapper} instance cannot be
			/// used to generate a valid value.
			/// </summary>
			ILLEGAL,

			/// <summary>
			/// The generated value based on the key that maps to this
			/// {@link ValueWrapper} would be an exact duplicate of the key. To
			/// conserve memory, this wrapped value instance will not have a value
			/// set. Use the key that points to this wrapper instead.
			/// </summary>
			USE_KEY,

			/// <summary>
			/// The key that maps to this {@link ValueWrapper} can be used to
			/// generate a valid value. The generated value is wrapped in this
			/// {@link ValueWrapper} instance.
			/// </summary>
			USE_VALUE
		}

		/// <summary>
		/// The value that is wrapped.
		/// </summary>
		private readonly T value;

		/// <summary>
		/// Indicates how the key that maps to this value can be used to extract the
		/// value from the cache entry.
		/// </summary>
		private readonly RepresentationType representation;

		/// <summary>
		/// Constructs an empty wrapper. This wrapper either is used to indicate that
		/// the key that maps to this value: cannot be used to generate a valid
		/// value, or, is an exact duplicate of the generated value.
		/// </summary>
		/// <param name="representation">Key representation indicator.</param>
		/// <exception cref="">An {@link IllegalArgumentException} is thrown if the argument is
		/// <tt>USE_VALUE</tt>.</exception>
		public ValueWrapper(RepresentationType representation)
		{
			if (representation == RepresentationType.USE_VALUE)
				throw new ArgumentException();

			this.representation = representation;
			this.value = default(T);
		}

		/// <summary>
		/// Wraps a value while using the <tt>USE_VALUE</tt> representation.
		/// </summary>
		/// <param name="value">The value that is wrapped.</param>
		public ValueWrapper(T value)
		{
			this.representation = RepresentationType.USE_VALUE;
			this.value = value;
		}

		/// <summary>
		/// Returns the wrapped value, or <tt>null</tt> if the representation used in
		/// this instance is not USE_VALUE;
		/// </summary>
		/// <returns>the wrapped value</returns>
		public T Value {
			get { return value; }
		}

		public RepresentationType Representation {
			get { return representation; }
		}
	}
}

