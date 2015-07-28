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

namespace org.xmpp.resultsetmangement
{
	/// <summary>
	/// A result set representation as described in XEP-0059. Note that this result
	/// 'set' actually makes use of a List implementations, as the Java Set
	/// definition disallows duplicate elements, while the List definition supplies
	/// most of the required indexing operations.
	/// 
	/// This ResultSet implementation loads all all results from the set into memory,
	/// which might be undesirable for very large sets, or for sets where the
	/// retrieval of a result is an expensive operation. sets.
	/// 
	/// As most methods are backed by the {@link List#subList(int, int)} method,
	/// non-structural changes in the returned lists are reflected in the ResultSet,
	/// and vice-versa.
	/// 
	/// @author Guus der Kinderen, guus.der.kinderen@gmail.com
	/// 
	/// @param <T>
	///            Each result set should be a collection of instances of the exact
	///            same class. This class must implement the {@link Result}
	///            interface.
	/// @see java.util.List#subList(int, int)
	/// </summary>
	/// TODO: do we want changes to the returned Lists of methods in this class be
	/// applied to the content of the ResultSet itself? Currently, because of the
	/// usage of java.util.List#subList(int, int), it does. I'm thinking a
	/// immodifiable solution would cause less problems. -Guus
	public class ResultSetImpl<T> : ResultSet<T> where T : Result
	{
		/**
	 * A list of all results in this ResultSet
	 */
		public List<T> resultList;

		/**
	 * A mapping of the UIDs of all results in resultList, to the index of those
	 * entries in that list.
	 */
		public Dictionary<string, int> uidToIndex;

		/**
	 * Creates a new Result Set instance, based on a collection of Result
	 * implementing objects. The collection should contain elements of the exact
	 * same class only, and cannot contain 'null' elements.
	 * 
	 * The order that's being used in the new ResultSet instance is the same
	 * order in which {@link Collection#iterator()} iterates over the
	 * collection.
	 * 
	 * Note that this constructor throws an IllegalArgumentException if the
	 * Collection that is provided contains Results that have duplicate UIDs.
	 * 
	 * @param results
	 *            The collection of Results that make up this result set.
	 */
		public ResultSetImpl(Collection<T> results) {
			this(results, null);
		}

		/**
	 * Creates a new Result Set instance, based on a collection of Result
	 * implementing objects. The collection should contain elements of the exact
	 * same class only, and cannot contain 'null' elements.
	 * 
	 * The order that's being used in the new ResultSet instance is defined by
	 * the supplied Comparator class.
	 * 
	 * Note that this constructor throws an IllegalArgumentException if the
	 * Collection that is provided contains Results that have duplicate UIDs.
	 * 
	 * @param results
	 *            The collection of Results that make up this result set.
	 * @param comparator
	 *            The Comparator that defines the order of the Results in this
	 *            result set.
	 */
		public ResultSetImpl(Collection<T> results, Comparator<T> comparator) {
			if (results == null) {
				throw new NullReferenceException("Argument 'results' cannot be null.");
			}

			int size = results. size();
			resultList = new List<T>(size);
			uidToIndex = new Dictionary<string, int>(size);

			// sort the collection, if need be.
			List<T> sortedResults = null;
			if (comparator != null) {
				sortedResults = new ArrayList<T>(results);
				Collections.sort(sortedResults, comparator);
			}

			int index = 0;
			// iterate over either the sorted or unsorted collection
			for (E result : (sortedResults != null ? sortedResults : results)) {
				if (result == null) {
					throw new NullPointerException(
						"The result set must not contain 'null' elements.");
				}

				final String uid = result.getUID();
				if (uidToIndex.containsKey(uid)) {
					throw new IllegalArgumentException(
						"The result set can not contain elements that have the same UID.");
				}

				resultList.add(result);
				uidToIndex.put(uid, index);
				index++;
			}
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see com.buzzaa.xmpp.resultsetmanager.ResultSet#size()
	 */
		public int size() {
			return resultList.Count;
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see com.buzzaa.xmpp.resultsetmanager.ResultSet#getAfter(E, int)
	 */
		public List<T> getAfter(String uid, int maxAmount) {
			if (string.IsNullOrEmpty(uid)) {
                throw new NullReferenceException("Argument 'uid' cannot be null or an empty String.");
			}

			if (maxAmount < 1) {
				throw new ArgumentException(
					"Argument 'maxAmount' must be a integer higher than zero.");
			}

			// the result of this method is exclusive 'result'
			int index = uidToIndex[uid] + 1;

			return get(index, maxAmount);
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see com.buzzaa.xmpp.resultsetmanager.ResultSet#getBefore(E, int)
	 */
		public override List<T> getBefore(String uid, int maxAmount) {
			if (string.IsNullOrEmpty(uid)) {
				throw new NullReferenceException("Argument 'uid' cannot be null or an empty String.");
			}

			if (maxAmount < 1) {
				throw new ArgumentException(
					"Argument 'maxAmount' must be a integer higher than zero.");
			}

			// the result of this method is exclusive 'result'
			int indexOfLastElement = uidToIndex[uid];
			int indexOfFirstElement = indexOfLastElement - maxAmount;

			if (indexOfFirstElement < 0) {
				return get(0, indexOfLastElement);
			}

			return get(indexOfFirstElement, maxAmount);
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see com.buzzaa.xmpp.resultsetmanager.ResultSet#get(int)
	 */
		public override T get(int index) {
			return resultList[index];
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see com.buzzaa.xmpp.resultsetmanager.ResultSet#getFirst(int)
	 */
		public override List<T> getFirst(int maxAmount) {
			if (maxAmount < 1) {
				throw new ArgumentException(
					"Argument 'maxAmount' must be a integer higher than zero.");
			}

			return get(0, maxAmount);
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see com.buzzaa.xmpp.resultsetmanager.ResultSet#getLast(int)
	 */
		public override List<T> getLast(int maxAmount) {
			if (maxAmount < 1) {
				throw new ArgumentException(
					"Argument 'maxAmount' must be a integer higher than zero.");
			}

			int indexOfFirstElement = size() - maxAmount;

			if (indexOfFirstElement < 0) {
				return get(0, maxAmount);
			}

			return get(indexOfFirstElement, maxAmount);
		}

		/*
	 * (non-Javadoc)
	 * 
	 * @see com.buzzaa.xmpp.resultsetmanager.ResultSet#get(int, int)
	 */
		public override List<T> get(int fromIndex, int maxAmount) {
			if (fromIndex < 0) {
				throw new ArgumentException(
					"Argument 'fromIndex' must be zero or higher.");
			}

			if (maxAmount < 1) {
				throw new ArgumentException(
					"Argument 'maxAmount' must be a integer higher than zero.");
			}

			if (fromIndex >= size()) {
				return new List<T>(0);
			}

			// calculate the last index to return, or return up to the end of last
			// index if 'amount' surpasses the list length.
			int absoluteTo = fromIndex + maxAmount;
			int toIndex = (absoluteTo > size() ? size() : absoluteTo);

			return resultList.GetRange(fromIndex, toIndex - fromIndex);
		}

		/*
	 * (non-Javadoc)
	 * @see org.jivesoftware.util.resultsetmanager.ResultSet#indexOf(java.lang.String)
	 */
		public int indexOf(String uid) {
			return uidToIndex[uid];
		}
	}
}

