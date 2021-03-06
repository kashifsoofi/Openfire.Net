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

using System.IO;
using System.Linq;
using System.Reflection;
using NLog;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace org.xmpp.packet
{
	/**
 * An XMPP packet (also referred to as a stanza). Each packet is backed by a
 * DOM4J Element. A set of convenience methods allows easy manipulation of
 * the Element, or the Element can be accessed directly and manipulated.<p>
 *
 * There are three core packet types:<ul>
 *      <li>{@link Message} -- used to send data between users.
 *      <li>{@link Presence} -- contains user presence information or is used
 *          to manage presence subscriptions.
 *      <li>{@link IQ} -- exchange information and perform queries using a
 *          request/response protocol.
 * </ul>
 *
 * @author Matt Tucker
 */
	public abstract class Packet
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		protected static readonly XDocument docFactory = new XDocument();

		protected XElement element;

		// Cache to and from JIDs
		protected JID toJID;
		protected JID fromJID;

		/**
     * Constructs a new Packet. The TO address contained in the XML Element will only be
     * validated. In other words, stringprep operations will only be performed on the TO JID to
     * verify that it is well-formed. The FROM address is assigned by the server so there is no
     * need to verify it.
     *
     * @param element the XML Element that contains the packet contents.
     */
		public Packet(XElement element)
            : this(element, false)
        { }

		/**
     * Constructs a new Packet. The JID address contained in the XML Element may not be
     * validated. When validation can be skipped then stringprep operations will not be performed
     * on the JIDs to verify that addresses are well-formed. However, when validation cannot be
     * skipped then <tt>only</tt> the TO address will be verified. The FROM address is assigned by
     * the server so there is no need to verify it. 
     *
     * @param element the XML Element that contains the packet contents.
     * @param skipValidation true if stringprep should not be applied to the TO address.
     */
		public Packet(XElement element, bool skipValidation) {
			this.element = element;
			// Apply stringprep profiles to the "to" and "from" values.
			string to = element.Attribute("to").Value;
			if (to != null) {
				if (to.Length == 0) {
					// Remove empty TO values
					element.SetAttributeValue("to", null);
				}
				else {
					string[] parts = JID.getParts(to);
					toJID = new JID(parts[0], parts[1], parts[2], skipValidation);
					element.SetAttributeValue("to", toJID.ToString());
				}
			}
			string from = element.Attribute("from").Value;
			if (from != null) {
				if (from.Length == 0) {
					// Remove empty FROM values
					element.SetAttributeValue("from", null);
				}
				else {
					string[] parts = JID.getParts(from);
					fromJID = new JID(parts[0], parts[1], parts[2], true);
					element.SetAttributeValue("from", fromJID.ToString());
				}
			}
		}

		/**
     * Constructs a new Packet with no element data. This method is used by
     * extensions of this class that require a more optimized path for creating
     * new packets.
     */
		protected Packet()
        { }

		/**
     * Returns the packet ID, or <tt>null</tt> if the packet does not have an ID.
     * Packet ID's are optional, except for IQ packets.
     *
     * @return the packet ID.
     */
		public string getID() {
			return element.Attribute("id").Value;
		}

		/**
     * Sets the packet ID. Packet ID's are optional, except for IQ packets.
     *
     * @param ID the packet ID.
     */
		public void setID(String ID) {
            element.Add(new XAttribute("id", ID));
		}

		/**
     * Returns the XMPP address (JID) that the packet is addressed to, or <tt>null</tt>
     * if the "to" attribute is not set. The XMPP protocol often makes the "to"
     * attribute optional, so it does not always need to be set.
     *
     * @return the XMPP address (JID) that the packet is addressed to, or <tt>null</tt>
     *      if not set.
     */
		public JID getTo() {
			string to = element.Attribute("to").Value;
			if (to == null || to.Length == 0) {
				return null;
			}
			else {
				if (toJID != null && to.Equals(toJID.ToString())) {
					return toJID;
				}
				else {
					// Return a new JID that bypasses stringprep profile checking.
					// This improves speed and is safe as long as the user doesn't
					// directly manipulate the attributes of the underlying Element
					// that represent JID's.
					String[] parts = JID.getParts(to);
					toJID = new JID(parts[0], parts[1], parts[2], true);
					return toJID;
				}
			}
		}

		/**
     * Sets the XMPP address (JID) that the packet is addressed to. The XMPP protocol
     * often makes the "to" attribute optional, so it does not always need to be set.
     *
     * @param to the XMPP address (JID) that the packet is addressed to.
     */
		public void setTo(String to) {
			// Apply stringprep profiles to value.
			if (to !=  null) {
				toJID = new JID(to);
				to = toJID.ToString();
			} else {
				toJID = null;
			}
			element.Add(new XAttribute("to", to));
		}

		/**
     * Sets the XMPP address (JID) that the packet is address to. The XMPP protocol
     * often makes the "to" attribute optional, so it does not always need to be set.
     *
     * @param to the XMPP address (JID) that the packet is addressed to.
     */
		public void setTo(JID to) {
			toJID = to;
			if (to == null) {
                element.Add(new XAttribute("to", null));
			}
			else {
				element.Add(new XAttribute("to", to.ToString()));
			}
		}

		/**
     * Returns the XMPP address (JID) that the packet is from, or <tt>null</tt>
     * if the "from" attribute is not set. The XMPP protocol often makes the "from"
     * attribute optional, so it does not always need to be set.
     *
     * @return the XMPP address that the packet is from, or <tt>null</tt>
     *      if not set.
     */
		public JID getFrom() {
			String from = element.Attribute("from").Value;
			if (from == null || from.Length == 0) {
				return null;
			}
			else {
				if (fromJID != null && from.Equals(fromJID.ToString())) {
					return fromJID;
				}
				else {
					// Return a new JID that bypasses stringprep profile checking.
					// This improves speed and is safe as long as the user doesn't
					// directly manipulate the attributes of the underlying Element
					// that represent JID's.
					String[] parts = JID.getParts(from);
					fromJID = new JID(parts[0], parts[1], parts[2], true);
					return fromJID;
				}
			}
		}

		/**
     * Sets the XMPP address (JID) that the packet comes from. The XMPP protocol
     * often makes the "from" attribute optional, so it does not always need to be set.
     *
     * @param from the XMPP address (JID) that the packet comes from.
     */
		public void setFrom(string from) {
			// Apply stringprep profiles to value.
			if (from != null) {
				fromJID = new JID(from);
				from = fromJID.ToString();
			} else {
				fromJID = null;
			}
			element.Add(new XAttribute("from", from));
		}

		/**
     * Sets the XMPP address (JID) that the packet comes from. The XMPP protocol
     * often makes the "from" attribute optional, so it does not always need to be set.
     *
     * @param from the XMPP address (JID) that the packet comes from.
     */
		public void setFrom(JID from) {
			fromJID = from;
			if (from == null) {
				element.Add(new XAttribute("from", null));
			}
			else {
				element.Add(new XAttribute("from", from.ToString()));
			}
		}

		/**
     * Adds the element contained in the PacketExtension to the element of this packet.
     * It is important that this is the first and last time the element contained in
     * PacketExtension is added to another Packet. Otherwise, a runtime error will be
     * thrown when trying to add the PacketExtension's element to the Packet's element.
     * Future modifications to the PacketExtension will be reflected in this Packet.
     *
     * @param extension the PacketExtension whose element will be added to this Packet's element.
     */
		public void addExtension(PacketExtension extension) {
			element.Add(extension.getElement());
		}

		/**
     * Returns a {@link PacketExtension} on the first element found in this packet
     * for the specified <tt>name</tt> and <tt>namespace</tt> or <tt>null</tt> if
     * none was found.
     *
     * @param name the child element name.
     * @param namespace the child element namespace.
     * @return a PacketExtension on the first element found in this packet for the specified
     *         name and namespace or null if none was found.
     */
		public PacketExtension getExtension(string name, string @namespace) {
			List<XElement> extensions = element.Elements(XName.Get(name, @namespace)).ToList();
			if (extensions.Any()) {
				//Class<? extends PacketExtension> extensionClass = PacketExtension.GetExtensionClass(name, @namespace);
                Type extensionClass = PacketExtension.GetExtensionClass(name, @namespace);
				// If a specific PacketExtension implementation has been registered, use that.
				if (extensionClass != null) {
					try
					{
					    ConstructorInfo constructor = extensionClass.GetConstructors()
					        .First(
					            x => x.GetParameters().Count() == 1 && x.GetParameters().All(a => a.ParameterType == typeof (XElement)));
					    return constructor.Invoke(new [] { extensions[0] }) as PacketExtension;
					}
					catch (Exception e) {
						log.Warn("Packet extension (name "+name+", namespace "+@namespace+") cannot be found.", e);
					}
				}
				// Otherwise, use a normal PacketExtension.
				else {
					return new PacketExtension(extensions[0]);
				}
			}
			return null;
		}

		/**
     * Deletes the first element whose element name and namespace matches the specified
     * element name and namespace.<p>
     *
     * Notice that this method may remove any child element that matches the specified
     * element name and namespace even if that element was not added to the Packet using a
     * {@link PacketExtension}.
     *
     *
     * @param name the child element name.
     * @param namespace the child element namespace.
     * @return true if a child element was removed.
     */

	    public bool deleteExtension(string name, string @namespace)
	    {
	        List<XElement> extensions = element.Elements(XName.Get(name, @namespace)).ToList();
	        if (extensions.Any())
	        {
	            foreach (var extension in extensions)
	            {
	                extension.Remove();
	            }
	            return true;
	        }
	        return false;
	    }

	    /**
     * Returns the packet error, or <tt>null</tt> if there is no packet error.
     *
     * @return the packet error.
     */
		public PacketError GetError() {
			XElement error = element.Element("error");
			if (error != null) {
				return new PacketError(error);
			}
			return null;
		}

		/**
     * Sets the packet error. Calling this method will automatically set
     * the packet "type" attribute to "error".
     *
     * @param error the packet error.
     */
		public void SetError(PacketError error) {
			if (element == null) {
				throw new NullReferenceException("Error cannot be null");
			}
			// Force the packet type to "error".
			element.Add(new XAttribute("type", "error"));
			// Remove an existing error packet.
			if (element.Element("error") != null)
			{
			    element.Element("error").Remove();
			}
			// Add the error element.
			element.Add(error.GetElement());
		}

		/**
     * Sets the packet error using the specified condition. Calling this
     * method will automatically set the packet "type" attribute to "error".
     * This is a convenience method equivalent to calling:
     *
     * <tt>setError(new PacketError(condition));</tt>
     *
     * @param condition the error condition.
     */
		public void SetError(PacketError.Condition condition) {
			SetError(new PacketError(condition));
		}

		/**
     * Creates a deep copy of this packet.
     *
     * @return a deep copy of this packet.
     */
		public abstract Packet CreateCopy();

		/**
     * Returns the DOM4J Element that backs the packet. The element is the definitive
     * representation of the packet and can be manipulated directly to change
     * packet contents.
     *
     * @return the DOM4J Element that represents the packet.
     */
		public XElement GetElement() {
			return element;
		}

		/**
     * Returns the textual XML representation of this packet.
     *
     * @return the textual XML representation of this packet.
     */
		public string toXML() {
			return element.asXML();
		}

		public string ToString() {
			StringWriter @out = new StringWriter();
			XMLWriter writer = new XMLWriter(@out, OutputFormat.createPrettyPrint());
			try {
				writer.write(element);
			}
			catch (Exception e) {
				// Ignore.
			}
			return @out.ToString();
		}
	}
}

