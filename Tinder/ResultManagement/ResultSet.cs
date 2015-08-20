// Copyright (C) 2004-2009 Jive Software. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace org.xmpp.resultsetmangement
{
	/// <summary>
	/// A result set representation as described in XEP-0059. A result set is a
	/// collection of objects that each have a unique identifier (UID).
	/// 
	/// It's expected that some implementations will have the complete result set
	/// loaded into memory, whereas more complex implementations might keep
	/// references to partial sets only. This latter would have considerable
	/// advantages if the result set is extremely large, or if the operation to get
	/// all results in the set is expensive.
	/// 
	/// @author Guus der Kinderen, guus.der.kinderen@gmail.com
	/// 
	/// @param <T>
	///            Each result set should be a collection of instances of the exact
	///            same class. This class must implement the {@link Result}
	///            interface.
	/// </summary>
	public abstract class ResultSet<T> : ICollection<T> where T : Result
	{
		/// A list of field names that are valid in jabber:iq:search
		private static readonly List<string> validRequestFields = new List<string>() {
			"max",
			"before",
			"after",
			"index"
		};

		/// The namespace that identifies Result Set Management functionality.
		private static readonly string NAMESPACE_RESULT_SET_MANAGEMENT = "http://jabber.org/protocol/rsm";

		/// <summary>
		/// Returns a List of results starting with the first result after the
		/// provided result (the returned List is exclusive).
		/// 
		/// The lenght of the list is equal to 'maxAmount', unless there are no more
		/// elements available (in which case the length of the result will be
		/// truncated).
		/// </summary>
		/// <param name="result">The element that is right before the first element in the result.</param>
		/// <param name="maxAmount">The maximum number of elements to return.</param>
		/// <returns>A List of elements the are exactly after the element that is provided as a parameter.</returns>
		/// <exception cref="Null">if the result does not exist in the result set.</exception>
		public List<T> getAfter(T result, int maxAmount)
		{
			return getAfter(result.getUID(), maxAmount);
		}

		/// <summary>
		/// Returns a List of results starting with the first result after the result
		/// that's identified by the provided UID (the returned List is exclusive).
		/// 
		/// The lenght of the list is equal to 'maxAmount', unless there are no more
		/// elements available (in which case the length of the result will be
		/// truncated).
		/// </summary>
		/// <param name="uid">The UID of the element that is right before the first element in the result.</param>
		/// <param name="maxAmount">The maximum number of elements to return.</param>
		/// <returns>A List of elements the are exactly after the element that is provided as a parameter.</returns>
		/// <exception cref="Null">if the result does not exist in the result set.</exception>
		public abstract List<T> getAfter(string uid, int maxAmount);

		/**
	 * Returns a list of results ending with the element right before the
	 * provided result (the returned List is exclusive).
	 * 
	 * At most 'maxAmount' elements are in the returned List, but the lenght of
	 * the result might be smaller if no more applicable elements are available.
	 * 
	 * Note that the order of the result is equal to the order of the results of
	 * other methods of this class: the last element in the returned List
	 * immediately preceeds the element denoted by the 'result' parameter.
	 * 
	 * @param result
	 *            The element preceding the last element returned by this
	 *            function.
	 * @param maxAmount
	 *            The length of the List that is being returned.
	 * @return A List of elements that are exactly before the element that's
	 *         provided as a parameter.
	 * @throws NullPointerException
	 *             if the result does not exist in the result set.
	 * 
	 */
		public List<T> getBefore(T result, int maxAmount) {
			return getBefore(result.getUID(), maxAmount);
		}

		/**
	 * Returns a list of results ending with the element right before the
	 * element identified by the provided UID (the returned List is exclusive).
	 * 
	 * At most 'maxAmount' elements are in the returned List, but the lenght of
	 * the result might be smaller if no more applicable elements are available.
	 * 
	 * Note that the order of the result is equal to the order of the results of
	 * other methods of this class: the last element in the returned List
	 * immediately preceeds the element denoted by the 'result' parameter.
	 * 
	 * @param uid
	 *            The UID of the element preceding the last element returned by
	 *            this function.
	 * @param maxAmount
	 *            The length of the List that is being returned.
	 * @return A List of elements that are exactly before the element that's
	 *         provided as a parameter.
	 * @throws NullPointerException
	 *             if there is no result in the result set that matches the UID.
	 */
		public abstract List<T> getBefore(String uid, int maxAmount);

		/**
	 * Returns the first elements from this result set.
	 * 
	 * @param maxAmount
	 *            the number of elements to return.
	 * @return the last 'maxAmount' elements of this result set.
	 */
		public abstract List<T> getFirst(int maxAmount);

		/**
	 * Returns the last elements from this result set.
	 * 
	 * @param maxAmount
	 *            the number of elements to return.
	 * @return the last 'maxAmount' elements of this result set.
	 */
		public abstract List<T> getLast(int maxAmount);

		/**
	 * Returns the element denoted by the index.
	 * 
	 * @param index
	 *            Index of the element to be returned
	 * @return the Element at 'index'.
	 */
		public abstract T get(int index);

		/**
	 * Returns a list of results, starting with the result that's at the
	 * specified index. If the difference between the startIndex and the index
	 * of the last element in the entire resultset is smaller than the size
	 * supplied in the 'amount' parameter, the length of the returned list will
	 * be smaller than the 'amount' paramater. If the supplied index is equal
	 * to, or larger than the size of the result set, an empty List is returned.
	 * 
	 * @param fromIndex
	 *            The index of the first element to be returned.
	 * @param maxAmount
	 *            The maximum number of elements to return.
	 * @return A list of elements starting with (inclusive) the element
	 *         referenced by 'fromIndex'. An empty List if startIndex is equal
	 *         to or bigger than the size of this entire result set.
	 */
		public abstract List<T> get(int fromIndex, int maxAmount);

		/**
	 * Returns the UID of the object at the specified index in this result set.
	 * 
	 * @param index
	 *            The index of the UID to be returned.
	 * @return UID of the object on the specified index.
	 */
		public String getUID(int index) {
			return get(index).getUID();
		}

		/**
	 * Returns the index in the full resultset of the element identified by the
	 * UID in te supplied argument.
	 * 
	 * @param uid
	 *            The UID of the element to search for
	 * @return The index of the element.
	 * @throws NullPointerException
	 *             if there is no result in the result set that matches the UID.
	 * 
	 */
		public abstract int indexOf(String uid);

		/**
	 * Returns the index in the full resultset of the supplied argument.
	 * 
	 * @param element
	 *            The element to search for
	 * @return The index of the element.
	 */
		public int indexOf(T element) {
			return indexOf(element.getUID());
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see java.util.AbstractCollection#iterator()
	 */
		public IEnumerable<T> iterator() {
			return new Itr();
		}

		/**
	 * Applies the 'result set management' directives to this result set, and
	 * returns a list of Results that matches the directives. Note that the
	 * orignal set is untouched. Instead, a new List is returned.
	 * 
	 * @param rsmElement
	 *            The XML element that contains the 'result set management'
	 *            directives.
     * @return a list of Results that matches the directives.
	 */
		public List<T> applyRSMDirectives(XElement rsmElement) {
			if (rsmElement == null || !isValidRSMRequest(rsmElement)) {
				throw new ArgumentException(
					"The 'rsmElement' argument must be a valid, non-null RSM element.");
			}

		    int max = Int32.Parse(rsmElement.Element("max").Value);

			if (max == 0) {
				// this is a request for a resultset count.
				return Collections.emptyList();
			}

			// optional elements
			XElement afterElement = rsmElement.Element("after");
			XElement beforeElement = rsmElement.Element("before");
			XElement indexElement = rsmElement.Element("index");

			// Identify the pointer object in this set. This is the object before
			// (or after) the first (respectivly last) element of the subset that
			// should be returned. If no pointer is specified, the pointer is said
			// to be before or after the first respectivly last element of the set.
			String pointerUID = null; // by default, the pointer is before the
			// first element of the set.

			// by default, the search list is forward oriented.
			bool isForwardOriented = true;

			if (afterElement != null) {
				pointerUID = afterElement.Value;
			} else if (beforeElement != null) {
				pointerUID = beforeElement.Value;
				isForwardOriented = false;
			} else if (indexElement != null) {
				int index = Int32.Parse(indexElement.Value);
				if (index > 0) {
					pointerUID = getUID(index - 1);
				}
			}

		    if (string.IsNullOrEmpty(pointerUID))
		    {
		        pointerUID = null;
		    }

		    if (isForwardOriented) {
				if (pointerUID == null) {
					return getFirst(max);
				}
				return getAfter(pointerUID, max);
			}

			if (pointerUID == null) {
				return getLast(max);
			}
			return getBefore(pointerUID, max);
		}

		/**
	 * Generates a Result Set Management 'set' element that describes the parto
	 * of the result set that was generated. You typically would use the List
	 * that was returned by {@link #applyRSMDirectives(Element)} as an argument
	 * to this method.
	 * 
	 * @param returnedResults
	 *            The subset of Results that is returned by the current query.
	 * @return An Element named 'set' that can be included in the result IQ
	 *         stanza, which returns the subset of results.
	 */
		public XElement generateSetElementFromResults(List<T> returnedResults) {
			if (returnedResults == null) {
				throw new ArgumentException(
					"Argument 'returnedResults' cannot be null.");
			}
			XElement setElement = DocumentHelper.createElement(XName.Get(
				"set", ResultSet.NAMESPACE_RESULT_SET_MANAGEMENT));
			// the size element contains the size of this entire result set.
			setElement.addElement("count").setText(String.valueOf(size()));

			// if the query wasn't a 'count only' query, add two more elements
			if (returnedResults.size() > 0) {
				XElement firstElement = setElement.addElement("first");
				firstElement.addText(returnedResults.get(0).getUID());
				firstElement.addAttribute("index", String
				                      .valueOf(indexOf(returnedResults.get(0))));

				setElement.addElement("last").addText(
					returnedResults.get(returnedResults.size() - 1).getUID());
			}

			return setElement;
		}

		/**
	 * Checks if the Element that has been passed as an argument is a valid
	 * Result Set Management element, in a request context.
	 * 
	 * @param rsmElement
	 *            The Element to check.
	 * @return ''true'' if this is a valid RSM query representation, ''false''
	 *         otherwise.
	 */
		// Dom4J doesn't do generics, sadly.
		public static bool isValidRSMRequest(XElement rsmElement) {
			if (rsmElement == null) {
				throw new ArgumentException(
					"The argument 'rsmElement' cannot be null.");
			}

			if (!rsmElement.Name.LocalName.Equals("set")) {
				// the name of the element must be "set".
				return false;
			}

			if (!rsmElement.Name.Namespace.NamespaceName.Equals(
				NAMESPACE_RESULT_SET_MANAGEMENT)) {
				// incorrect namespace
				return false;
			}

			XElement maxElement = rsmElement.Element("max");
			if (maxElement == null) {
				// The 'max' element in an RSM request must be available
				return false;
			}

			string sMax = maxElement.Value;
			if (string.IsNullOrEmpty(sMax)) {
				// max element must contain a value.
				return false;
			}

			try {
				if (Int32.Parse(sMax) < 0) {
					// must be a postive integer.
					return false;
				}
			} catch (FormatException e) {
				// the value of 'max' must be an integer value.
				return false;
			}

		    List<XElement> allElements = rsmElement.Elements().ToList();
			int optionalElements = 0;
			foreach (XElement element in allElements)
			{
			    string name = element.Name.LocalName;
				if (!validRequestFields.Contains(name)) {
					// invalid element.
					return false;
				}

				if (!name.Equals("max")) {
					optionalElements++;
				}

				if (optionalElements > 1) {
					// only one optional element is allowed.
					return false;
				}

				if (name.Equals("index"))
				{
				    string value = element.Value;
					if (string.IsNullOrEmpty(value)) {
						// index elements must have a numberic value.
						return false;
					}
					try {
						if (Int32.Parse(value) < 0) {
							// index values must be positive.
							return false;
						}
					} catch (FormatException e) {
						// index values must be numeric.
						return false;
					}
				}
			}

			return true;
		}
	}
}

