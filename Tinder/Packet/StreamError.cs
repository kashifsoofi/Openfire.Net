// Copyright (C) 2004-2009 Jive Software. All rights reserved.
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
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;

namespace org.xmpp.packet
{
	/**
 * A stream error. Stream errors have a condition and they
 * can optionally include explanation text.
 *
 * @author Matt Tucker
 */
	public class StreamError
	{
		private static readonly string ERROR_NAMESPACE = "urn:ietf:params:xml:ns:xmpp-streams";

		private static XDocument docFactory = new XDocument();

		private XElement element;

		/**
     * Construcs a new StreamError with the specified condition.
     *
     * @param condition the error condition.
     */
		public StreamError(Condition condition) {
			this.element = docFactory.createElement(docFactory.createQName("error", "stream",
			                                                           "http://etherx.jabber.org/streams"));
		    //new XNamespace()
			setCondition(condition);
		}

		/**
     * Constructs a new StreamError with the specified condition and error text.
     *
     * @param condition the error condition.
     * @param text the text description of the error.
     */
		public StreamError(Condition condition, String text) {
			this.element = docFactory.createElement(docFactory.createQName("error", "stream",
			                                                           "http://etherx.jabber.org/streams"));
			setCondition(condition);
			setText(text, null);
		}

		/**
     * Constructs a new StreamError with the specified condition and error text.
     *
     * @param condition the error condition.
     * @param text the text description of the error.
     * @param language the language code of the error description (e.g. "en").
     */
		public StreamError(Condition condition, String text, String language) {
			this.element = docFactory.createElement(docFactory.createQName("error", "stream",
			                                                           "http://etherx.jabber.org/streams"));
			setCondition(condition);
			setText(text, language);
		}

		/**
     * Constructs a new StreamError using an existing Element. This is useful
     * for parsing incoming error Elements into StreamError objects.
     *
     * @param element the stream error Element.
     */
		public StreamError(XElement element) {
			this.element = element;
		}

		/**
     * Returns the error condition.
     *
     * @return the error condition.
     * @see Condition
     */
		public Condition GetCondition() {
			for (Iterator<XElement> i=element.elementIterator(); i.hasNext(); ) {
				XElement el = i.next();
				if (el.getNamespaceURI().equals(ERROR_NAMESPACE) &&
				!el.getName().equals("text"))
				{
					return Condition.fromXMPP(el.getName());
				}
			}
			return null;
		}

		/**
     * Sets the error condition.
     *
     * @param condition the error condition.
     * @see Condition
     */
		public void setCondition(Condition condition) {
			if (condition == null) {
				throw new ArgumentException("Condition cannot be null");
			}
			XElement conditionElement = null;
			for (Iterator<Element> i=element.elementIterator(); i.hasNext(); ) {
				XElement el = i.next();
				if (el.getNamespaceURI().equals(ERROR_NAMESPACE) &&
				!el.getName().equals("text"))
				{
					conditionElement = el;
				}
			}
			if (conditionElement != null) {
				conditionElement.Remove();
			}

			conditionElement = docFactory.createElement(condition.toXMPP(), ERROR_NAMESPACE);
			element.Add(conditionElement);
		}

		/**
     * Returns a text description of the error, or <tt>null</tt> if there
     * is no text description.
     *
     * @return the text description of the error.
     */
		public String GetText()
		{
		    return element.Element("text").Value;
		}

		/**
     * Sets the text description of the error.
     *
     * @param text the text description of the error.
     */
		public void setText(String text) {
			setText(text, null);
		}

		/**
     * Sets the text description of the error. Optionally, a language code
     * can be specified to indicate the language of the description.
     *
     * @param text the text description of the error.
     * @param language the language code of the description, or <tt>null</tt> to specify
     *      no language code.
     */
		public void setText(String text, String language) {
			XElement textElement = element.Element("text");
			// If text is null, clear the text.
			if (text == null) {
				if (textElement != null) {
					textElement.Remove();
				}
				return;
			}

			if (textElement == null) {
				textElement = new XElement(XName.Get("text", ERROR_NAMESPACE), text);
				if (language != null) {
					textElement.Add(new XAttribute(XName.Get("lang", "xml",
					                               "http://www.w3.org/XML/1998/namespace"), language));
				}
				element.Add(textElement);
			}
		}

		/**
     * Returns the text description's language code, or <tt>null</tt> if there
     * is no language code associated with the description text.
     *
     * @return the language code of the text description, if it exists.
     */
		public String getTextLanguage() {
			XElement textElement = element.Element("text");
			if (textElement != null)
			{
			    return textElement.Attribute(XName.Get("lang", "xml",
			        "http://www.w3.org/XML/1998/namespace")).Value;
			}
			return null;
		}

		/**
     * Returns the DOM4J Element that backs the error. The element is the definitive
     * representation of the error and can be manipulated directly to change
     * error contents.
     *
     * @return the DOM4J Element.
     */
		public XElement getElement() {
			return element;
		}

		/**
     * Returns the textual XML representation of this stream error.
     *
     * @return the textual XML representation of this stream error.
     */
		public String toXML() {
			return element.asXML();
		}

		public String toString() {
			StringWriter @out = new StringWriter();
			XMLWriter writer = new XMLWriter(@out, OutputFormat.createPrettyPrint());
			try {
				writer.write(element);
			}
			catch (Exception e) { e.printStackTrace(); }
			return @out.ToString();
		}

		/**
     * Type-safe enumeration for the error condition.<p>
     *
     * Implementation note: XMPP error conditions use "-" characters in
     * their names such as "bad-request". Because "-" characters are not valid
     * identifier parts in Java, they have been converted to "_" characters in
     * the  enumeration names, such as <tt>bad_request</tt>. The {@link #toXMPP()} and
     * {@link #fromXMPP(String)} methods can be used to convert between the
     * enumertation values and XMPP error code strings.
     */
		public enum Condition {

			/**
         * The entity has sent XML that cannot be processed; this error MAY be used
         * instead of the more specific XML-related errors, such as &lt;bad-namespace-prefix/&gt;,
         * &lt;invalid-xml/&gt;, &lt;restricted-xml/&gt;, &lt;unsupported-encoding/&gt;, and
         * &lt;xml-not-well-formed/&gt;, although the more specific errors are preferred.
         */
            [Description("bad-format")]
			bad_format,

			/**
         * The entity has sent a namespace prefix that is unsupported, or has sent no
         * namespace prefix on an element that requires such a prefix.
         */
            [Description("bad-namespace-prefix")]
			bad_namespace_prefix,

			/**
         * The server is closing the active stream for this entity because a new stream
         * has been initiated that conflicts with the existing stream.
         */
        [Description("conflict")]
			conflict,

			/**
         * The entity has not generated any traffic over the stream for some period of
         * time (configurable according to a local service policy).
         */
        [Description("connection-timeout")]
			connection_timeout,

			/**
         * The value of the 'to' attribute provided by the initiating entity in the
         * stream header corresponds to a hostname that is no longer hosted by the server.
         */
        [Description("host-gone")]
			host_gone,

			/**
         * The value of the 'to' attribute provided by the initiating entity in the
         * stream header does not correspond to a hostname that is hosted by the server.
         */
        [Description("host-unknown")]
			host_unknown,

			/**
         * A stanza sent between two servers lacks a 'to' or 'from' attribute
         * (or the attribute has no value).
         */
        [Description("improper-addressing")]
			improper_addressing,

			/**
         * The server has experienced a misconfiguration or an otherwise-undefined
         * internal error that prevents it from servicing the stream.
         */
        [Description("internal-server-error")]
			internal_server_error,

			/**
         * The JID or hostname provided in a 'from' address does not match an authorized
         * JID or validated domain negotiated between servers via SASL or dialback, or
         * between a client and a server via authentication and resource binding.
         */
        [Description("invalid-from")]
			invalid_from,

			/**
         * The stream ID or dialback ID is invalid or does not match an ID previously provided.
         */
        [Description("invalid-id")]
			invalid_id,

			/**
         * the streams namespace name is something other than "http://etherx.jabber.org/streams"
         * or the dialback namespace name is something other than "jabber:server:dialback".
         */
        [Description("invalid-namespace")]
			invalid_namespace,

			/**
         * The entity has sent invalid XML over the stream to a server that performs validation.
         */
        [Description("invalid-xml")]
			invalid_xml,

			/**
         * The entity has attempted to send data before the stream has been authenticated,
         * or otherwise is not authorized to perform an action related to stream
         * negotiation; the receiving entity MUST NOT process the offending stanza before
         * sending the stream error.
         */
        [Description("not-authorized")]
			not_authorized,

			/**
         * The entity has violated some local service policy; the server MAY choose to
         * specify the policy in the <text/> element or an application-specific condition
         * element.
         */
        [Description("policy-violation")]
			policy_violation,

			/**
         * The server is unable to properly connect to a remote entity that is required for
         * authentication or authorization.
         */
        [Description("remote-connection-failed")]
			remote_connection_failed,

			/**
         * The server lacks the system resources necessary to service the stream.
         */
        [Description("resource-constraint")]
			resource_constraint,

			/**
         * The entity has attempted to send restricted XML features such as a comment,
         * processing instruction, DTD, entity reference, or unescaped character.
         */
        [Description("restricted-xml")]
			restricted_xml,

			/**
         * The server will not provide service to the initiating entity but is redirecting
         * traffic to another host; the server SHOULD specify the alternate hostname or IP
         * address (which MUST be a valid domain identifier) as the XML character data of the
         * &lt;see-other-host/&gt; element.
         */
        [Description("see-other-host")]
			see_other_host,

			/**
         * The server is being shut down and all active streams are being closed.
         */
        [Description("system-shutdown")]
			system_shutdown,

			/**
         * The error condition is not one of those defined by the other conditions in this
         * list; this error condition SHOULD be used only in conjunction with an
         * application-specific condition.
         */
        [Description("undefined-condition")]
			undefined_condition,

			/**
         * The initiating entity has encoded the stream in an encoding that is not
         * supported by the server.
         */
        [Description("unsupported-encoding")]
			unsupported_encoding,

			/**
         * The initiating entity has sent a first-level child of the stream that is
         * not supported by the server.
         */
        [Description("unsupported-stanza-type")]
			unsupported_stanza_type,

			/**
         * the value of the 'version' attribute provided by the initiating entity in the
         * stream header specifies a version of XMPP that is not supported by the server;
         * the server MAY specify the version(s) it supports in the &lt;text/&gt; element.
         */
        [Description("unsupported-version")]
			unsupported_version,

			/**
         * The initiating entity has sent XML that is not well-formed.
         */
        [Description("xml-not-well-formed")]
			xml_not_well_formed,

			/**
         * Converts a String value into its Condition representation.
         *
         * @param condition the String value.
         * @return the condition corresponding to the String.
         */
			public static Condition fromXMPP(String condition) {
				if (condition == null) {
					throw new NullPointerException();
				}
				condition = condition.toLowerCase();
				if (bad_format.toXMPP().equals(condition)) {
					return bad_format;
				}
				else if (bad_namespace_prefix.toXMPP().equals(condition)) {
					return bad_namespace_prefix;
				}
				else if (conflict.toXMPP().equals(condition)) {
					return conflict;
				}
				else if (connection_timeout.toXMPP().equals(condition)) {
					return connection_timeout;
				}
				else if (host_gone.toXMPP().equals(condition)) {
					return host_gone;
				}
				else if (host_unknown.toXMPP().equals(condition)) {
					return host_unknown;
				}
				else if (improper_addressing.toXMPP().equals(condition)) {
					return improper_addressing;
				}
				else if (internal_server_error.toXMPP().equals(condition)) {
					return internal_server_error;
				}
				else if (invalid_from.toXMPP().equals(condition)) {
					return invalid_from;
				}
				else if (invalid_id.toXMPP().equals(condition)) {
					return invalid_id;
				}
				else if (invalid_namespace.toXMPP().equals(condition)) {
					return invalid_namespace;
				}
				else if (invalid_xml.toXMPP().equals(condition)) {
					return invalid_xml;
				}
				else if (not_authorized.toXMPP().equals(condition)) {
					return not_authorized;
				}
				else if (policy_violation.toXMPP().equals(condition)) {
					return policy_violation;
				}
				else if (remote_connection_failed.toXMPP().equals(condition)) {
					return remote_connection_failed;
				}
				else if (resource_constraint.toXMPP().equals(condition)) {
					return resource_constraint;
				}
				else if (restricted_xml.toXMPP().equals(condition)) {
					return restricted_xml;
				}
				else if (see_other_host.toXMPP().equals(condition)) {
					return see_other_host;
				}
				else if (system_shutdown.toXMPP().equals(condition)) {
					return system_shutdown;
				}
				else if (undefined_condition.toXMPP().equals(condition)) {
					return undefined_condition;
				}
				else if (unsupported_encoding.toXMPP().equals(condition)) {
					return unsupported_encoding;
				}
				else if (unsupported_stanza_type.toXMPP().equals(condition)) {
					return unsupported_stanza_type;
				}
				else if (unsupported_version.toXMPP().equals(condition)) {
					return unsupported_version;
				}
				else if (xml_not_well_formed.toXMPP().equals(condition)) {
					return xml_not_well_formed;
				}
				else {
					throw new IllegalArgumentException("Condition invalid:" + condition);
				}
			}

			private string value;

			private Condition(String value) {
				this.value = value;
			}

			/**
         * Returns the error code as a valid XMPP error code string.
         *
         * @return the XMPP error code value.
         */
			public String toXMPP() {
				return value;
			}
	}
}

