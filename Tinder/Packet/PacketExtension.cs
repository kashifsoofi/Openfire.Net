﻿// Copyright (C) 2004-2009 Jive Software. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace org.xmpp.packet
{
	/**
 * A packet extension represents a child element of a Packet for a given qualified name. The
 * PacketExtension acts as a wrapper on a child element the same way Packet does for a whole
 * element. The wrapper provides an easy way to handle the packet extension.<p>
 *
 * Subclasses of this class can be registered using the static variable
 * <tt>registeredExtensions</tt>. The registration process associates the new subclass
 * with a given qualified name (ie. element name and namespace). This information will be used by
 * {@link Packet#getExtension(String, String)} for locating the corresponding PacketExtension
 * subclass to return for the requested qualified name. Each PacketExtension must have a public
 * constructor that takes an Element instance as an argument. 
 *
 * @author Gaston Dombiak
 */
	public class PacketExtension
	{
		protected static readonly XDocument docFactory = new XDocument();
		/**
     * Subclasses of PacketExtension should register the element name and namespace that the
     * subclass is using.
     */
        protected static readonly ConcurrentDictionary<XName, Type> registeredExtensions = new ConcurrentDictionary<XName, Type>();

		protected XElement element;

		/**
     * Returns the extension class to use for the specified element name and namespace. For
     * instance, the DataForm class should be used for the element "x" and
     * namespace "jabber:x:data".
     *
     * @param name the child element name.
     * @param namespace the child element namespace.
     * @return the extension class to use for the specified element name and namespace.
     */
		public static Type GetExtensionClass(String name, String @namespace) {
			return registeredExtensions[XName.Get(name, @namespace)];
		}

		/**
     * Constructs a new Packet extension using the specified name and namespace.
     *
     * @param name the child element name.
     * @param namespace the child element namespace.
     */
		public PacketExtension(String name, String @namespace)
        {
            this.element = new XElement(XName.Get(name, @namespace));
            docFactory.Add(element);
		}

		/**
     * Constructs a new PacketExtension.
     *
     * @param element the XML Element that contains the packet extension contents.
     */
		public PacketExtension(XElement element)
        {
			this.element = element;
		}

		/**
     * Returns the DOM4J Element that backs the packet. The element is the definitive
     * representation of the packet and can be manipulated directly to change
     * packet contents.
     *
     * @return the DOM4J Element that represents the packet.
     */
		public XElement getElement() {
			return element;
		}

		/**
     * Creates a deep copy of this packet extension.
     *
     * @return a deep copy of this packet extension.
     */
		public PacketExtension createCopy() {
			XElement copy = new XElement(element);
			//TODO: docFactory.createDocument().add(copy);
			return new PacketExtension(copy);
		}
	}
}

